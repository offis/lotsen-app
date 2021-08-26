using System;
using System.Diagnostics.CodeAnalysis;
using LotsenApp.Client.Configuration.Api;
using Newtonsoft.Json;
using Xunit;

namespace LotsenApp.Client.Configuration.File.Test
{
    [ExcludeFromCodeCoverage]
    public abstract class ConfigurationUtilityTest
    {
        public abstract IConfigurationUtility GetInstance();
        [Fact]
        public void ShouldReturnDefaultGlobalObjectOnNonExistentFile()
        {
            var utility = GetInstance();
            var configuration = utility.ReadGlobalConfiguration(fileName:$"{Guid.NewGuid().ToString()}.config");
            var expectedConfiguration = new GlobalConfiguration();
            configuration.ConfigurationId = expectedConfiguration.ConfigurationId; // Random guid otherwise
            var serializedConfiguration = JsonConvert.SerializeObject(configuration);
            var serializedExpectedConfiguration = JsonConvert.SerializeObject(expectedConfiguration);
            Assert.Equal(serializedExpectedConfiguration, serializedConfiguration);
        }
        
        [Fact]
        public void ShouldSaveAndReturnUpdatedGlobalObject()
        {
            var utility = GetInstance();
            var fileName = $"{Guid.NewGuid().ToString()}.config";
            var configuration = utility.ReadGlobalConfiguration(AccessMode.Write, fileName);
            configuration.DefaultLanguage = "en";
            var serializedExpectedConfiguration = JsonConvert.SerializeObject(configuration);
            utility.SaveGlobalConfiguration(configuration, fileName);
            var configurationUnderTest = utility.ReadGlobalConfiguration(fileName:fileName);
            var serializedConfiguration = JsonConvert.SerializeObject(configurationUnderTest);
            Assert.Equal(serializedExpectedConfiguration, serializedConfiguration);
        }
        
        [Fact]
        public void ShouldThrowOnInvalidGlobalLockState()
        {
            var utility = GetInstance();
            var configuration = utility.ReadGlobalConfiguration();
            Assert.Throws<InvalidOperationException>(() => utility.SaveGlobalConfiguration(configuration));
        }
        
        [Fact]
        public void ShouldReturnDefaultUserObjectOnNonExistentFile()
        {
            var utility = GetInstance();
            var userId = Guid.NewGuid().ToString("N");
            var configuration = utility.ReadUserConfiguration(userId, $"{userId}.config");
            var expectedConfiguration = new UserConfiguration();
            var serializedConfiguration = JsonConvert.SerializeObject(configuration);
            var serializedExpectedConfiguration = JsonConvert.SerializeObject(expectedConfiguration);
            Assert.Equal(serializedExpectedConfiguration, serializedConfiguration);
        }
        
        [Fact]
        public void ShouldSaveAndReturnUpdatedUserObject()
        {
            var utility = GetInstance();
            var userId = Guid.NewGuid().ToString("N");
            var userFile = $"{userId}.config";
            var configuration = utility.ReadUserConfiguration(userId, userFile, AccessMode.Write);
            configuration.DisplayConfiguration.Theme = "dark-theme";
            var serializedExpectedConfiguration = JsonConvert.SerializeObject(configuration);
            utility.SaveUserConfiguration(configuration, userId, userFile);
            var configurationUnderTest = utility.ReadUserConfiguration(userId, userFile);
            var serializedConfiguration = JsonConvert.SerializeObject(configurationUnderTest);
            Assert.Equal(serializedExpectedConfiguration, serializedConfiguration);
        }
        
        [Fact]
        public void ShouldThrowOnInvalidUserLockState()
        {
            var utility = GetInstance();
            var configuration = utility.ReadUserConfiguration("3", "3.config");
            Assert.Throws<InvalidOperationException>(() => utility.SaveUserConfiguration(configuration, "3", "3.config"));
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldThrowOnNullOrEmptyUserId(string userId)
        {
            var utility = GetInstance();
           Assert.Throws<ArgumentNullException>(() => utility.ReadUserConfiguration(userId, "3.config"));
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldThrowOnNullOrEmptyFileName(string fileName)
        {
            var utility = GetInstance();
            Assert.Throws<ArgumentNullException>(() => utility.ReadUserConfiguration("4", fileName));
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldThrowOnNullOrEmptyUserIdWhileSaving(string userId)
        {
            var utility = GetInstance();
            Assert.Throws<ArgumentNullException>(() => utility.SaveUserConfiguration(new UserConfiguration(), userId, "3.config"));
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldThrowOnNullOrEmptyFileNameWhileSaving(string fileName)
        {
            var utility = GetInstance();
            Assert.Throws<ArgumentNullException>(() => utility.SaveUserConfiguration(new UserConfiguration(), "4", fileName));
        }
    }
}