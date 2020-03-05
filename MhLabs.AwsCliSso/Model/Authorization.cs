using System;

namespace MhLabs.AwsCliSso.Model
{
    public class Authorization
    {
        public string startUrl { get; set; }
        public string region { get; set; }
        public string accessToken { get; set; }
        public DateTime expiresAt { get; set; }
    }
}
