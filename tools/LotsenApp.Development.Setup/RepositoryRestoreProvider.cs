using System;
using System.Diagnostics;
using System.Threading.Tasks;
using LotsenApp.Tools.Common;

namespace LotsenApp.Development.Setup
{
    public class RepositoryRestoreProvider: ISetupProvider
    {
        public Task PerformSetup()
        {
            Console.WriteLine("Executing 'dotnet restore' in repository root");
            var root = Helper.GetRepositoryRoot();
            var process = Process.Start(new ProcessStartInfo
            {
                Arguments = "restore",
                FileName = "dotnet",
                WorkingDirectory = root.FullName,
                RedirectStandardOutput = true,
            });
            return process?.WaitForExitAsync() ?? Task.CompletedTask;
        }
    }
}