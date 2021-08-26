using System.Diagnostics.CodeAnalysis;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.Configuration.Api.Test;

namespace LotsenApp.Client.Configuration.Database.Test
{
    [ExcludeFromCodeCoverage]
    public class DatabaseUserConfigurationServiceTest: UserConfigurationServiceTest
    {
        protected override IUserConfigurationService GetInstance()
        {
            return new DatabaseUserConfigurationService();
        }
    }
}