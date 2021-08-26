using System.Diagnostics.CodeAnalysis;
using LotsenApp.Client.Configuration.Api;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LotsenApp.Client.Configuration.Database.Test
{
    [ExcludeFromCodeCoverage]
    public abstract class DatabaseConfigurationStorageTest: DatabaseConfigurationContextTest
    {
        protected override IConfigurationStorage GetInstance()
        {
            var dataProtectionProvider = new Mock<IDataProtectionProvider>();
            SetupProtectionMock(dataProtectionProvider);
            return new DatabaseConfigurationStorage(new DatabaseConfigurationContext(ContextOptions), dataProtectionProvider.Object);
        }

        protected DatabaseConfigurationStorageTest(DbContextOptions<DatabaseConfigurationContext> contextOptions) : base(contextOptions)
        {
        }
        
        private void SetupProtectionMock(Mock<IDataProtectionProvider> mock)
        {
            var protectorMock = new Mock<IDataProtector>();
            SetupDataProtector(protectorMock);
            mock.Setup(p => p.CreateProtector(It.IsAny<string>()))
                .Returns(protectorMock.Object);
        }

        private void SetupDataProtector(Mock<IDataProtector> mock)
        {
            mock.Setup(p => p.Protect(It.IsAny<byte[]>()))
                .Returns((byte[] input) => input);
            
            mock.Setup(p => p.Unprotect(It.IsAny<byte[]>()))
                .Returns((byte[] input) => input);
        }
    }
}