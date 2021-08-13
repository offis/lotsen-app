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