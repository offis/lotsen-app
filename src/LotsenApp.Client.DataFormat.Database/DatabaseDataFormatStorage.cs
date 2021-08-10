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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace LotsenApp.Client.DataFormat.Database
{
    public class DatabaseDataFormatStorage: IDataFormatStorage
    {
        private const string Purpose = "LotsenApp.DataFormat";
        private readonly DataFormatContext _context;
        private readonly IDataProtector _protector;
        public IDictionary<string, string[]> Associations => GetAssociations();
        public Project[] Projects => GetProjects();

        public DatabaseDataFormatStorage(DataFormatContext context, IDataProtectionProvider provider)
        {
            _context = context;
            _protector = provider.CreateProtector(Purpose);
        }
        
        public async Task<bool> CreateProject(Project project)
        {
            var projectCreated = false;
            // TODO encrypt project
            var serializedProject = JsonConvert.SerializeObject(project);
            
            var storedProject = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == project.Id);
            if (storedProject == null)
            {
                await _context.Projects.AddAsync(new ProjectEntry
                {
                    ProjectId = project.Id,
                    EncryptedProjectDefinition = serializedProject
                });
                projectCreated = true;
            }
            else
            {

                var deserializedProject = JsonConvert.DeserializeObject<Project>(storedProject.EncryptedProjectDefinition);
                if ((deserializedProject?.Version ?? -1) < project.Version)
                {
                    storedProject.EncryptedProjectDefinition = serializedProject;
                    _context.Projects.Update(storedProject);
                    projectCreated = true;
                }
            }


            await _context.SaveChangesAsync();
            return projectCreated;
        }

        public async Task AddI18N(Project project, string locale, string i18N, bool forceUpdate)
        {
            var existingI18N = await _context.I18N.FirstOrDefaultAsync(i => i.ProjectId == project.Id && i.Locale == locale);
            if (existingI18N != null && !forceUpdate)
            {
                return;
            }

            if (existingI18N != null)
            {
                existingI18N.I18NValues = i18N;
                _context.I18N.Update(existingI18N);
            }
            else
            {
                await _context.I18N.AddAsync(new I18NEntry
                {
                    ProjectId = project.Id,
                    Locale = locale,
                    I18NValues = i18N
                });
            }

            await _context.SaveChangesAsync();
        }

        public Task<string[]> GetProjectI18N(string locale)
        {
            return _context.I18N.Where(i => i.Locale == locale)
                .Select(i => i.I18NValues)
                .ToArrayAsync();
        }

        public Task<string[]> GetLocalesForProject(string projectId)
        {
            return _context.I18N.Where(i => i.ProjectId == projectId)
                .Select(i => i.Locale)
                .ToArrayAsync();
        }

        private IDictionary<string, string[]> GetAssociations()
        {
            var encryptedContent = _context.Association.First();
            var decryptedContent = _protector?.Unprotect(encryptedContent.Association) ?? encryptedContent.Association;
            // Format projectId => userId[]
            return JsonConvert.DeserializeObject<IDictionary<string, string[]>>(decryptedContent);
        }
        
        private Project[] GetProjects()
        {

            return _context.Projects
                .Select(e => e.EncryptedProjectDefinition)
                .Select(e => _protector != null ? _protector.Unprotect(e) : e)
                .Select(e => JsonConvert.DeserializeObject<Project>(e))
                .ToArray();
        }
    }
}