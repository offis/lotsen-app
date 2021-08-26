using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.DataProtection;
using Moq;

namespace LotsenApp.Client.Configuration.File.Test
{
    [ExcludeFromCodeCoverage]
    public class ConfigurationFileUtilityTest: ConfigurationUtilityTest
    {
        public override IConfigurationUtility GetInstance()
        {
            var dataProtectionProvider = new Mock<IDataProtectionProvider>();
            SetupProtectionMock(dataProtectionProvider);
            return new ConfigurationFileUtility(dataProtectionProvider.Object);
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