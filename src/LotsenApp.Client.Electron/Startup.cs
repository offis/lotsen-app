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
using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API.Entities;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.Electron.Hooks;
using LotsenApp.Client.File;
using LotsenApp.Client.Plugin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LotsenApp.Client.Electron
{
    public class Startup
    {
        private ILotsenAppPlugin[] _plugins;

        public static ApplicationMode Mode = ApplicationMode.Desktop;

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mvcBuilder = services.AddMvc(option => option.EnableEndpointRouting = false);
            services.AddLogging();

            services.AddSingleton<PluginManager>();
            var providerFactory = new DefaultServiceProviderFactory();
            var serviceProvider = providerFactory.CreateServiceProvider(services);

            #region PluginInitialization

            var pluginManager = serviceProvider.GetService<PluginManager>();
            var logger = serviceProvider.GetService<ILogger<PluginManager>>();
            if (pluginManager != null)
            {
#if DEBUG
                if (Mode == ApplicationMode.Server)
                {
                    pluginManager.PluginFilePath = Guid.NewGuid().ToString();
                }

                _plugins = pluginManager.DiscoverPlugins(true);
#else
                _plugins = pluginManager.DiscoverPlugins();
#endif
                foreach (var plugin in _plugins)
                {
                    logger.LogDebug(
                        $"Configuring plugin {plugin.GetType().Name}[{plugin.GetType().Assembly.GetName().Version}]");
                    plugin.ConfigurePlugin(services, mvcBuilder);
                }
            }

            #endregion

            serviceProvider = providerFactory.CreateServiceProvider(services);
            logger.LogDebug("Finished plugin configuration");
            
            // Add information about the data store using electron to the file service
            var fileService = serviceProvider.GetService<IFileService>();
            if (fileService == null)
            {
                logger.LogError("There is no instance of the FileService available");
                throw new Exception("The FileService must not be null");
            }

            Task.WaitAll(new[]
                {
                    Task.Run(async () =>
                    {
                        if (Mode != ApplicationMode.Desktop)
                        {
                            return;
                        }
                        logger.LogDebug("Receiving data directory from electron");
                        var rootPath = await ElectronNET.API.Electron.App.GetPathAsync(PathName.UserData);
                        fileService.Root = rootPath;
                        logger.LogDebug($"Data will be stored in {rootPath}");
                    })
                }, TimeSpan.FromSeconds(10));
            logger.LogDebug("Setting up application encryption");
            // Use Root-Path
            var keyDirectory = fileService.Join("config/keys");
            Directory.CreateDirectory(keyDirectory);
            try
            {
                services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(keyDirectory))
                    .ProtectKeysWithCertificate(CertificateProvider.ProvideCertificate(Configuration, keyDirectory));
            }
            catch (Exception ex)
            {
                logger.LogError("There was an error during application encryption. The encryption keys are unencrypted. {ex}", ex);
            }

            logger.LogDebug("Application encryption is done");
            
            Task.WaitAll(new[]
            {
                Task.Run(async () =>
                {
                    logger.LogDebug("Trying to set application mode in global configuration");
                    var configurationStorage = serviceProvider.GetService<IConfigurationStorage>();
                    if (configurationStorage == null)
                    {
                        return;
                    }
                    var globalConfiguration = await configurationStorage.GetGlobalConfiguration(AccessMode.Write);
                    globalConfiguration.ApplicationMode = Mode == ApplicationMode.Desktop
                        ? Client.Configuration.ApplicationMode.Offline
                        : Client.Configuration.ApplicationMode.Online;
                    await configurationStorage.SaveGlobalConfiguration(globalConfiguration);
                    logger.LogDebug($"Application mode: {globalConfiguration.ApplicationMode}");
                })
            }, TimeSpan.FromSeconds(10));

            services
                .AddTransient<IElectronHook, CloseApplicationHook>()
                .AddTransient<IElectronHook, DeveloperToolsHook>()
                .AddTransient<IElectronHook, ForceReloadHook>()
                .AddTransient<IElectronHook, MaximizeHook>()
                .AddTransient<IElectronHook, PrintHook>()
                .AddTransient<IElectronHook, UpdateHook>()
                .AddTransient<IElectronHook, SaveFileDialogHook>()
                .AddTransient<IElectronHook, VersionHook>();

            // Execute the AfterConfiguration method for the plugins
            foreach (var plugin in _plugins)
            {
                logger.LogDebug(
                    $"AfterConfiguration for plugin {plugin.GetType().Name}[{plugin.GetType().Assembly.GetName().Version}]");
                plugin.AfterConfiguration(services, serviceProvider);
            }

            services.AddControllers(options =>
            {
                var outputServiceProvider = providerFactory.CreateServiceProvider(services);
                var outputLogger = serviceProvider.GetService<ILogger<Startup>>();
                var outputFormatter = outputServiceProvider.GetServices<TextOutputFormatter>();
                foreach (var textOutputFormatter in outputFormatter)
                {
                    outputLogger.LogDebug($"Adding {textOutputFormatter.GetType().Name} as OutputFormatter");
                    options.OutputFormatters.Insert(0, textOutputFormatter);
                }
            });

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist/lotsen-app-view/";
            });
            logger.LogInformation("Finished application setup for the execution mode.");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
        {
            var fileService = app.ApplicationServices.GetService<IFileService>();
            const long mb4 = 4194304; // 4 MB
            factory.AddFile(fileService?.Join("logs/la2-{Date}.log") ?? "logs/la2-{Date}.log", fileSizeLimitBytes: mb4,
                retainedFileCountLimit: 7,
                outputTemplate:
                "[{Timestamp:o}][{RequestId,13}][{Level:u3}] {Message} ({EventId:x8}){NewLine}{Exception}");
            var startupLogger = factory.CreateLogger(typeof(Startup));
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

            #region Plugin Activation

            var logger = factory.CreateLogger(typeof(PluginManager));
            foreach (var plugin in _plugins)
            {
                logger.LogDebug(
                    $"Activating plugin {plugin.GetType().Name}[{plugin.GetType().Assembly.GetName().Version}]");
                plugin.ActivatePlugin(app, env);
            }

            #endregion

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";
                // spa.Options.DefaultPage = $"/{_language}/index.html";
                if (!env.IsDevelopment())
                {
                    return;
                }

                
                var integrateProcess = Configuration.GetValue<bool>("IntegrateFrontendDevelopmentProcess");
                Console.WriteLine($"Integrating frontend development process: {integrateProcess}");
                
                if (integrateProcess)
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
                else
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");      
                }

            });
            logger.LogInformation("Finished application configuration for the server environment");
            if (Mode == ApplicationMode.Server)
            {
                return;
            }

            Task.Run(async () =>
            {
                var preloadPath = Path.Join(Environment.CurrentDirectory, "./Assets/preload.js");
                startupLogger.LogInformation("Loading preload from " + preloadPath);
                // In Release mode a new window will always be created.
                var window = await ElectronNET.API.Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
                {
                    // Do not show it right away. Additional configuration is needed.
                    Show = false,
                    WebPreferences = new WebPreferences
                    {
                        NodeIntegration = false,
                        ContextIsolation = true,
                        Preload = preloadPath,
                        Sandbox = true
                    },
#if RELEASE
                    // Do not Show the window frame in Release mode. The view should handle all operations (closing, minimizing, maximizing).
                    Frame = false
#endif
                });
#if DEBUG
                await window.WebContents.Session.ClearCacheAsync();
#endif
                // customize window
                window.SetTitle($"LotsenApp v{Program.Version}");

                // Load correct url
                // var serverConfiguration = new ServerConfiguration();
                // Configuration.GetSection("Server").Bind(serverConfiguration);
                // var sslConfiguration = serverConfiguration.Endpoints.FirstOrDefault(e => e.Ssl);
                // if (sslConfiguration != null)
                // {
                //     window.LoadURL($"https://{sslConfiguration.Host}:{sslConfiguration.Port ?? 443}");
                // }
                // else
                // {
                //     var configuration = serverConfiguration.Endpoints.First();
                //     window.LoadURL($"http://{configuration.Host}:{configuration.Port ?? 80}");
                // }
                // Finally show the window
                window.OnReadyToShow += () => window.Show();
            });

            ElectronNET.API.Electron.App.GetPathAsync(PathName.UserData).ContinueWith(result =>
            {
                var filePath = result.Result;
                startupLogger.LogInformation("Data will be stored to " + filePath);
            });

            // IPC Hooks
            var hooks = app.ApplicationServices.GetServices<IElectronHook>().ToList();
            foreach (var hook in hooks)
            {
                startupLogger.LogInformation($"Using hook {hook.GetType().Name}");
                hook.Down();
                hook.Up();
            }
            logger.LogInformation("Finished application configuration for the desktop environment");
        }
    }
}