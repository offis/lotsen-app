using System;
using System.Diagnostics.CodeAnalysis;
using LotsenApp.Client.Configuration.Api;
using Xunit;

namespace LotsenApp.Client.Configuration.Rest.Test
{
    [ExcludeFromCodeCoverage]
    public class UserConfigurationDtoTest
    {
        [Fact]
        public void ShouldBeCreatableFromUserConfiguration()
        {
            var userConfiguration = new UserConfiguration
            {
                UserId = Guid.NewGuid().ToString()
            };
            var dto = new UserConfigurationDto(userConfiguration);
            
            Assert.Equal(userConfiguration.UserId, dto.UserId);
            Assert.Same(userConfiguration.DashboardConfigurations, dto.DashboardConfiguration);
            Assert.Same(userConfiguration.DisplayConfiguration, dto.DisplayConfiguration);
            Assert.Same(userConfiguration.EditorConfiguration, dto.EditorConfiguration);
            Assert.Same(userConfiguration.LocalisationConfiguration, dto.LocalisationConfiguration);
            Assert.Same(userConfiguration.NotificationConfiguration, dto.NotificationConfiguration);
            Assert.Same(userConfiguration.ProgrammeConfiguration, dto.ProgrammeConfiguration);
            Assert.Same(userConfiguration.ReminderConfiguration, dto.ReminderConfiguration);
            Assert.Same(userConfiguration.SaveConfiguration, dto.SaveConfiguration);
            Assert.Same(userConfiguration.SynchronisationConfiguration, dto.SynchronisationConfiguration);
            Assert.Same(userConfiguration.UpdateConfiguration, dto.UpdateConfiguration);
        }
        
        [Fact]
        public void ShouldPerformOneWayMerge()
        {
            var userConfiguration = new UserConfiguration();
            var dto = new UserConfigurationDto();
            dto.Merge(userConfiguration);
            
            Assert.Null(userConfiguration.DashboardConfigurations);
            Assert.Null(userConfiguration.DisplayConfiguration);
            Assert.Null(userConfiguration.EditorConfiguration);
            Assert.Null(userConfiguration.LocalisationConfiguration);
            Assert.Null(userConfiguration.NotificationConfiguration);
            Assert.Null(userConfiguration.ProgrammeConfiguration);
            Assert.Null(userConfiguration.ReminderConfiguration);
            Assert.Null(userConfiguration.SaveConfiguration);
            Assert.Null(userConfiguration.SynchronisationConfiguration);
            Assert.Null(userConfiguration.UpdateConfiguration);
        }
    }
}