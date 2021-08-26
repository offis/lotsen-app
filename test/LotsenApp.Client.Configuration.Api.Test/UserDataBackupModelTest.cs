using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace LotsenApp.Client.Configuration.Api.Test
{
    [ExcludeFromCodeCoverage]
    public class UserDataBackupModelTest
    {
        [Fact]
        public void ShouldSetPrivateKeyDataPassword()
        {
            var model = new UserDataBackupModel();
            var pw = Guid.NewGuid().ToString();
            model.EncryptedPrivateKeyByDataPassword = pw;
            
            Assert.Equal(pw, model.EncryptedPrivateKeyByDataPassword);
        }
        
        [Fact]
        public void ShouldSetPrivateKeyRecoveryKey()
        {
            var model = new UserDataBackupModel();
            var pw = Guid.NewGuid().ToString();
            model.EncryptedPrivateKeyByRecoveryKey = pw;
            
            Assert.Equal(pw, model.EncryptedPrivateKeyByRecoveryKey);
        }
        
        [Fact]
        public void ShouldSetEncryptedDataKey()
        {
            var model = new UserDataBackupModel();
            var pw = Guid.NewGuid().ToString();
            model.EncryptedDataKey = pw;
            
            Assert.Equal(pw, model.EncryptedDataKey);
        }
        
        [Fact]
        public void ShouldSetHashedDataPassword()
        {
            var model = new UserDataBackupModel();
            var pw = Guid.NewGuid().ToString();
            model.HashedDataPassword = pw;
            
            Assert.Equal(pw, model.HashedDataPassword);
        }
    }
}