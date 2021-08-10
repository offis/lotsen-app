using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
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
                return LoadCertificateWithThumbprint(thumbprint);
            }

            return !string.IsNullOrEmpty(file) ? LoadCertificateFromFile(file) : GenerateAndPersistCertificate(keyDirectory, password);
        }

        private static X509Certificate2 LoadCertificateWithThumbprint(string thumbprint)
        {
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
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
            return new(fileLocation);
        }

        private static X509Certificate2 GenerateAndPersistCertificate(string keyDirectory, string password)
        {
            var existingCertificate = CheckExistingCertificate(keyDirectory, password);
            if (existingCertificate != null)
            {
                return existingCertificate;
            }
            Debug.WriteLine("Creating new certificate");
            var keyGenerator = CreateKeyPairGenerator();
            var keyPair = keyGenerator.GenerateKeyPair();

            var generator = new X509V3CertificateGenerator();
            var serialNumber = BigInteger.ProbablePrime(128, new Random());
            var certificateName = new X509Name($"CN={Environment.UserName}");
            generator.SetSerialNumber(serialNumber);
            generator.SetSubjectDN(certificateName);
            generator.SetIssuerDN(certificateName);
            generator.SetNotAfter(DateTime.Now.AddYears(100));
            generator.SetNotBefore(DateTime.Now.Subtract(TimeSpan.FromDays(7)));
            generator.SetPublicKey(keyPair.Public);

            var signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", keyPair.Private);
            var certificate = generator.Generate(signatureFactory);
            return ConvertToDotnetCertificate(certificate, keyPair, keyDirectory, password);
        }

        private static IAsymmetricCipherKeyPairGenerator CreateKeyPairGenerator()
        {
            var generator = new RsaKeyPairGenerator();
            generator.Init(new KeyGenerationParameters(new SecureRandom(new CryptoApiRandomGenerator()), 4096));
            return generator;
        }

        private static X509Certificate2 ConvertToDotnetCertificate(X509Certificate certificate, AsymmetricCipherKeyPair keyPair, string keyDirectory, string password)
        {
            // Taken from https://stackoverflow.com/questions/6128541/bouncycastle-privatekey-to-x509certificate2-privatekey
            // Convert BouncyCastle X509 Certificate to .NET's X509Certificate
            var cert = DotNetUtilities.ToX509Certificate(certificate);
            var certBytes = cert.Export(X509ContentType.Pkcs12, password);

            // Convert X509Certificate to X509Certificate2
            var cert2 = new X509Certificate2(certBytes, password);

            // Convert BouncyCastle Private Key to RSA
            var rsaPrivateKey = DotNetUtilities.ToRSA(keyPair.Private as RsaPrivateCrtKeyParameters);
            
            // Set private key on our X509Certificate2
            //cert2.PrivateKey = rsaPrivateKey;
            cert2 = cert2.CopyWithPrivateKey(rsaPrivateKey);
            
            // Export Certificate with private key
            System.IO.File.WriteAllBytes(Path.Join(keyDirectory, "cert.pfx"), cert2.Export(X509ContentType.Pkcs12, password));
            return cert2;
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