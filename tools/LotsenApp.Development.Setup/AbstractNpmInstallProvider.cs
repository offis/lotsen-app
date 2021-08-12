using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LotsenApp.Tools.Common;

namespace LotsenApp.Development.Setup
{
    public abstract class AbstractNpmInstallProvider: ISetupProvider
    {
        protected virtual string WorkingDirectory => "./";

        public Task PerformSetup()
        {
            const int maxExecutionTime = 360;
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(maxExecutionTime));
            using var process = new Process();
            const int interval = 10;
            for (var i = interval; i < maxExecutionTime; i+= interval)
            {
                var j = i;
                Task.Delay(TimeSpan.FromSeconds(j), cancellationToken.Token)
                    .ContinueWith(_ =>
                {
                    Console.Write($"\rnpm install ran for {j} seconds and will be cancelled in {maxExecutionTime - j} seconds. Press 'c' to cancel the operation immediately.");
                }, cancellationToken.Token);
            }

            // ReSharper disable once MethodSupportsCancellation
            Task.Run(() =>
            {
                var input = Console.ReadKey();
                if (input.Key == ConsoleKey.C)
                {
                    Console.WriteLine("\nThe operation was cancelled by the user");
                    cancellationToken.Cancel();
                }
            });

            Console.WriteLine($"Executing 'npm install' in {WorkingDirectory}");
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var root = Helper.GetRepositoryRoot();
            var cwd = Path.Join(root.FullName, WorkingDirectory);
            var info = new ProcessStartInfo
            {
                Arguments = "/c npm install",
                FileName = "cmd.exe",
                WorkingDirectory = cwd,
                RedirectStandardOutput = true,
            };
            if (!isWindows)
            {
                info.FileName = "bash";
                info.Arguments = "-c \"npm install\"";
            }
            process.StartInfo = info;
            process.Start();
            return process.WaitForExitAsync(cancellationToken.Token);
        }
    }
}