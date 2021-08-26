using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace LotsenApp.Client.Configuration.Database.Test
{
    [ExcludeFromCodeCoverage]
    public class GlobalConfigurationEntryTest
    {
        [Fact]
        public void ShouldSetValue()
        {
            var configuration = new GlobalConfigurationEntry();
            var id = Guid.NewGuid().ToString();
            configuration.ConfigurationId = id;
            
            Assert.Equal(id, configuration.ConfigurationId);
        }
        
    }
}