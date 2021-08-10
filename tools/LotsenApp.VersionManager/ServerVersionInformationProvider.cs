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
using System.Text.RegularExpressions;
using System.Xml;

namespace LotsenApp.VersionManager
{
    public class ServerVersionInformationProvider: IVersionInformationProvider
    {
        private static readonly string[] ValidNodes = {"AssemblyVersion", "FileVersion", "PackageVersion"};
        public string Name => "Server";
        
        public string GetVersion()
        {
            var xml = LoadXmlDocument();

            var versions = (xml.DocumentElement ?? throw new InvalidOperationException()).Cast<XmlNode>()
                .SelectMany(n => n.ChildNodes.Cast<XmlNode>())
                .Where(n => ValidNodes.Contains(n.Name))
                .ToDictionary(k => k.Name, v => v.InnerText);
            return versions.ContainsKey("PackageVersion")? versions["PackageVersion"] : versions["AssemblyVersion"];
        }

        public void SetVersion(string version)
        {
            var mainVersion = version.Split("-")[0];
            var fileName = GetFileName();
            var content = File.ReadAllText(fileName);
            foreach (var node in ValidNodes)
            {
                var regex = new Regex($"<{node}>.+?</{node}>");
                content = regex.Replace(content, $"<{node}>{(node != "PackageVersion" ? $"{mainVersion}.0" : version)}</{node}>");
            }
            File.WriteAllText(fileName, content);
        }

        private XmlDocument LoadXmlDocument()
        {
            var file = GetFileName();
            var xml = new XmlDocument();
            xml.Load(file);
            return xml;
        }

        private string GetFileName()
        {
            var root = Helper.GetRepositoryRoot();
            return Path.Join(root.FullName, "src/LotsenApp.Client.Electron/LotsenApp.Client.Electron.csproj");
        }
    }
}