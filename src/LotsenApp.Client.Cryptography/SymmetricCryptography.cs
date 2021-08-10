// Copyright (c) 2021 OFFIS e.V.. All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//    
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//    
// 3. Neither the name of the copyright holder nor the names of its contributors
//    may be used to endorse or promote products derived from this software without
//    specific prior written permission.
//    
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace LotsenApp.Client.Cryptography
{
    public static class SymmetricCryptography
    {
        private const int Rfc2898KeygenIterations = 1024;
        
        private const int SaltLength = 16;

        public static string CreateKey()
        {
            using var aes = Aes.Create();
            return BitConverterHelper.ToBase64String(aes?.Key);
        }

        public static string Encrypt(string text, string key)
        {
            // Use AES to encrypt stuff
            using var aes = Aes.Create();

            if (aes == null)
            {
                throw new OperationCanceledException("AES is not supported on the operating system");
            }

            var keyStrengthInBytes = aes.KeySize / 8;
            var blockSizeInBytes = aes.BlockSize / 8;
            
            // Encryption of the actual value using a random salt that differs every time a value is encrypted
            var salt = GenerateSalt();
            var rfc2898 = new Rfc2898DeriveBytes(key, salt, Rfc2898KeygenIterations);
            
            aes.Key = rfc2898.GetBytes(keyStrengthInBytes);
            aes.IV = rfc2898.GetBytes(blockSizeInBytes);

            var encryptedValue = EncryptValue(text, aes);

            // Encrypt the salt as well. Since the salt always changes, we can use a fixed, but unknown salt
            var saltEncryption = new Rfc2898DeriveBytes(key, GenerateSaltFromString(key), Rfc2898KeygenIterations);
            aes.Key = saltEncryption.GetBytes(keyStrengthInBytes);
            aes.IV = saltEncryption.GetBytes(blockSizeInBytes);

            var encryptedSalt = EncryptValue(BitConverterHelper.ToBase64String(salt), aes);
            
            return $"${encryptedSalt}${encryptedValue}";
        }

        private static string EncryptValue(string text, SymmetricAlgorithm aes)
        {
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using var writer = new StreamWriter(cs);
            writer.Write(text);
            writer.Flush();
            writer.Close();

            var encryptedTextBytes = ms.ToArray();
            return BitConverterHelper.ToBase64String(encryptedTextBytes);
        }

        public static string Decrypt(string encryptedText, string key)
        {
            var startSalt = encryptedText.IndexOf("$", StringComparison.InvariantCulture);
            var endSalt = encryptedText.LastIndexOf("$", StringComparison.InvariantCulture);
            var dollarCount = encryptedText.Count(e => e == '$');
            if (startSalt < 0 || endSalt < 0 || dollarCount > 2)
            {
                throw new ArgumentException("The text encryption cannot be recognized", nameof(encryptedText));
            }
            
            // Use AES to decrypt stuff
            using var aes = Aes.Create();

            if (aes == null)
            {
                throw new OperationCanceledException("AES is not supported on the operating system");
            }
            var keyStrengthInBytes = aes.KeySize / 8;
            var blockSizeInBytes = aes.BlockSize / 8;
            var encryptedSalt = encryptedText.Substring(startSalt + 1, endSalt - startSalt - 1);
            
            var saltEncryption = new Rfc2898DeriveBytes(key, GenerateSaltFromString(key), Rfc2898KeygenIterations);
            aes.Key = saltEncryption.GetBytes(keyStrengthInBytes);
            aes.IV = saltEncryption.GetBytes(blockSizeInBytes);

            var base64EncodedSalt = DecryptValue(encryptedSalt, aes);
            var salt = BitConverterHelper.FromBase64String(base64EncodedSalt);
            var encryptedValue = encryptedText.Substring(endSalt + 1);
            
            var encryption = new Rfc2898DeriveBytes(key, salt, Rfc2898KeygenIterations);
            aes.Key = encryption.GetBytes(keyStrengthInBytes);
            aes.IV = encryption.GetBytes(blockSizeInBytes);

            return DecryptValue(encryptedValue, aes);
        }

        private static string DecryptValue(string encryptedValue, SymmetricAlgorithm aes)
        {
            var encryptedTextBytes = BitConverterHelper.FromBase64String(encryptedValue);
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            
            using var ms = new MemoryStream(encryptedTextBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cs);
            return streamReader.ReadToEnd();
        }

        private static byte[] GenerateSalt()
        {
            using var rng = new RNGCryptoServiceProvider();
            var keyBytes = new byte[SaltLength];
            rng.GetBytes(keyBytes);
            return keyBytes;
        }

        private static byte[] GenerateSaltFromString(string key)
        {
            var bytes = BitConverterHelper.GetBytes(key);
            while (bytes.Length < SaltLength)
            {
                bytes = bytes
                    .Concat(BitConverterHelper.GetBytes(key))
                    .ToArray();
            }

            return bytes.Take(SaltLength).ToArray();
        }
    }
}
