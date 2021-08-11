namespace LotsenApp.Client.Electron
{
    public class EndpointConfiguration
    {
        public bool Ssl { get; set; } = false;
        public string Certificate { get; set; }
        public int? CipherBits { get; set; }
        public string CertificateStore { get; set; }
        public string CertificateStoreLocation { get; set; }
        public string Host { get; set; }
        public int? Port { get; set; }
    }
}