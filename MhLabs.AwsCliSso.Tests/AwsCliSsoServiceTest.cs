using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MhLabs.AwsCliSso.Tests
{
    public class AwsCliSsoServiceTest
    {
        [Fact(Skip = "Enable and fill in your account information to run")]
        public async Task IntegrationTest()
        {
            var service = new AwsCliSsoService(RegionEndpoint.EUWest1);
            var startUrl = "https://your.awsapps.com/start";
            var accountId = "123456789000";
            var roleName = "ReadOnly";

            var credentials = await service.GetCredentials(startUrl, accountId, roleName);
            var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.EUWest1);

            var tables = await client.ListTablesAsync(new ListTablesRequest { Limit = 10 });

            foreach (var tableName in tables.TableNames)
            {
                Console.WriteLine(tableName);
            }
        }
    }
}
