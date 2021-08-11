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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using LotsenApp.Client.Configuration.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LotsenApp.Client.Electron
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static string Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "<unknown>";
        public static void Main(string[] args)
        {
            var index = args.ToList().FindIndex(a => a == "--mode" || a == "-m");
            if (args.Length > index + 1)
            {
                var mode = args[index + 1];
                var parsable = Enum.TryParse(mode, out ApplicationMode parsedMode);
                if (parsable)
                {
                    Startup.Mode = parsedMode;
                }
            }

            // Cannot be used since the ASP.NET Core Server is started after electron is ready
            // if (Startup.Mode == ApplicationMode.Desktop)
            // {
                // ElectronNET.API.Electron.App.CommandLine.AppendArgument("ignore-certificate-errors");            
                // ElectronNET.API.Electron.App.CommandLine.AppendSwitch("allow-insecure-localhost", "true");
            // }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls();
                    if (Startup.Mode == ApplicationMode.Desktop)
                    {
                        webBuilder.UseElectron(args);
                    }
                    
                    webBuilder.UseStartup<Startup>();
                    if (Startup.Mode != ApplicationMode.Desktop)
                    {
                        webBuilder.UseKestrel(options => options.ConfigureEndpoints());
                    }
                });
    }
}
