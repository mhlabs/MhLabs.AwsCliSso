using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using MhLabs.AwsCliSso.Service;

namespace MhLabs.AwsCliSso.Tests
{
    public class AwsCacheServiceTest
    {
        [Fact]
        public void InitializeCache()
        {
            var fileCache = new AwsFileCache("test");
        }
    }
}
