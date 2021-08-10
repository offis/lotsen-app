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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LotsenApp.Client.File;
using LotsenApp.Client.Plugin;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;

namespace LotsenApp.Client.DataFormat.File
{
    public class FileDataFormatStorage: IDataFormatStorage
    {
        private const string AssociationFileName = "association.crypt";
        private const string Purpose = "LotsenApp.DataFormat";
        private readonly DirectoryInfo _informationStorage;

        private DirectoryInfo I18NDirectory =>
            Directory.CreateDirectory(Path.Join(_informationStorage.FullName, "i18n"));
        private readonly IDataProtector _protector;
        
        private string AssociationFile => Path.Join(_informationStorage.FullName, AssociationFileName);

        public FileDataFormatStorage(IDataProtectionProvider provider, IFileService fileService)
        {
            _informationStorage = Directory.CreateDirectory(fileService.Join("config/data-definition"));
            Directory.CreateDirectory(Path.Join(_informationStorage.FullName, "i18n"));
            _protector = provider.CreateProtector(Purpose);
        }

        /// <summary>
        /// Format: projectId => userId[]
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string[]> Associations => GetAssociations();

        public Project[] Projects => GetProjects();
        public Task<bool> CreateProject(Project project)
        {
            // TODO encrypt project
            var serializedProject = JsonConvert.SerializeObject(project, Formatting.Indented);
            var projectFile = Path.Join(_informationStorage.FullName, $"{project.Id}.json");

            if (!System.IO.File.Exists(projectFile))
            {
                System.IO.File.WriteAllText(projectFile, serializedProject);
                return Task.FromResult(true);
            }

            var currentDefinition = JsonConvert.DeserializeObject<Project>(System.IO.File.ReadAllText(projectFile));

            if ((currentDefinition?.Version ?? -1) < project.Version)
            {
                System.IO.File.WriteAllText(projectFile, serializedProject);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task AddI18N(Project project, string locale, string i18N, bool forceUpdate)
        {
            var i18NFileName = $"{project.Id}_{locale}.json";
            var i18NFile = Path.Join(I18NDirectory.FullName, i18NFileName);

            if (!System.IO.File.Exists(i18NFile) || forceUpdate)
            {
                System.IO.File.WriteAllText(i18NFile, i18N);
            }
            return Task.CompletedTask;
        }

        public Task<string[]> GetProjectI18N(string locale)
        {
            return Task.Run(() =>
            {
                return I18NDirectory.GetFiles()
                    .Where(f => f.Name.EndsWith($"{locale}.json"))
                    .Select(f =>
                {
                    var readLock = ConcurrentFileAccessHelper.GetAccessor(f.FullName);
                    readLock.EnterReadLock();
                    var content = System.IO.File.ReadAllText(f.FullName);
                    readLock.ExitReadLock();
                    return content;
                }).ToArray();
            });
        }

        public Task<string[]> GetLocalesForProject(string projectId)
        {
            var i18NFolder = Directory.CreateDirectory(Path.Join(_informationStorage.FullName, "i18n"));
            var locales = i18NFolder.GetFiles()
                .Where(f => f.Name.StartsWith(projectId))
                .Select(f => f.Name.Replace($"{projectId}_", "").Replace(".json", ""))
                .ToArray();
            return Task.FromResult(locales);
        }

        private IDictionary<string, string[]> GetAssociations()
        {
            if (!System.IO.File.Exists(AssociationFile))
            {
                return new Dictionary<string, string[]>();
            }
            var lockSlim = ConcurrentFileAccessHelper.GetAccessor(AssociationFile);
            lockSlim.EnterReadLock();
            var encryptedContent = System.IO.File.ReadAllText(AssociationFile);
            lockSlim.ExitReadLock();
            var decryptedContent = _protector.Unprotect(encryptedContent);
            // Format projectId => userId[]
            return JsonConvert.DeserializeObject<IDictionary<string, string[]>>(decryptedContent);
        }
        
        private Project[] GetProjects()
        {
            return _informationStorage.GetFiles()
                .Where(f => !f.Name.EndsWith(AssociationFileName))
                .Select(f =>
                {
                    var lockSlim = ConcurrentFileAccessHelper.GetAccessor(f.FullName);
                    lockSlim.EnterReadLock();
                    var encryptedContent = System.IO.File.ReadAllText(f.FullName);
                    lockSlim.ExitReadLock();
                    // TODO Create configuration for encryption/decryption of project files
                    var decryptedContent = encryptedContent;//_protector.Unprotect(encryptedContent);
                    return JsonConvert.DeserializeObject<Project>(decryptedContent);
                }).ToArray();
        }
    }
}