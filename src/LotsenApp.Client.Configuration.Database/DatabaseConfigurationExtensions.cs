using LotsenApp.Client.Configuration.Api;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;

namespace LotsenApp.Client.Configuration.Database
{
    public static class DatabaseConfigurationExtensions
    {
        public static GlobalConfigurationEntry FromConfiguration(this GlobalConfiguration configuration,
            IDataProtector protector)
        {
            var serializedConfiguration = JsonConvert.SerializeObject(configuration);
            var encryptedConfiguration = protector.Protect(serializedConfiguration);
            return new GlobalConfigurationEntry
            {
                ConfigurationId = configuration.ConfigurationId,
                Configuration = encryptedConfiguration
            };
        }

        public static GlobalConfiguration ToConfiguration(this GlobalConfigurationEntry entry, IDataProtector protector)
        {
            var unencryptedConfiguration = protector.Unprotect(entry.Configuration);
            return JsonConvert.DeserializeObject<GlobalConfiguration>(unencryptedConfiguration);
        }
        
        public static UserConfigurationEntry FromConfiguration(this UserConfiguration configuration,
            IDataProtector protector)
        {
            var serializedConfiguration = JsonConvert.SerializeObject(configuration);
            var encryptedConfiguration = protector.Protect(serializedConfiguration);
            return new UserConfigurationEntry
            {
                UserId = configuration.UserId,
                Configuration = encryptedConfiguration
            };
        }
        
        public static UserConfiguration ToConfiguration(this UserConfigurationEntry entry, IDataProtector protector)
        {
            var unencryptedConfiguration = protector.Unprotect(entry.Configuration);
            return JsonConvert.DeserializeObject<UserConfiguration>(unencryptedConfiguration);
        }
    }
}