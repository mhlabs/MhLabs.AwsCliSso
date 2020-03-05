using Amazon.SSO.Model;

namespace MhLabs.AwsCliSso.Model
{
    public class SSOProviderCredentials
    {
        public SSOProviderCredentials()
        {
            Credentials = new SSORoleCredentials();
        }

        public SSOProviderCredentials(RoleCredentials credentials)
        {
            Credentials = new SSORoleCredentials(credentials);
        }

        public readonly string ProviderType = "sso";
        public SSORoleCredentials Credentials { get; set; }
    }
}
