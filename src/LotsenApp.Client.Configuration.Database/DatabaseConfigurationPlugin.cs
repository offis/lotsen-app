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
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.File;
using LotsenApp.Client.Plugin;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LotsenApp.Client.Configuration.Database
{
    [PluginDefault(AutomaticallyActivate = false)]
    public class DatabaseConfigurationPlugin: ILotsenAppPlugin
    {
        public void ConfigurePlugin(IServiceCollection collection, IMvcBuilder builder)
        {
            collection.AddScoped<IConfigurationStorage, DatabaseConfigurationStorage>();

            builder.AddApplicationPart(GetType().Assembly);
        }
        
        public void AfterConfiguration(IServiceCollection collection, IServiceProvider serviceProvider)
        {
            var fileService = serviceProvider.GetService<IFileService>();
            collection.AddDbContext<DatabaseConfigurationContext>(options => options.UseDatabase(fileService));
        }

        public void ActivatePlugin(IApplicationBuilder app, IHostEnvironment env)
        {
            CreateDatabase(app);
        }

        private void CreateDatabase(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var context = scope.ServiceProvider.GetService<DatabaseConfigurationContext>();
            context?.Database.Migrate();
        }
    }
}