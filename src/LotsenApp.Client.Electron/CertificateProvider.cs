using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Extensions;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace LotsenApp.Client.Electron
{
    public static class CertificateProvider
    {

        public static X509Certificate2 ProvideCertificate(IConfiguration configuration, string keyDirectory)
        {
            var thumbprint = configuration["Protection:Thumbprint"];
            var file = configuration["Protection:CertificateLocation"];
            var password = configuration["Protection:Password"];
            if (!string.IsNullOrEmpty(thumbprint))
            {
                Debug.WriteLine($"Locating certificate by thumbprint");
                return LoadCertificateWithThumbprint(thumbprint, StoreName.My.GetDisplayName(), StoreLocation.CurrentUser.GetDisplayName());
            }

            return !string.IsNullOrEmpty(file) ? LoadCertificateFromFile(file) : GenerateAndPersistCertificate(keyDirectory, password);
        }
        
        public static X509Certificate2 ProvideCertificateForEndpoint(EndpointConfiguration configuration)
        {
            if (!configuration.Ssl)
            {
                return null;
            }
            // Try to load certificate from file
            var certificate = LoadCertificateFromFile(configuration.Certificate);
            if (certificate != null)
            {
                return certificate;
            }

            if (!string.IsNullOrEmpty(configuration.CertificateStore) && !string.IsNullOrEmpty(configuration.CertificateStoreLocation))
            {
                return LoadCertificateWithThumbprint(configuration.Certificate, configuration.CertificateStore, configuration.CertificateStoreLocation);                
            }

            return GenerateTransientCertificate(128, configuration.CipherBits ?? 2048);
        }

        private static X509Certificate2 LoadCertificateWithThumbprint(string thumbprint, string storeName, string storeLocation)
        {
            var store = new X509Store(Enum.Parse<StoreName>(storeName), Enum.Parse<StoreLocation>(storeLocation));
            store.Open(OpenFlags.OpenExistingOnly);
            if (!store.IsOpen)
            {
                return null;
            }
            var collection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, true);
            return collection.Count > 0 ? collection[0] : null;
        }

        private static X509Certificate2 LoadCertificateFromFile(string fileLocation)
        {
            Debug.WriteLine("Locating certificate from file");
            return !System.IO.File.Exists(fileLocation) ? null : new X509Certificate2(fileLocation);
        }

        private static X509Certificate2 GenerateTransientCertificate(int serialNumberBits = 128,
            int cipherLength = 4096, string password = null)
        {
            var keyGenerator = CreateKeyPairGenerator(cipherLength);
            var keyPair = keyGenerator.GenerateKeyPair();

            var generator = new X509V3CertificateGenerator();
            var serialNumber = BigInteger.ProbablePrime(serialNumberBits, new Random());
            var certificateName = new X509Name($"CN={Environment.UserName}");
            generator.SetSerialNumber(serialNumber);
            generator.SetSubjectDN(certificateName);
            generator.SetIssuerDN(certificateName);
            generator.SetNotAfter(DateTime.Now.AddYears(100));
            generator.SetNotBefore(DateTime.Now.Subtract(TimeSpan.FromDays(7)));
            generator.SetPublicKey(keyPair.Public);

            var signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", keyPair.Private);
            var certificate = generator.Generate(signatureFactory);
            return ConvertToDotnetCertificate(certificate, keyPair, password);
        }

        private static X509Certificate2 GenerateAndPersistCertificate(string keyDirectory, string password)
        {
            var existingCertificate = CheckExistingCertificate(keyDirectory, password);
            if (existingCertificate != null)
            {
                return existingCertificate;
            }
            Debug.WriteLine("Creating new certificate");
            var certificate = GenerateTransientCertificate(password: password);
            PersistCertificate(keyDirectory, password, certificate);
            return certificate;
        }

        private static IAsymmetricCipherKeyPairGenerator CreateKeyPairGenerator(int cipherLength = 4096)
        {
            var generator = new RsaKeyPairGenerator();
            generator.Init(new KeyGenerationParameters(new SecureRandom(new CryptoApiRandomGenerator()), cipherLength));
            return generator;
        }

        public static void PersistCertificate(string keyDirectory, string password, X509Certificate2 cert2, string name = "cert")
        {
            // Export Certificate with private key
            System.IO.File.WriteAllBytes(Path.Join(keyDirectory, $"{name}.pfx"), cert2.Export(X509ContentType.Pkcs12, password));
        }

        private static X509Certificate2 ConvertToDotnetCertificate(X509Certificate certificate, AsymmetricCipherKeyPair keyPair, string password = null)
        {
            password ??= Guid.NewGuid().ToString();
            // Taken from https://stackoverflow.com/questions/6128541/bouncycastle-privatekey-to-x509certificate2-privatekey
            // Convert BouncyCastle X509 Certificate to .NET's X509Certificate
            var cert = DotNetUtilities.ToX509Certificate(certificate);
            var certBytes = cert.Export(X509ContentType.Pkcs12, password);

            // Convert X509Certificate to X509Certificate2
            var cert2 = new X509Certificate2(certBytes, password);

            // Convert BouncyCastle Private Key to RSA
            var rsaPrivateKey = DotNetUtilities.ToRSA(keyPair.Private as RsaPrivateCrtKeyParameters);
            
            // Set private key on our X509Certificate2
            //cert2.PrivateKey = rsaPrivateKey; // Throws on .NET Core
            return cert2.CopyWithPrivateKey(rsaPrivateKey);
        }

        private static X509Certificate2 CheckExistingCertificate(string keyDirectory, string password)
        {
            var file = Path.Join(keyDirectory, "cert.pfx");
            if (!System.IO.File.Exists(file))
            {
                return null;
            }
            Debug.WriteLine($"Returning cached certificate.");
            return new X509Certificate2(file, password);
        }
    }
}