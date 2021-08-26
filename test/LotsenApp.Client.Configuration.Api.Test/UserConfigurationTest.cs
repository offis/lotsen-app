using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace LotsenApp.Client.Configuration.Api.Test
{
    [ExcludeFromCodeCoverage]
    public class UserConfigurationTest
    {
        [Fact]
        public void ShouldSetReminderConfiguration()
        {
            var userConfiguration = new UserConfiguration();
            var configuration = new ReminderConfiguration();
            
            userConfiguration.ReminderConfiguration = configuration;
            
            Assert.Same(configuration, userConfiguration.ReminderConfiguration);
        }
        
        [Fact]
        public void ShouldSetDisplayConfiguration()
        {
            var userConfiguration = new UserConfiguration();
            var configuration = new DisplayConfiguration();
            
            userConfiguration.DisplayConfiguration = configuration;
            
            Assert.Same(configuration, userConfiguration.DisplayConfiguration);
        }
        
        [Fact]
        public void ShouldSetEditorConfiguration()
        {
            var userConfiguration = new UserConfiguration();
            var configuration = new EditorConfiguration();
            
            userConfiguration.EditorConfiguration = configuration;
            
            Assert.Same(configuration, userConfiguration.EditorConfiguration);
        }
        
        [Fact]
        public void ShouldSetLocalisationConfiguration()
        {
            var userConfiguration = new UserConfiguration();
            var configuration = new LocalisationConfiguration();
            
            userConfiguration.LocalisationConfiguration = configuration;
            
            Assert.Same(configuration, userConfiguration.LocalisationConfiguration);
        }
        
        [Fact]
        public void ShouldSetNotificationConfiguration()
        {
            var userConfiguration = new UserConfiguration();
            var configuration = new NotificationConfiguration();
            
            userConfiguration.NotificationConfiguration = configuration;
            
            Assert.Same(configuration, userConfiguration.NotificationConfiguration);
        }
        
        [Fact]
        public void ShouldSetSaveConfiguration()
        {
            var userConfiguration = new UserConfiguration();
            var configuration = new SaveConfiguration();
            
            userConfiguration.SaveConfiguration = configuration;
            
            Assert.Same(configuration, userConfiguration.SaveConfiguration);
        }
        
        [Fact]
        public void ShouldSetSynchronisationConfiguration()
        {
            var userConfiguration = new UserConfiguration();
            var configuration = new SynchronisationConfiguration();
            
            userConfiguration.SynchronisationConfiguration = configuration;
            
            Assert.Same(configuration, userConfiguration.SynchronisationConfiguration);
        }
        
        [Fact]
        public void ShouldSetUpdateConfiguration()
        {
            var userConfiguration = new UserConfiguration();
            var configuration = new UpdateConfiguration();
            
            userConfiguration.UpdateConfiguration = configuration;
            
            Assert.Same(configuration, userConfiguration.UpdateConfiguration);
        }
    }
}