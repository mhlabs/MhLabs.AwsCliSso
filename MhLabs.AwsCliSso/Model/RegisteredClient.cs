using System;

namespace MhLabs.AwsCliSso.Model
{
    public class RegisteredClient
    {
        public string clientId { get; set; }
        public string clientSecret { get; set; }
        public DateTime expiresAt { get; set; }
    }
}
