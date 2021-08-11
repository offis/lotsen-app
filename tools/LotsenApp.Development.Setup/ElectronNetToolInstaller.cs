using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using LotsenApp.Tools.Common;

namespace LotsenApp.Development.Setup
{
    public class ElectronNetToolInstaller: ISetupProvider
    {
        public Task PerformSetup()
        {
            const string workingDirectory = "src/LotsenApp.Client.Electron";
            Console.WriteLine($"Executing 'dotnet tool restore' in {workingDirectory}");
            var root = Helper.GetRepositoryRoot();
            var cwd = Path.Join(root.FullName, workingDirectory);
            var process = Process.Start(new ProcessStartInfo
            {
                Arguments = "tool restore",
                FileName = "dotnet",
                WorkingDirectory = cwd,
                RedirectStandardOutput = true,
            });
            return process?.WaitForExitAsync() ?? Task.CompletedTask;
        }
    }
}