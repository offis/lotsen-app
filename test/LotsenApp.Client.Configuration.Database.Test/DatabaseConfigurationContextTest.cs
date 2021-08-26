using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LotsenApp.Client.Configuration.Api.Test;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LotsenApp.Client.Configuration.Database.Test
{
    [ExcludeFromCodeCoverage]
    public abstract class DatabaseConfigurationContextTest: ConfigurationStorageTest
    {
        protected DbContextOptions<DatabaseConfigurationContext> ContextOptions { get; }

        protected DatabaseConfigurationContextTest(DbContextOptions<DatabaseConfigurationContext> contextOptions)
        {
            ContextOptions = contextOptions;
            
            Seed();
        }
        
        [Fact]
        public void GlobalConfigurationShouldBeEmptyOnCreation()
        {
            using var context = new DatabaseConfigurationContext(ContextOptions);
            
            Assert.Empty(context.GlobalConfiguration);
        }
        
        [Fact]
        public void UserConfigurationShouldBeEmptyOnCreation()
        {
            using var context = new DatabaseConfigurationContext(ContextOptions);
            
            Assert.Empty(context.UserConfigurations);
        }

        [Fact]
        public async Task ShouldSaveGlobalConfiguration()
        {
            await using var context = new DatabaseConfigurationContext(ContextOptions);

            await context.GlobalConfiguration.AddAsync(new GlobalConfigurationEntry
            {
                ConfigurationId = Guid.NewGuid().ToString()
            });

            await context.SaveChangesAsync();

            Assert.Single(context.GlobalConfiguration);
        }
        
        [Fact]
        public async Task ShouldSaveUserConfiguration()
        {
            await using var context = new DatabaseConfigurationContext(ContextOptions);

            await context.UserConfigurations.AddAsync(new UserConfigurationEntry
            {
                UserId = "1"
            });

            await context.SaveChangesAsync();

            Assert.Single(context.UserConfigurations);
        }
        
        private void Seed()
        {
            using var context = new DatabaseConfigurationContext(ContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.SaveChanges();
        }
    }
}