using System;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.Configuration.Api.Test;
using LotsenApp.Client.File;
using Xunit;

namespace LotsenApp.Client.Configuration.File.Test
{
    public class FileUserConfigurationServiceTest: UserConfigurationServiceTest
    {
        private IFileService _fileService;
        
        protected override IUserConfigurationService GetInstance()
        {
            _fileService = new ScopedFileService
            {
                Root = $"./{Guid.NewGuid().ToString()}"
            };
            return new FileUserConfigurationService(_fileService);
        }
        
        [Fact]
        public void ShouldBackUpDataKeys()
        {
            var instance = GetInstance() as FileUserConfigurationService;
            Assert.NotNull(instance);
            var configuration = new UserConfiguration
            {
                UserId = "1"
            };
            instance.BackupDataKeys(configuration);
            
            Assert.True(System.IO.File.Exists(_fileService.Join("data/key-backup/1.backup")));
        }
    }
}