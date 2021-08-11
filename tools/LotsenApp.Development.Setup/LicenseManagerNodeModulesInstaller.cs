namespace LotsenApp.Development.Setup
{
    public class LicenseManagerNodeModulesInstaller: AbstractNpmInstallProvider
    {
        protected override string WorkingDirectory => "tools/LotsenApp.LicenseManager/ClientApp";
    }
}