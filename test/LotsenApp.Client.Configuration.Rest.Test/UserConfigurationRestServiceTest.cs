using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.Http;
using Moq;
using Xunit;

namespace LotsenApp.Client.Configuration.Rest.Test
{
    [ExcludeFromCodeCoverage]
    public class UserConfigurationRestServiceTest
    {
        protected IConfigurationStorage Storage;
        protected virtual IUserConfigurationRestService GetInstance()
        {
            return new UserConfigurationRestService(CreateStorageMock(), CreateServiceMock());
        }

        protected IConfigurationStorage CreateStorageMock()
        {
            var globalConfiguration = new GlobalConfiguration();
            var storage = new Dictionary<string, UserConfiguration>();
            var mock = new Mock<IConfigurationStorage>();
            mock.Setup(m => m.GetConfigurationForUser(It.IsAny<string>(), It.IsAny<AccessMode>()))
                .Returns((string userId, AccessMode accessMode) => Task.FromResult(storage.ContainsKey(userId) ? storage[userId] : new UserConfiguration
                {
                    UserId = userId
                }));
            mock.Setup(m => m.SaveUserConfiguration(It.IsAny<UserConfiguration>()))
                .Callback((UserConfiguration configuration) =>
                {
                    if (storage.ContainsKey(configuration.UserId))
                    {
                        storage.Remove(configuration.UserId);
                    }

                    storage.Add(configuration.UserId, configuration);
                });
            mock.Setup(m => m.GetGlobalConfiguration(It.IsAny<AccessMode>()))
                .ReturnsAsync(globalConfiguration);
            mock.Setup(m => m.SaveGlobalConfiguration(It.IsAny<GlobalConfiguration>()))
                .Callback((GlobalConfiguration configuration) =>
                {
                    globalConfiguration = configuration;
                });
            Storage = mock.Object;
            return Storage;
        }

        protected IUserConfigurationService CreateServiceMock()
        {
            var mock = new Mock<IUserConfigurationService>();
            mock.Setup(m => m.SetDataKeys(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserConfiguration>()))
                .Returns((string dataPassword, string recoveryKey, UserConfiguration configuration) =>
                {
                    configuration.HashedDataPassword = dataPassword;
                    configuration.EncryptedPrivateKeyByDataPassword = dataPassword;
                    configuration.EncryptedPrivateKeyByRecoveryKey = recoveryKey;
                    configuration.EncryptedDataKey = $"{dataPassword}{recoveryKey}";
                    return configuration;
                });
            mock.Setup(m => m.ReplaceDataPassword(It.IsAny<UserConfiguration>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((UserConfiguration configuration, string dataPassword, string recoveryKey) =>
                {
                    configuration.HashedDataPassword = dataPassword;
                    configuration.EncryptedPrivateKeyByDataPassword = dataPassword;
                    configuration.EncryptedPrivateKeyByRecoveryKey = recoveryKey;
                    configuration.EncryptedDataKey = $"{dataPassword}{recoveryKey}";
                    return configuration;
                });
            mock.Setup(m => m.ReplaceDataPassword(It.IsAny<UserConfiguration>(), "error", It.IsAny<string>()))
                .Throws<Exception>();
            mock.Setup(m => m.ReplaceRecoveryKey(It.IsAny<UserConfiguration>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((UserConfiguration configuration, string dataPassword, string recoveryKey) =>
                {
                    configuration.HashedDataPassword = dataPassword;
                    configuration.EncryptedPrivateKeyByDataPassword = dataPassword;
                    configuration.EncryptedPrivateKeyByRecoveryKey = recoveryKey;
                    configuration.EncryptedDataKey = $"{dataPassword}{recoveryKey}";
                    return configuration;
                });
            mock.Setup(m => m.ReplaceRecoveryKey(It.IsAny<UserConfiguration>(), "error", It.IsAny<string>()))
                .Throws<Exception>();
            return mock.Object;
        }

        [Fact]
        public async Task ShouldReturnUserConfigurationDto()
        {
            var instance = GetInstance();
            var userId = Guid.NewGuid().ToString();
            var dto = await instance.GetUserConfiguration(userId);
            Assert.Equal(userId, dto.UserId);
        }
        
        [Fact]
        public async Task ShouldSetUserConfigurationDto()
        {
            var instance = GetInstance();
            var userId = Guid.NewGuid().ToString();
            var dto = await instance.GetUserConfiguration(userId);

            dto.DashboardConfiguration = Array.Empty<DashboardConfiguration>();

            await instance.SetUserConfiguration(userId, dto);

            var configurationUnderTest = await instance.GetUserConfiguration(userId);
            Assert.Empty(configurationUnderTest.DashboardConfiguration);
        }

        [Fact]
        public async Task ShouldGetApplicationThemeWithoutLogin()
        {
            var instance = GetInstance();
            var theme = await instance.GetApplicationTheme(null);
            Assert.Equal("light-theme", theme.Theme);
        }
        
        [Fact]
        public async Task ShouldGetApplicationThemeWithLogin()
        {
            var instance = GetInstance();
            var userId = Guid.NewGuid().ToString();
            var userConfiguration = new UserConfiguration
            {
                UserId = userId,
                DisplayConfiguration = new DisplayConfiguration
                {
                    Theme = "dark-theme"
                }
            };
            await instance.SetUserConfiguration(userId, new UserConfigurationDto(userConfiguration));
            var theme = await instance.GetApplicationTheme(userId);
            Assert.Equal("dark-theme", theme.Theme);
        }
        
        [Fact]
        public async Task ShouldSetApplicationTheme()
        {
            var instance = GetInstance();
            var userId = Guid.NewGuid().ToString();
            var themeDto = new ApplicationThemeDto
            {
                Theme = "dark-theme"
            };
            await instance.SetApplicationTheme(userId, themeDto);
            var theme = await instance.GetApplicationTheme(userId);
            Assert.Equal(themeDto.Theme, theme.Theme);
        }

        [Fact]
        public async Task ShouldGetLanguageWithoutLogin()
        {
            var instance = GetInstance();
            var language = await instance.GetLanguage(null);
            var expectedLanguage = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            var supportedLanguages = new[] { "en", "de" };
            expectedLanguage = supportedLanguages.Contains(expectedLanguage)
                ? expectedLanguage
                : new GlobalConfiguration().DefaultLanguage;
            Assert.Equal(expectedLanguage, language.Language);
        }
        
        [Fact]
        public async Task ShouldGetLanguageWithLogin()
        {
            var instance = GetInstance();
            var userId = Guid.NewGuid().ToString();
            var userConfiguration = new UserConfiguration
            {
                UserId = userId,
                LocalisationConfiguration = new LocalisationConfiguration
                {
                    Language = "fr"
                }
            };
            await instance.SetUserConfiguration(userId, new UserConfigurationDto(userConfiguration));
            var language = await instance.GetLanguage(userId);
            Assert.Equal(userConfiguration.LocalisationConfiguration.Language, language.Language);
        }
        
        [Fact]
        public async Task ShouldSetLanguage()
        {
            var instance = GetInstance();
            var userId = Guid.NewGuid().ToString();
            var dto = new LanguageDto
            {
                Language = "fr"
            };
            await instance.SetLanguage(userId, dto);
            var language = await instance.GetLanguage(userId);
            Assert.Equal(dto.Language, language.Language);
        }

        [Fact]
        public async Task ShouldGetFirstTime()
        {
            var instance = GetInstance();
            Assert.True(await instance.IsFirstTime("1"));
        }
        
        [Fact]
        public async Task ShouldCompleteFirstTime()
        {
            var instance = GetInstance();
            await instance.FirstTimeCompleted("1");
            Assert.False(await instance.IsFirstTime("1"));
        }
        
        [Fact]
        public async Task ShouldSaveDataPassword()
        {
            var instance = GetInstance();
            var dto = new DataPasswordDto
            {
                DataPassword = Guid.NewGuid().ToString(),
                RecoveryKey = Guid.NewGuid().ToString()
            };
            await instance.SaveDataPassword("1", dto);
            Assert.True(await instance.HasDataPassword("1"));
        }
        
        [Fact]
        public async Task ShouldThrowWithExistingDataPassword()
        {
            var instance = GetInstance();
            var dto = new DataPasswordDto
            {
                DataPassword = Guid.NewGuid().ToString(),
                RecoveryKey = Guid.NewGuid().ToString()
            };
            await instance.SaveDataPassword("1", dto);
            await Assert.ThrowsAsync<BadRequestException>(async () => await instance.SaveDataPassword("1", dto));
        }
        
        [Fact]
        public async Task ShouldReplaceDataPassword()
        {
            var instance = GetInstance();
            var dto = new ReplaceDataPasswordDto
            {
                NewDataPassword = Guid.NewGuid().ToString(),
                RecoveryKey = Guid.NewGuid().ToString()
            };
            await instance.ReplaceDataPassword("1", dto);
            var configuration = await Storage.GetConfigurationForUser("1");
            Assert.Equal(configuration.HashedDataPassword, dto.NewDataPassword);
        }
        
        [Fact]
        public async Task ShouldThrowBadRequestExceptionOnFalsyDataPasswordReplacement()
        {
            var instance = GetInstance();
            var dto = new ReplaceDataPasswordDto
            {
                NewDataPassword = "error",
                RecoveryKey = Guid.NewGuid().ToString()
            };
            await Assert.ThrowsAsync<BadRequestException>(async () => await instance.ReplaceDataPassword("1", dto));
        }
        
        [Fact]
        public async Task ShouldReplaceRecoveryKey()
        {
            var instance = GetInstance();
            var dto = new ReplaceDataPasswordDto
            {
                NewDataPassword = Guid.NewGuid().ToString(),
                RecoveryKey = Guid.NewGuid().ToString()
            };
            await instance.ReplaceRecoveryKey("1", dto);
            var configuration = await Storage.GetConfigurationForUser("1");
            Assert.Equal(configuration.EncryptedPrivateKeyByRecoveryKey, dto.RecoveryKey);
        }
        
        [Fact]
        public async Task ShouldThrowBadRequestExceptionOnFalsyRecoveryKeyReplacement()
        {
            var instance = GetInstance();
            var dto = new ReplaceDataPasswordDto
            {
                NewDataPassword = "error",
                RecoveryKey = Guid.NewGuid().ToString()
            };
            await Assert.ThrowsAsync<BadRequestException>(async () => await instance.ReplaceRecoveryKey("1", dto));
        }

        [Fact]
        public async Task ShouldUpdateDashboardConfiguration()
        {
            var instance = GetInstance();
            var dto = new[]
            {
                new DashboardConfiguration()
                {
                    X = 0,
                    Y = 0,
                    Cols = 5,
                    Rows = 5,
                    LayerIndex = 10,
                    Component = DashboardComponentType.Reminder
                }
            };
            await instance.UpdateDashboardConfiguration("1", dto);
            
            Assert.Same(dto, await instance.LoadDashboardConfiguration("1"));
        }
        
        [Fact]
        public async Task ShouldUpdateProgrammeConfiguration()
        {
            var instance = GetInstance();
            var dto = new[]
            {
                new ProgrammeDefinition
                {
                    Label = "Awesome Label",
                    Path = "/path/to/executable"
                }
            };
            await instance.UpdateProgrammeConfiguration("1", dto);
            
            Assert.Same(dto, await instance.LoadProgrammeConfiguration("1"));
        }
        
        [Fact]
        public async Task ShouldUpdateReminderConfiguration()
        {
            var instance = GetInstance();
            var dto = new ReminderConfiguration
            {
                DisplayType = "random-value"
            };
            await instance.UpdateReminderConfiguration("1", dto);
            
            Assert.Same(dto, await instance.LoadReminderConfiguration("1"));
        }
    }
}