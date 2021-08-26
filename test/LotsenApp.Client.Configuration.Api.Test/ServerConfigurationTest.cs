using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace LotsenApp.Client.Configuration.Api.Test
{
    [ExcludeFromCodeCoverage]
    public class ServerConfigurationTest
    {
        [Fact]
        public void ShouldSetAddress()
        {
            var configuration = new ServerConfiguration();
            var address = Guid.NewGuid().ToString();
            configuration.Address = address;
            
            Assert.Equal(address, configuration.Address);
        }
        
        [Fact]
        public void ShouldSetVerificationKey()
        {
            var configuration = new ServerConfiguration();
            var verificationKey = Guid.NewGuid().ToString();
            configuration.VerificationKey = verificationKey;
            
            Assert.Equal(verificationKey, configuration.VerificationKey);
        }
    }
}