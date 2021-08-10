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

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using Xunit;

namespace LotsenApp.Client.Cryptography.Test
{
    [ExcludeFromCodeCoverage]
    public class BitConverterTest
    {
        [Fact]
        public void ImportAndExportAreTheSame()
        {
            var rsa = RSA.Create(4096);
            var publicKey = rsa.ExportRSAPublicKey();
            var stringRepresentation = BitConverterHelper.ToBase64String(publicKey);
            var byteRepresentation = BitConverterHelper.FromBase64String(stringRepresentation);
            Assert.Equal(publicKey, byteRepresentation);
        }
        
        [Fact]
        public void ImportingPublicKey()
        {
            var rsa = RSA.Create(4096);
            var publicKey = rsa.ExportRSAPublicKey();
            var stringRepresentation = BitConverterHelper.ToBase64String(publicKey);
            var byteRepresentation = BitConverterHelper.FromBase64String(stringRepresentation);
            rsa.ImportRSAPublicKey(byteRepresentation, out var bytesRead);
            Assert.Equal(526, bytesRead);
        }
        
        [Fact]
        public void ImportingPublicKeyInDifferentRsa()
        {
            var rsa = RSA.Create(4096);
            var secondRsa = RSA.Create(4096);
            var publicKey = rsa.ExportRSAPublicKey();
            var stringRepresentation = BitConverterHelper.ToBase64String(publicKey);
            var byteRepresentation = BitConverterHelper.FromBase64String(stringRepresentation);
            // Does not throw
            secondRsa.ImportRSAPublicKey(byteRepresentation, out var bytesRead);
            Assert.Equal(526, bytesRead);
        }

        [Fact]
        public void SymmetricEncryptionOfPublicKey()
        {
            const string password = "password";
            var rsa = RSA.Create(4096);
            var publicKey = rsa.ExportRSAPublicKey();
            var publicKeyBase64 = BitConverterHelper.ToBase64String(publicKey);
            var encryptedKey = SymmetricEncryptionFunction.Encrypt(publicKeyBase64, password);
            var decryptedKey = SymmetricEncryptionFunction.Decrypt(encryptedKey, password);
            
            Assert.Equal(publicKeyBase64, decryptedKey);
        }
        
        [Fact]
        public void SymmetricEncryptionOfPrivateKey()
        {
            const string password = "password";
            var rsa = RSA.Create(4096);
            var privateKey = rsa.ExportRSAPrivateKey();
            var privateKeyBase64 = BitConverterHelper.ToBase64String(privateKey);
            var encryptedKey = SymmetricEncryptionFunction.Encrypt(privateKeyBase64, password);
            var decryptedKey = SymmetricEncryptionFunction.Decrypt(encryptedKey, password);
            
            Assert.Equal(privateKeyBase64, decryptedKey);
        }
        
        [Fact]
        public void SymmetricEncryption()
        {
            var aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;
            var valueToEncrypt = "{document:\"testDocument\"}";
            var stringLength = valueToEncrypt.Length;
            
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using var writer = new StreamWriter(cs);
            writer.Write(valueToEncrypt);
            writer.Flush();
            writer.Close();


            var encryptedTextBytes = ms.ToArray();

            using var msDecrypt = new MemoryStream(encryptedTextBytes);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(csDecrypt);
            var roundTripText = streamReader.ReadToEnd();

            Assert.Equal(valueToEncrypt, roundTripText);
        }

        [Fact]
        public void EncryptionAndDecryption()
        {
            var keyPair = PersistentAsymmetricCryptography.CreateKeyPair();
            var valueToEncrypt = "{document:\"testDocument\"}";
            var encryptedValue = PersistentAsymmetricCryptography.Encrypt(valueToEncrypt, keyPair.publicKey);
            var decryptedValue = PersistentAsymmetricCryptography.Decrypt(encryptedValue, keyPair.privateKey);
            Assert.Equal(valueToEncrypt, decryptedValue);
        }
    }
}