using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.Http;

namespace LotsenApp.Client.Configuration.Rest
{
    public class UserConfigurationRestService: IUserConfigurationRestService
    {
        private readonly IConfigurationStorage _storage;
        private readonly IUserConfigurationService _configurationService;

        public UserConfigurationRestService(IConfigurationStorage storage, IUserConfigurationService configurationService)
        {
            _storage = storage;
            _configurationService = configurationService;
        }

        public async Task<UserConfigurationDto> GetUserConfiguration(string userId)
        {
            var userConfiguration = await _storage.GetConfigurationForUser(userId);

            return new UserConfigurationDto(userConfiguration);
        }
        
        public async Task SetUserConfiguration(string userId, UserConfigurationDto dto)
        {
            var userConfiguration = await _storage.GetConfigurationForUser(userId, AccessMode.Write);

            await _storage.SaveUserConfiguration(dto.Merge(userConfiguration));
        }

        public async Task<ApplicationThemeDto> GetApplicationTheme(string userId)
        {
            var globalConfiguration = await _storage.GetGlobalConfiguration();
            if (userId == null)
            {
                return new ApplicationThemeDto
                {
                    Theme = globalConfiguration.DefaultTheme
                };
            }

            var userConfiguration = await _storage.GetConfigurationForUser(userId);
            return new ApplicationThemeDto
            {
                Theme = userConfiguration.DisplayConfiguration.Theme ?? globalConfiguration.DefaultTheme

            };
        }

        public async Task SetApplicationTheme(string userId, ApplicationThemeDto dto)
        {
            var userConfiguration = await _storage.GetConfigurationForUser(userId, AccessMode.Write);
            userConfiguration.DisplayConfiguration.Theme = dto.Theme;
            await _storage.SaveUserConfiguration(userConfiguration);
        }

        public async Task<LanguageDto> GetLanguage(string userId)
        {
            var globalConfiguration = await _storage.GetGlobalConfiguration();
            if (userId == null)
            {
                var locale = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
                var supportedLanguages = new[] {"en", "de"};
                return new LanguageDto()
                {
                    Language = supportedLanguages.Contains(locale) ? locale : globalConfiguration.DefaultLanguage
                };
            }

            var userConfiguration = await _storage.GetConfigurationForUser(userId);
            return new LanguageDto
            {
                Language = userConfiguration.LocalisationConfiguration.Language ?? "de"

            };
        }

        public async Task SetLanguage(string userId, LanguageDto dto)
        {
            var userConfiguration = await _storage.GetConfigurationForUser(userId, AccessMode.Write);
            userConfiguration.LocalisationConfiguration.Language = dto.Language;
            await _storage.SaveUserConfiguration(userConfiguration);
        }

        public async Task<bool> IsFirstTime(string userId)
        {
            var userConfiguration = await _storage.GetConfigurationForUser(userId);
            return userConfiguration.FirstSignIn;
        }

        public async Task FirstTimeCompleted(string userId)
        {
            var userConfiguration = await _storage.GetConfigurationForUser(userId, AccessMode.Write);
            userConfiguration.FirstSignIn = false;
            await _storage.SaveUserConfiguration(userConfiguration);
        }

        public async Task SaveDataPassword(string userId, DataPasswordDto request)
        {
            var userConfiguration = await _storage.GetConfigurationForUser(userId, AccessMode.Write);
            if (userConfiguration.HashedDataPassword != null)
            {
                await _storage.SaveUserConfiguration(userConfiguration);
                throw new BadRequestException();
            }

            userConfiguration = _configurationService.SetDataKeys(request.DataPassword, request.RecoveryKey, userConfiguration);

            await _storage.SaveUserConfiguration(userConfiguration);
        }

        public async Task ReplaceDataPassword(string userId, ReplaceDataPasswordDto request)
        {
            var userConfiguration = await _storage.GetConfigurationForUser(userId, AccessMode.Write);
            try
            {
                _configurationService.ReplaceDataPassword(userConfiguration, request.NewDataPassword,
                    request.RecoveryKey);
            }
            catch (Exception ex)
            {
                throw new BadRequestException("Could not replace the data password", ex);
            }
            finally
            {
                await _storage.SaveUserConfiguration(userConfiguration);
            }
        }

        public async Task ReplaceRecoveryKey(string userId, ReplaceDataPasswordDto request)
        {
            var userConfiguration = await _storage.GetConfigurationForUser(userId);
            try
            {
                _configurationService.ReplaceRecoveryKey(userConfiguration, request.NewDataPassword,
                    request.RecoveryKey);
            }
            catch (Exception ex)
            {
                throw new BadRequestException("Could not replace the recovery key", ex);
            }
            finally
            {
                await _storage.SaveUserConfiguration(userConfiguration);
            }
        }

        public async Task<bool> HasDataPassword(string userId)
        {
            var userConfiguration = await _storage.GetConfigurationForUser(userId);
            return userConfiguration.HashedDataPassword != null;
        }
        
        public async Task<DashboardConfiguration[]> LoadDashboardConfiguration(string userId)
        {
            var userConfiguration = await _storage.GetConfigurationForUser(userId);
            return userConfiguration.DashboardConfigurations;
        }
        
        public async Task UpdateDashboardConfiguration(string userId, DashboardConfiguration[] newConfiguration)
        {
            var userConfiguration = await _storage.GetConfigurationForUser(userId, AccessMode.Write);
            userConfiguration.DashboardConfigurations = newConfiguration;
            await _storage.SaveUserConfiguration(userConfiguration);
        }
        
        public async Task<ProgrammeDefinition[]> LoadProgrammeConfiguration(string userId)
        {
            var userConfiguration = await _storage.GetConfigurationForUser(userId);
            return userConfiguration.ProgrammeConfiguration;
        }
        
        public async Task UpdateProgrammeConfiguration(string userId, ProgrammeDefinition[] newConfiguration)
        {
            var userConfiguration = await _storage.GetConfigurationForUser(userId, AccessMode.Write);
            userConfiguration.ProgrammeConfiguration = newConfiguration;
            await _storage.SaveUserConfiguration(userConfiguration);
        }
        
        public async Task<ReminderConfiguration> LoadReminderConfiguration(string userId)
        {
            var userConfiguration = await _storage.GetConfigurationForUser(userId);
            return userConfiguration.ReminderConfiguration;
        }
        
        public async Task UpdateReminderConfiguration(string userId, ReminderConfiguration newConfiguration)
        {
            var userConfiguration = await _storage.GetConfigurationForUser(userId, AccessMode.Write);
            userConfiguration.ReminderConfiguration = newConfiguration;
            await _storage.SaveUserConfiguration(userConfiguration);
        }
    }
}