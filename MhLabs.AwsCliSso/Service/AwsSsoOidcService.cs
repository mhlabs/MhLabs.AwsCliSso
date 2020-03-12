using Amazon;
using Amazon.Runtime;
using Amazon.SSOOIDC;
using Amazon.SSOOIDC.Model;
using MhLabs.AwsCliSso.Helper;
using MhLabs.AwsCliSso.Model;
using System;
using System.Threading.Tasks;

namespace MhLabs.AwsCliSso.Service
{
    public class AwsSsoOidcService
    {
        private const string CLIENT_REGISTRATION_TYPE = "public";
        private const string TOKEN_GRANT_TYPE = "urn:ietf:params:oauth:grant-type:device_code";

        private readonly RegionEndpoint _region;
        private readonly AwsFileCache _awsSsoCache;
        private readonly AmazonSSOOIDCClient _SSOOIDCClient;

        public AwsSsoOidcService(RegionEndpoint region)
            : this(region, new AwsFileCache("sso", "cache"))
        {
        }

        public AwsSsoOidcService(RegionEndpoint region, AwsFileCache awsCacheService)
        {
            _region = region;
            _awsSsoCache = awsCacheService;

            _SSOOIDCClient = new AmazonSSOOIDCClient(new AnonymousAWSCredentials(), new AmazonSSOOIDCConfig
            {
                RegionEndpoint = region
            });
        }

        public async Task<RegisteredClient> GetRegistration()
        {
            var cacheKey = $"botocore-client-id-{_region.SystemName}";

            var registration = _awsSsoCache.GetItem<RegisteredClient>(cacheKey);

            if (registration.expiresAt < DateTime.Now)
            {
                var clientConfig = await _SSOOIDCClient.RegisterClientAsync(new RegisterClientRequest
                {
                    ClientName = $"botocore-client-{DateTime.Now.ToUniversalTime().Ticks}",
                    ClientType = CLIENT_REGISTRATION_TYPE
                });

                registration.clientId = clientConfig.ClientId;
                registration.clientSecret = clientConfig.ClientSecret;
                registration.expiresAt = DateTimeOffset.FromUnixTimeSeconds(clientConfig.ClientSecretExpiresAt).UtcDateTime;

                await _awsSsoCache.SetItem(cacheKey, registration);
            }
            return registration;
        }

        public async Task<Authorization> AuthorizeClient(string startUrl, RegisteredClient client)
        {
            var cacheKey = AwsFileCache.GetSha1(startUrl);
            var authorization = _awsSsoCache.GetItem<Authorization>(cacheKey);

            if (authorization.expiresAt > DateTime.Now)
            {
                return authorization;
            }

            var pendingDeviceAuthorization = await _SSOOIDCClient.StartDeviceAuthorizationAsync(new StartDeviceAuthorizationRequest
            {
                ClientId = client.clientId,
                ClientSecret = client.clientSecret,
                StartUrl = startUrl
            });

            Browser.Open(pendingDeviceAuthorization.VerificationUriComplete);

            var expiration = DateTime.UtcNow.AddSeconds(pendingDeviceAuthorization.ExpiresIn);
            var pollingInterval = TimeSpan.FromSeconds(pendingDeviceAuthorization.Interval);

            var tokenRequest = new CreateTokenRequest
            {
                ClientId = client.clientId,
                ClientSecret = client.clientSecret,
                DeviceCode = pendingDeviceAuthorization.DeviceCode,
                GrantType = TOKEN_GRANT_TYPE
            };

            CreateTokenResponse tokenResponse = default;
            while (tokenResponse == default)
            {
                try
                {
                    tokenResponse = await _SSOOIDCClient.CreateTokenAsync(tokenRequest);
                }
                catch (AuthorizationPendingException)
                when (DateTime.UtcNow.Add(pollingInterval) <= expiration)
                {
                    await Task.Delay(pollingInterval);
                }
                catch (SlowDownException)
                {
                    pollingInterval = pollingInterval.Add(TimeSpan.FromSeconds(10));
                    if (DateTime.UtcNow.Add(pollingInterval) > expiration)
                    {
                        throw;
                    }
                    await Task.Delay(pollingInterval);
                }
            }

            authorization = new Authorization
            {
                accessToken = tokenResponse.AccessToken,
                expiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                startUrl = startUrl,
                region = _region.SystemName
            };

            await _awsSsoCache.SetItem(cacheKey, authorization);

            return authorization;
        }
    }
}
