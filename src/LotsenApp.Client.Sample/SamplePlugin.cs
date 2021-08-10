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