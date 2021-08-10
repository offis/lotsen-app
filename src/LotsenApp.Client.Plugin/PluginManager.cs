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
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LotsenApp.Client.Plugin
{
    /// <summary>
    /// This class was taken from my private tool box
    /// </summary>
    public class PluginManager
    {
        private readonly ILogger<PluginManager> _logger;

        public string PluginFilePath { get; set; } = "config";

        public PluginManager(ILogger<PluginManager> logger)
        {
            _logger = logger;
        }

        public ILotsenAppPlugin[] DiscoverPlugins(bool forceNewDiscover = false)
        {
            Directory.CreateDirectory("config");
            const string pluginFile = "plugins.json";
            Directory.CreateDirectory(PluginFilePath);
            var file = Path.Join(PluginFilePath, pluginFile);
            if (File.Exists(file) && !forceNewDiscover)
            {
                _logger.LogInformation("Loading plugins from preexisting file");
                return LoadPluginsFromFile(file);
            }

            _logger.LogInformation("Loading plugins from assemblies");
            var plugins = DiscoverFromAssemblies();

            WritePluginFile(plugins, file);

            plugins = plugins.Where(p =>
                p.GetType().GetCustomAttribute<PluginDefaultAttribute>()?.AutomaticallyActivate ?? true).ToArray();
            return plugins;
        }

        private ILotsenAppPlugin[] LoadPluginsFromFile(string file)
        {
            var serializedPluginDefinition = File.ReadAllText(file);
            var definitions = JsonConvert.DeserializeObject<LotsenAppPluginDefinition[]>(serializedPluginDefinition);
            var plugins = new List<ILotsenAppPlugin>();

            if (definitions == null)
            {
                return Array.Empty<ILotsenAppPlugin>();
            }

            foreach (var definition in definitions)
            {
                if (!definition.Enabled)
                {
                    _logger.LogDebug($"Skipping plugin {definition.AssemblyQualifiedName} as it is disabled");
                    continue;
                }
                try
                {
                    _logger.LogDebug($"Trying to load assembly {definition.Assembly}");
                    AppDomain.CurrentDomain.Load(definition.Assembly);
                    var type = Type.GetType(definition.AssemblyQualifiedName);
                    if (type == null)
                    {
                        _logger.LogWarning($"The type {definition.AssemblyQualifiedName} could not be found");
                        Debug.WriteLine($"Cannot get type {definition.AssemblyQualifiedName} as a plugin");
                        continue;
                    }

                    var plugin = (ILotsenAppPlugin) Activator.CreateInstance(type);
                    plugins.Add(plugin);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"An exception loading the plugin {definition.AssemblyQualifiedName} occured", ex);
                    _logger.LogError($"Cannot load assembly {definition.Assembly}", ex);
                }
            }

            return plugins.ToArray();
        }

        private ILotsenAppPlugin[] DiscoverFromAssemblies()
        {
            var files = Directory.GetFiles(".", "LotsenApp.Client.*.dll")
                .Where(f => !f.EndsWith("Views.dll"));
            var assemblies = files
                .Select(f => f.Replace(".dll", "")
                    .Replace(".\\", ""))
                .ToArray();
            foreach (var assembly in assemblies)
            {
                try
                {
                    _logger.LogDebug($"Trying to load assembly {assembly}");
                    AppDomain.CurrentDomain.Load(assembly);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Could not load assembly {assembly}", ex);
                }
            }
            var pluginTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName?.StartsWith("LotsenApp.Client") ?? false)
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface && t.GetInterfaces().Contains(typeof(ILotsenAppPlugin)))
                .ToArray();
            return pluginTypes.Select(p => (ILotsenAppPlugin) Activator.CreateInstance(p)).ToArray();
        }

        private void WritePluginFile(ILotsenAppPlugin[] plugins, string file)
        {
            _logger.LogDebug("Writing plugin definitions to file");
            var serializedDefinition = JsonConvert.SerializeObject(plugins.Select(p => new LotsenAppPluginDefinition
            {
                AssemblyQualifiedName = p.GetType().AssemblyQualifiedName,
                Assembly = p.GetType().Assembly.FullName,
                Enabled = p.GetType().GetCustomAttribute<PluginDefaultAttribute>()?.AutomaticallyActivate ?? true,
            }), Formatting.Indented);
            File.WriteAllText(file, serializedDefinition);
        }
    }
}
