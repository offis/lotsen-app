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