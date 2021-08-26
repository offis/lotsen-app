using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using LotsenApp.Client.Cryptography;
using Xunit;

namespace LotsenApp.Client.Configuration.Api.Test
{
    [ExcludeFromCodeCoverage]
    public abstract class UserConfigurationServiceTest
    {
        protected abstract IUserConfigurationService GetInstance();

        
        
        [Fact]
        public void ShouldSetDataKeysAndBackThemUp()
        {
            var instance = GetInstance();
            var configuration = new UserConfiguration
            {
                UserId = "1"
            };
            var dataPassword = Guid.NewGuid().ToString();
            var recoveryKey = Guid.NewGuid().ToString();
            instance.SetDataKeys(dataPassword, recoveryKey, configuration);
            
            Assert.True(OneWayHashFunction.Verify(dataPassword, configuration.HashedDataPassword));
            var decryptedPrivateKey =
                SymmetricCryptography.Decrypt(configuration.EncryptedPrivateKeyByDataPassword, dataPassword);
            Assert.NotNull(decryptedPrivateKey);
            var decryptedPrivateKeyByRecoveryKey = SymmetricCryptography.Decrypt(configuration.EncryptedPrivateKeyByRecoveryKey, recoveryKey);
            Assert.NotNull(decryptedPrivateKeyByRecoveryKey);
            Assert.NotNull(PersistentAsymmetricCryptography.Decrypt(configuration.EncryptedDataKey, decryptedPrivateKey));
            Assert.NotNull(PersistentAsymmetricCryptography.Decrypt(configuration.EncryptedDataKey, decryptedPrivateKeyByRecoveryKey));
        }
        
        [Fact]
        public void ShouldReplaceDataPassword()
        {
            var instance = GetInstance();
            var configuration = new UserConfiguration
            {
                UserId = "1"
            };
            var dataPassword = Guid.NewGuid().ToString();
            var newDataPassword = Guid.NewGuid().ToString();
            var recoveryKey = Guid.NewGuid().ToString();
            instance.SetDataKeys(dataPassword, recoveryKey, configuration);

            instance.ReplaceDataPassword(configuration, newDataPassword, recoveryKey);
            Assert.False(OneWayHashFunction.Verify(dataPassword, configuration.HashedDataPassword));
            Assert.True(OneWayHashFunction.Verify(newDataPassword, configuration.HashedDataPassword));
        }
        
        [Fact]
        public void ShouldReplaceRecoveryKey()
        {
            var instance = GetInstance();
            var configuration = new UserConfiguration
            {
                UserId = "1"
            };
            var dataPassword = Guid.NewGuid().ToString();
            var recoveryKey = Guid.NewGuid().ToString();
            var newRecoveryKey = Guid.NewGuid().ToString();
            instance.SetDataKeys(dataPassword, recoveryKey, configuration);

            instance.ReplaceRecoveryKey(configuration, dataPassword, newRecoveryKey);

            var decryptedPrivateKey = SymmetricCryptography.Decrypt(configuration.EncryptedPrivateKeyByRecoveryKey, newRecoveryKey);
            Assert.NotNull(decryptedPrivateKey);
            Assert.Throws<CryptographicException>(() =>
                SymmetricCryptography.Decrypt(configuration.EncryptedPrivateKeyByRecoveryKey, recoveryKey));
        }
        
        [Fact]
        public void ShouldThrowOnWrongDataPassword()
        {
            var instance = GetInstance();
            var configuration = new UserConfiguration
            {
                UserId = "1"
            };
            var dataPassword = Guid.NewGuid().ToString();
            var recoveryKey = Guid.NewGuid().ToString();
            var newRecoveryKey = Guid.NewGuid().ToString();
            instance.SetDataKeys(dataPassword, recoveryKey, configuration);

            Assert.Throws<Exception>(() => instance.ReplaceRecoveryKey(configuration, recoveryKey, newRecoveryKey));
        }
    }
}