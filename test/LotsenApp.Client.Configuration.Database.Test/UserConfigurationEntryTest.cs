using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace LotsenApp.Client.Configuration.Database.Test
{
    [ExcludeFromCodeCoverage]
    public class UserConfigurationEntryTest
    {
        [Fact]
        public void ShouldSetValue()
        {
            var configuration = new UserConfigurationEntry();
            var id = Guid.NewGuid().ToString();
            configuration.UserId = id;
            
            Assert.Equal(id, configuration.UserId);
        }
        
    }
}