using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace LotsenApp.Client.Configuration.Api.Test
{
    [ExcludeFromCodeCoverage]
    public class ServerConfigurationEqualityComparerTest
    {
        [Fact]
        public void ShouldBeEqualOnEqualReference()
        {
            var serverConfiguration = new ServerConfiguration();
            var comparer = new ServerConfigurationEqualityComparer();
            
            Assert.True(comparer.Equals(serverConfiguration, serverConfiguration));
        }
        
        [Fact]
        public void ShouldNotBeEqualOnXNullComparison()
        {
            var serverConfiguration = new ServerConfiguration();
            var comparer = new ServerConfigurationEqualityComparer();
            
            Assert.False(comparer.Equals(serverConfiguration, null));
        }
        
        [Fact]
        public void ShouldNotBeEqualOnYNullComparison()
        {
            var serverConfiguration = new ServerConfiguration();
            var comparer = new ServerConfigurationEqualityComparer();
            
            Assert.False(comparer.Equals(null, serverConfiguration));
        }
        
        [Fact]
        public void ShouldNotBeEqualOnTypeDifference()
        {
            var serverConfiguration = new ServerConfiguration();
            var comparer = new ServerConfigurationEqualityComparer();
            
            Assert.False(comparer.Equals(new ServerConfigurationChild(), serverConfiguration));
        }
        
        [Fact]
        public void ShouldBeEqualOnAddressWithoutVerificationKey()
        {
            var serverConfiguration1 = new ServerConfiguration
            {
                Address = "address",
                VerificationKey = "key1"
            };
            var serverConfiguration2 = new ServerConfiguration
            {
                Address = "address",
                VerificationKey = "key2"
            };
            var comparer = new ServerConfigurationEqualityComparer();
            
            Assert.True(comparer.Equals(serverConfiguration1, serverConfiguration2));
        }
        
        [Fact]
        public void ShouldEqualHashCodeOnSameAddress()
        {
            var serverConfiguration1 = new ServerConfiguration
            {
                Address = "address",
                VerificationKey = "key1"
            };
            var serverConfiguration2 = new ServerConfiguration
            {
                Address = "address",
                VerificationKey = "key2"
            };
            var comparer = new ServerConfigurationEqualityComparer();
            
            Assert.Equal(comparer.GetHashCode(serverConfiguration1), comparer.GetHashCode(serverConfiguration2));
        }

        private class ServerConfigurationChild : ServerConfiguration
        {
            
        }
    }
}