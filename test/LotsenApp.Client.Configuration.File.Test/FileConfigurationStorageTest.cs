using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.Configuration.Api.Test;
using LotsenApp.Client.File;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace LotsenApp.Client.Configuration.File.Test
{
    [ExcludeFromCodeCoverage]
    public class FileConfigurationStorageTest: ConfigurationStorageTest
    {
        protected override IConfigurationStorage GetInstance()
        {
            var fileUtility = new Mock<IConfigurationUtility>();
            SetupFileUtilityMock(fileUtility);
            return GetInstance(fileUtility).storage;
        }
        
        private (FileConfigurationStorage storage, IFileService service) GetInstance(Mock<IConfigurationUtility> fileUtility)
        {
            fileUtility ??= new Mock<IConfigurationUtility>();
            IFileService fileService = new ScopedFileService
            {
                Root = $"./{Guid.NewGuid().ToString()}"
            };
            return (new FileConfigurationStorage(fileUtility.Object, fileService), fileService);
        }

        private void SetupFileUtilityMock(Mock<IConfigurationUtility> mock)
        {
            var globalConfigurations = new Dictionary<string, GlobalConfiguration>();
            var userConfigurations = new Dictionary<string, UserConfiguration>();
            
            mock.Setup(m => m.ReadGlobalConfiguration(It.IsAny<AccessMode>(), It.IsAny<string>()))
                .Returns((AccessMode mode, string fileName) => globalConfigurations.ContainsKey(fileName)
                    ? globalConfigurations[fileName]
                    : new GlobalConfiguration());
            mock.Setup(m => m.SaveGlobalConfiguration(It.IsAny<GlobalConfiguration>(), It.IsAny<string>()))
                .Callback((GlobalConfiguration conf, string fileName) =>
                {
                    if (globalConfigurations.ContainsKey(fileName))
                    {
                        globalConfigurations.Remove(fileName);
                    }
                    globalConfigurations.Add(fileName, conf);
                });
            
            mock.Setup(m => m.ReadUserConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AccessMode>()))
                .Returns((string userId, string fileName, AccessMode accessMode) =>
                {
                    var key = $"{userId}{fileName}";
                    return userConfigurations.ContainsKey(key) ? userConfigurations[key] : new UserConfiguration();
                });
            mock.Setup(m =>
                    m.SaveUserConfiguration(It.IsAny<UserConfiguration>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((UserConfiguration configuration, string userId, string fileName) =>
                {
                    var key = $"{userId}{fileName}";
                    if (userConfigurations.ContainsKey(key))
                    {
                        userConfigurations.Remove(key);
                    }
                    userConfigurations.Add(key, configuration);
                });
        }

        [Theory]
        [InlineData(AccessMode.Read)]
        [InlineData(AccessMode.Write)]
        public async Task ShouldExitLockAndReturnDefaultConfigurationOnException(AccessMode mode)
        {
            var utilityMock = new Mock<IConfigurationUtility>();
            utilityMock.Setup(m => m.ReadGlobalConfiguration(It.IsAny<AccessMode>(), It.IsAny<string>()))
                .Returns((AccessMode accessMode, string fileName) =>
                {
                    var lockSlim = ConcurrentFileAccessHelper.GetAccessor(fileName);
                    if (accessMode == AccessMode.Read)
                    {
                        lockSlim.EnterReadLock();
                    }
                    else
                    {
                        lockSlim.EnterWriteLock();
                    }

                    throw new InvalidOperationException();
                });
            var (instance, service) = GetInstance(utilityMock);

            var configurationUnderTest = await instance.GetGlobalConfiguration(mode);
            var defaultConfiguration = new GlobalConfiguration();
            configurationUnderTest.ConfigurationId = defaultConfiguration.ConfigurationId;
            var serializedConfiguration = JsonConvert.SerializeObject(defaultConfiguration);
            var testConfiguration = JsonConvert.SerializeObject(configurationUnderTest);
            var lockSlim = ConcurrentFileAccessHelper.GetAccessor(service.Join(ConfigurationConstants.GlobalConfigurationFile));
            
            Assert.False(mode == AccessMode.Read ? lockSlim.IsReadLockHeld : lockSlim.IsWriteLockHeld);
            Assert.Equal(serializedConfiguration, testConfiguration);
        }
    }
}