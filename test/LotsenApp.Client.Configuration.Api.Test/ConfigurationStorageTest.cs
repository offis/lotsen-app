using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace LotsenApp.Client.Configuration.Api.Test
{
    [ExcludeFromCodeCoverage]
    public abstract class ConfigurationStorageTest
    {
        protected abstract IConfigurationStorage GetInstance();
        
        [Fact]
        public async Task ShouldGetDefaultGlobalConfiguration()
        {
            var storage = GetInstance();
            var configuration = await storage.GetGlobalConfiguration();
            var expectedConfiguration = new GlobalConfiguration();
            configuration.ConfigurationId = expectedConfiguration.ConfigurationId;
            var serializedConfiguration = JsonConvert.SerializeObject(configuration);
            var serializedExpectedConfiguration = JsonConvert.SerializeObject(expectedConfiguration);
            Assert.Equal(serializedExpectedConfiguration, serializedConfiguration);
        }
        
        [Fact]
        public async Task ShouldSaveAndReturnUpdatedGlobalObject()
        {
            var utility = GetInstance();
            var configuration = await utility.GetGlobalConfiguration(AccessMode.Write);
            configuration.DefaultTheme = "dark-theme";
            var serializedExpectedConfiguration = JsonConvert.SerializeObject(configuration);
            await utility.SaveGlobalConfiguration(configuration);
            var configurationUnderTest = await utility.GetGlobalConfiguration();
            var serializedConfiguration = JsonConvert.SerializeObject(configurationUnderTest);
            Assert.Equal(serializedExpectedConfiguration, serializedConfiguration);
        }
        
        [Fact]
        public async Task ShouldReturnDefaultUserObjectOnNonExistentFile()
        {
            var utility = GetInstance();
            var userId = Guid.NewGuid().ToString("N");
            var configuration = await utility.GetConfigurationForUser(userId);
            var expectedConfiguration = new UserConfiguration
            {
                UserId = userId
            };
            var serializedConfiguration = JsonConvert.SerializeObject(configuration);
            var serializedExpectedConfiguration = JsonConvert.SerializeObject(expectedConfiguration);
            Assert.Equal(serializedExpectedConfiguration, serializedConfiguration);
        }
        
        [Fact]
        public async Task ShouldSaveAndReturnUpdatedUserObject()
        {
            var utility = GetInstance();
            var userId = Guid.NewGuid().ToString("N");
            var configuration = await utility.GetConfigurationForUser(userId, AccessMode.Write);
            configuration.DisplayConfiguration.Theme = "dark-theme";
            var serializedExpectedConfiguration = JsonConvert.SerializeObject(configuration);
            await utility.SaveUserConfiguration(configuration);
            var configurationUnderTest = await utility.GetConfigurationForUser(userId);
            var serializedConfiguration = JsonConvert.SerializeObject(configurationUnderTest);
            Assert.Equal(serializedExpectedConfiguration, serializedConfiguration);
        }
    }
}