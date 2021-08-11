using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LotsenApp.Development.Setup
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var program = new Program();
            program.CheckForToolInstallation();
            var actions = await program.GetAllActions();
            var startTime = DateTime.Now;
            foreach (var setupProvider in actions)
            {
                try
                {
                    await setupProvider.PerformSetup();
                }
                catch (TaskCanceledException)
                {
                    await Console.Error.WriteLineAsync("The operation timed out");
                }

            }



            var endTime = DateTime.Now;
            var elapsedTime = endTime - startTime;
            Console.WriteLine($"The repository has been setup after {elapsedTime}. Happy coding!");
        }
        
        Task<IEnumerable<ISetupProvider>> GetAllActions()
        {
            return Task.FromResult(GetType().Assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && t.GetInterfaces().Contains(typeof(ISetupProvider)))
                .Select(Activator.CreateInstance)
                .Cast<ISetupProvider>());
        }

        void CheckForToolInstallation()
        {
            var dotnetMissing = false;
            var dotnet = new Process();
            var dotnetInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--version",
                RedirectStandardOutput = true
            };
            dotnet.StartInfo = dotnetInfo;
            try
            {
                dotnet.Start();
                dotnet.WaitForExit();
                Console.WriteLine("dotnet is installed");
                if (dotnet.ExitCode != 0)
                {
                    dotnetMissing = true;
                    Console.WriteLine("dotnet is not installed. The tool cannot execute.");
                }
            }
            catch (Exception)
            {
                dotnetMissing = true;
                Console.WriteLine("dotnet cannot be found. The tool cannot execute.");
            }
            var nodeMissing = false;
            var node = new Process();
            var nodeInfo = new ProcessStartInfo
            {
                FileName = "node",
                Arguments = "--version",
                RedirectStandardOutput = true
            };
            node.StartInfo = nodeInfo;
            try
            {
                node.Start();
                node.WaitForExit();
                Console.WriteLine("node is installed.");
                if (node.ExitCode != 0)
                {
                    nodeMissing = true;
                    Console.WriteLine("node is not installed. The tool cannot execute.");
                }
            }
            catch (Exception)
            {
                nodeMissing = true;
                Console.WriteLine("node cannot be found. The tool cannot execute.");
            }

            if (dotnetMissing || nodeMissing)
            {
                Console.WriteLine("Exiting tool since dependencies are not satisfied.");
                Environment.Exit(1);
            }
        }
    }
}