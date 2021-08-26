using LotsenApp.Client.Configuration.Api;

namespace LotsenApp.Client.Configuration.File
{
    public interface IConfigurationUtility
    {
        public UserConfiguration ReadUserConfiguration(string userId, string fileName,
            AccessMode accessMode = AccessMode.Read);

        public void SaveUserConfiguration(UserConfiguration configuration, string userId, string fileName);

        public GlobalConfiguration ReadGlobalConfiguration(AccessMode accessMode = AccessMode.Read, string fileName = ConfigurationConstants.GlobalConfigurationFile);

        public void SaveGlobalConfiguration(GlobalConfiguration configuration, string fileName = ConfigurationConstants.GlobalConfigurationFile);
    }
}