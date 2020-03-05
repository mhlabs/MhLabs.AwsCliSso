using Amazon.Runtime;
using Amazon.SSO.Model;
using System;

namespace MhLabs.AwsCliSso.Model
{
    public class SSORoleCredentials
    {
        public SSORoleCredentials() { }

        public SSORoleCredentials(RoleCredentials credentials)
        {
            AccessKeyId = credentials.AccessKeyId;
            SecretAccessKey = credentials.SecretAccessKey;
            SessionToken = credentials.SessionToken;
            Expiration = DateTimeOffset.FromUnixTimeMilliseconds(credentials.Expiration).UtcDateTime;
        }

        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        public string SessionToken { get; set; }
        public DateTime Expiration { get; set; }

        public SessionAWSCredentials ToSessionAWSCredentials()
        {
            return new SessionAWSCredentials(
                AccessKeyId,
                SecretAccessKey,
                SessionToken
            );
        }
    }
}
