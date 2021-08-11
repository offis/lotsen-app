using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LotsenApp.Client.Electron
{
    // Inspired by https://devblogs.microsoft.com/aspnet/configuring-https-in-asp-net-core-across-different-platforms/
    public static class KestrelConfigurationExtension
    {
        public static void ConfigureEndpoints(this KestrelServerOptions options)
        {
            var configuration = options.ApplicationServices.GetRequiredService<IConfiguration>();
            var environment = options.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            var server = new ServerConfiguration();
            configuration.GetSection("Server").Bind(server);
            
            foreach (var endpoint in server.Endpoints)
            {
                var port = endpoint.Port ?? (endpoint.Ssl ? 443 : 80);
                var ipAddresses = new List<IPAddress>();
                if (endpoint.Host == "localhost")
                {
                    ipAddresses.Add(IPAddress.IPv6Loopback);
                    ipAddresses.Add(IPAddress.Loopback);
                } else if (IPAddress.TryParse(endpoint.Host, out var address))
                {
                    ipAddresses.Add(address);
                }
                else
                {
                    ipAddresses.Add(IPAddress.IPv6Any);
                }
                
                foreach (var address in ipAddresses)
                {
                    options.Listen(address, port, listenOptions =>
                    {
                        if (!endpoint.Ssl) return;
                        var certificate = CertificateProvider.ProvideCertificateForEndpoint(endpoint);
                        listenOptions.UseHttps(certificate);
                    });
                }
            }
        }
    }
}