namespace LotsenApp.Development.Setup
{
    public class LotsenAppNodeModulesInstaller: AbstractNpmInstallProvider
    {
        protected override string WorkingDirectory => "src/LotsenApp.Client.Electron/ClientApp";
    }
}