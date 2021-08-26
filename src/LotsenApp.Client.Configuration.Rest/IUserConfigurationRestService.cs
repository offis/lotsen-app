using System.Threading.Tasks;
using LotsenApp.Client.Configuration.Api;
using Microsoft.AspNetCore.Mvc;

namespace LotsenApp.Client.Configuration.Rest
{
    public interface IUserConfigurationRestService
    {
        public Task<UserConfigurationDto> GetUserConfiguration(string userId);

        public Task SetUserConfiguration(string userId, UserConfigurationDto dto);

        public Task<ApplicationThemeDto> GetApplicationTheme(string userId);

        public Task SetApplicationTheme(string userId, ApplicationThemeDto dto);

        public Task<LanguageDto> GetLanguage(string userId);
        
        public Task SetLanguage(string userId, LanguageDto dto);
        
        public Task<bool> IsFirstTime(string userId);

        public Task FirstTimeCompleted(string userId);

        public Task SaveDataPassword(string userId, DataPasswordDto request);

        public Task ReplaceDataPassword(string userId, ReplaceDataPasswordDto request);

        public Task ReplaceRecoveryKey(string userId, ReplaceDataPasswordDto request);

        public Task<bool> HasDataPassword(string userId);

        public Task<DashboardConfiguration[]> LoadDashboardConfiguration(string userId);

        public Task UpdateDashboardConfiguration(string userId, DashboardConfiguration[] newConfiguration);

        public Task<ProgrammeDefinition[]> LoadProgrammeConfiguration(string userId);

        public Task UpdateProgrammeConfiguration(string userId, ProgrammeDefinition[] newConfiguration);

        public Task<ReminderConfiguration> LoadReminderConfiguration(string userId);

        public Task UpdateReminderConfiguration(string userId, ReminderConfiguration newConfiguration);
    }
}