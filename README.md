# MhLabs.AwsCliSso
Simplify usage of Aws SSO in your local projects

# Example usage
```csharp
    var service = new AwsCliSsoService(RegionEndpoint.EUWest1);
    var startUrl = "https://your.awsapps.com/start";
    var accountId = "123456789012";
    var roleName = "ReadOnly";

    var credentials = await service.GetCredentials(startUrl, accountId, roleName);
    var client = new Amazon.DynamoDBv2.AmazonDynamoDBClient(credentials, RegionEndpoint.EUWest1);

    var tables = await client.ListTablesAsync();
```