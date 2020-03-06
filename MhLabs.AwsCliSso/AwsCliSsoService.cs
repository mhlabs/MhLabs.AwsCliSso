using Amazon;
using Amazon.Runtime;
using Amazon.SSO.Model;
using MhLabs.AwsCliSso.Model;
using MhLabs.AwsCliSso.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MhLabs.AwsCliSso
{
    public class AwsCliSsoService
    {
        private readonly RegionEndpoint _region;
        private readonly AwsFileCache _awsCliCache;
        private readonly AwsSsoOidcService _awsSsoOidc;

        public AwsCliSsoService(RegionEndpoint region)
            : this(region, new AwsFileCache("cli", "cache"))
        {
        }

        public AwsCliSsoService(RegionEndpoint region, AwsFileCache cliCache)
        {
            _region = region;
            _awsCliCache = cliCache;

            _awsSsoOidc = new AwsSsoOidcService(region);
        }

        public AwsCliSsoService(RegionEndpoint region, AwsFileCache cliCache, AwsFileCache oidcCache)
        {
            _region = region;
            _awsCliCache = cliCache;

            _awsSsoOidc = new AwsSsoOidcService(region, oidcCache);
        }

        private string GetCliCompatibleCacheKey(string startUrl, string roleName, string accountId)
        {
            // These properties needs to be added in alphabetical order to match the cli.
            // Preferably this would be achieved another way.
            var profileConfig = new Dictionary<string, string>
            {
                { "accountId", accountId },
                { "roleName", roleName },
                { "startUrl", startUrl },
            };
            return AwsFileCache.GetSha1(JsonConvert.SerializeObject(profileConfig, Formatting.None));
        }

        public async Task<AWSCredentials> GetCredentials(string startUrl, string accountId, string roleName)
        {
            var cacheKey = GetCliCompatibleCacheKey(startUrl, roleName, accountId);
            var providerCredentials = _awsCliCache.GetItem<SSOProviderCredentials>(cacheKey);

            if (providerCredentials.Credentials.Expiration > DateTime.UtcNow)
            {
                return providerCredentials.Credentials.ToSessionAWSCredentials();
            }

            var client = await _awsSsoOidc.GetRegistration();

            var authorization = await _awsSsoOidc.AuthorizeClient(startUrl, client);

            using var ssoClient = new Amazon.SSO.AmazonSSOClient(_region);

            var creds = await ssoClient.GetRoleCredentialsAsync(new GetRoleCredentialsRequest
            {
                AccessToken = authorization.accessToken,
                AccountId = accountId,
                RoleName = roleName
            });

            await _awsCliCache.SetItem(cacheKey, new SSOProviderCredentials(creds.RoleCredentials));

            return new SessionAWSCredentials(creds.RoleCredentials.AccessKeyId, creds.RoleCredentials.SecretAccessKey, creds.RoleCredentials.SessionToken);
        }
    }
}
