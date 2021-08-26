using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LotsenApp.Client.Configuration.Api;
using Moq;
using Xunit;

namespace LotsenApp.Client.Configuration.Rest.Test
{
    public class GlobalConfigurationControllerTest
    {
        private GlobalConfigurationController CreateInstance()
        {
            var storageMock = new Mock<IConfigurationStorage>();
            storageMock.Setup(m => m.GetGlobalConfiguration(It.IsAny<AccessMode>()))
                .Returns(Task.FromResult(new GlobalConfiguration
                {
                    KnownServers = new List<ServerConfiguration>
                    {
                        new ()
                        {
                            Address = "1"
                        },
                    }
                }));
            return new GlobalConfigurationController(storageMock.Object);
        }
        
        [Fact]
        public async Task ShouldReturnApplicationMode()
        {
            var instance = CreateInstance();
            var mode = await instance.GetApplicationMode();
            var configuration = new GlobalConfiguration();
            
            Assert.Equal(configuration.ApplicationMode, mode);
        }
        
        [Fact]
        public async Task ShouldReturnKnownServer()
        {
            var instance = CreateInstance();
            var knownServer = await instance.GetKnownServer();
            
            Assert.Single(knownServer);
        }
        
        [Fact]
        public void ShouldGetCpuCount()
        {
            var instance = CreateInstance();
            var cpuCount = instance.GetCpuCount();
            
            Assert.Equal(Environment.ProcessorCount, cpuCount);
        }
    }
}