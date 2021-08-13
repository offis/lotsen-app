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
using System.IO;
using System.Reflection;
using LotsenApp.Client.DataFormat;
using LotsenApp.Client.Plugin;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace LotsenApp.Client.Sample
{
    public class SamplePlugin: ILotsenAppPlugin
    {
        public void ConfigurePlugin(IServiceCollection collection, IMvcBuilder builder)
        {
            
        }

        public void ActivatePlugin(IApplicationBuilder app, IHostEnvironment env)
        {
            var projectStorage = app.ApplicationServices.GetService<IDataFormatStorage>();

            var assembly = Assembly.GetAssembly(typeof(SamplePlugin));
            var projectStream = assembly?.GetManifestResourceStream(
                "LotsenApp.Client.Sample.Assets.sports.json");
            // var i18NDeStream = assembly?.GetManifestResourceStream(
            //    "LotsenApp.Client.Sample.Assets.a163d5f4-65dc-451b-a776-ee2329f0f69e_de.json");
            // var i18NEnStream = assembly?.GetManifestResourceStream(
            //    "LotsenApp.Client.DataFormat.StrokeOwl.Assets.a163d5f4-65dc-451b-a776-ee2329f0f69e_en.json");
            // || i18NEnStream == null
            if (projectStream == null || /*i18NDeStream == null ||*/ projectStorage == null)
            {
                throw new Exception("Internal Data may not be null");
            }
            
            using var projectStreamReader = new StreamReader(projectStream);
            // using var i18NDeStreamReader = new StreamReader(i18NDeStream);
            // using var i18NEnStreamReader = new StreamReader(i18NEnStream);
            var serializedProject = projectStreamReader.ReadToEnd();
            var project = JsonConvert.DeserializeObject<Project>(serializedProject);
            // var i18NDe = i18NDeStreamReader.ReadToEnd();
            // var i18NEn = i18NEnStreamReader.ReadToEnd();
            
            projectStorage.CreateProject(project)/*.ContinueWith(task =>
            {
                projectStorage.AddI18N(project, "de", i18NDe, task.Result);
                // projectStorage.AddI18N(project, "en", i18NEn);                
            })*/;
        }
    }
}