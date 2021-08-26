using System;
using LotsenApp.Client.Cryptography;

namespace LotsenApp.Client.Configuration.Api
{
    public interface IUserConfigurationService
    {
        protected static UserConfiguration SetDataKeysDefault(string dataPassword, string recoveryKey, UserConfiguration userConfiguration)
        {
            var hashedPassword = OneWayHashFunction.Hash(dataPassword);
            var (publicKey, privateKey) = PersistentAsymmetricCryptography.CreateKeyPair();
            var dataKey = SymmetricCryptography.CreateKey();
            var encryptedPrivateKeyByDataPassword = SymmetricCryptography.Encrypt(privateKey, dataPassword);
            var encryptedPrivateKeyByRecoveryKey = SymmetricCryptography.Encrypt(privateKey, recoveryKey);
            var encryptedDataKey = PersistentAsymmetricCryptography.Encrypt(dataKey, publicKey);
            
            userConfiguration.HashedDataPassword = hashedPassword;
            userConfiguration.EncryptedPrivateKeyByDataPassword = encryptedPrivateKeyByDataPassword;
            userConfiguration.EncryptedPrivateKeyByRecoveryKey = encryptedPrivateKeyByRecoveryKey;
            userConfiguration.EncryptedDataKey = encryptedDataKey;

            return userConfiguration;
        }
        
        protected static UserConfiguration ReplaceDataPasswordDefault(UserConfiguration userConfiguration,
            string newDataPassword, string recoveryKey)
        {
            var hashedPassword = OneWayHashFunction.Hash(newDataPassword);
            var encryptedPrivateKey = userConfiguration.EncryptedPrivateKeyByRecoveryKey;
            var privateKey = SymmetricCryptography.Decrypt(encryptedPrivateKey, recoveryKey);

            var encryptedPrivateKeyByDataPassword = SymmetricCryptography.Encrypt(privateKey, newDataPassword);

            userConfiguration.HashedDataPassword = hashedPassword;
            userConfiguration.EncryptedPrivateKeyByDataPassword = encryptedPrivateKeyByDataPassword;
            return userConfiguration;
        }
        
        protected static UserConfiguration ReplaceRecoveryKeyDefault(UserConfiguration userConfiguration,
            string dataPassword, string newRecoveryKey)
        {

            if (userConfiguration == null || !OneWayHashFunction.Verify(dataPassword, userConfiguration.HashedDataPassword))
            {
                throw new Exception("No configuration exists or the data password was invalid");
            }

            var encryptedPrivateKey = userConfiguration.EncryptedPrivateKeyByDataPassword;
            var privateKey = SymmetricCryptography.Decrypt(encryptedPrivateKey, dataPassword);

            var encryptedPrivateKeyByRecoveryKey = SymmetricCryptography.Encrypt(privateKey, newRecoveryKey);
            userConfiguration.EncryptedPrivateKeyByRecoveryKey = encryptedPrivateKeyByRecoveryKey;
            
            return userConfiguration;
        }

        public UserConfiguration SetDataKeys(string dataPassword, string recoveryKey,
            UserConfiguration userConfiguration) => SetDataKeysDefault(dataPassword, recoveryKey, userConfiguration);

        public UserConfiguration ReplaceDataPassword(UserConfiguration userConfiguration,
            string newDataPassword, string recoveryKey) =>
            ReplaceDataPasswordDefault(userConfiguration, newDataPassword, recoveryKey);

        public UserConfiguration ReplaceRecoveryKey(UserConfiguration userConfiguration,
            string dataPassword, string newRecoveryKey) =>
            ReplaceRecoveryKeyDefault(userConfiguration, dataPassword, newRecoveryKey);
    }
}