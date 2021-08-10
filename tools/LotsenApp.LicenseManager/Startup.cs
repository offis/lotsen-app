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

using System.Net.Http;
using LotsenApp.LicenseManager.Configuration;
using LotsenApp.LicenseManager.DependencyCrawling;
using LotsenApp.LicenseManager.DependencyCrawling.Npm;
using LotsenApp.LicenseManager.DependencyCrawling.Nuget;
using LotsenApp.LicenseManager.LicenseCreation;
using LotsenApp.LicenseManager.LicenseResolving;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LotsenApp.LicenseManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/dist"; });

            services.AddSingleton<LicenseManagerConfiguration>()
                .AddSingleton<CrawlingProcess>()
                .AddSingleton<ProjectFileCrawler>()
                .AddSingleton<IProjectCrawler, NpmCrawler>()
                .AddSingleton<IProjectCrawler, CsProjCrawler>()
                .AddSingleton<NugetCache>()
                .AddSingleton<NugetService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<LicenseResolver>()
                .AddSingleton<LicenseService>()
                .AddSingleton<ILicenseResolver, NpmLicenseResolver>()
                .AddSingleton<ILicenseResolver, NugetLicenseResolver>()
                .AddSingleton<LicenseHeaderCreator>()
                .AddSingleton<LicenseHeaderFormatter>()
                .AddSingleton<ILicenseHeaderFormatter, CSharpLicenseHeaderFormatter>()
                .AddSingleton<ILicenseHeaderFormatter, XmlLicenseHeaderFormatter>()
                .AddSingleton<ILicenseHeaderFormatter, EcmaScriptLicenseHeaderFormatter>()
                .AddSingleton<ILicenseHeaderFormatter, StylesheetLicenseHeaderFormatter>()
                .AddSingleton<ILicenseHeaderFormatter, CsProjLicenseHeaderFormatter>()
                .AddSingleton<LicenseCreator>()
                .AddTransient<XmlLicenseHeaderFormatter>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}