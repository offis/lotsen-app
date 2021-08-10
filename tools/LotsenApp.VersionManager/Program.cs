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
using System.Linq;
using System.Threading.Tasks;
using CommandLine;

namespace LotsenApp.VersionManager
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var program = new Program();
            var configuration = await program.ParseInput(args);
            var actions = await program.GetAllActions();
            var actionToExecute = actions.FirstOrDefault(a => a.Provide == configuration.Action);
            if (actionToExecute == null)
            {
                Console.WriteLine($"The action '{configuration.Action}' is not known");
                return;
            }
            await actionToExecute.Execute(configuration);
        }

        async Task<CommandLineConfiguration> ParseInput(string[] args)
        {
            var parser = new Parser(s => { s.IgnoreUnknownArguments = true; });
            CommandLineConfiguration configuration = new CommandLineConfiguration();
            var result = await parser.ParseArguments<CommandLineConfiguration>(args).WithParsedAsync(c =>
            {
                configuration = c;
                return Task.CompletedTask;
            });
            var i = 0;
            result.WithNotParsed(errors => errors
                .ToList()
                .ForEach(e =>
            {
                Console.WriteLine($"{i}: {e.Tag} {e.StopsProcessing} {((NamedError) e).NameInfo.LongName}");
                ++i;
            }));
            return configuration;
        }

        Task<IEnumerable<IAction>> GetAllActions()
        {
            return Task.FromResult(GetType().Assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && t.GetInterfaces().Contains(typeof(IAction)))
                .Select(Activator.CreateInstance)
                .Cast<IAction>());
        }
    }
}