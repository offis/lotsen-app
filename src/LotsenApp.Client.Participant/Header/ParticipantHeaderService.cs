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
using LotsenApp.Client.DataFormat;
using LotsenApp.Client.DataFormat.Definition;
using LotsenApp.Client.DataFormat.Display;
using LotsenApp.Client.File;
using LotsenApp.Client.Participant.Model;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;

namespace LotsenApp.Client.Participant.Header
{
    public class ParticipantHeaderService : IParticipantHeaderService
    {
        private const string HeaderPurpose = "Participant.Header.";
        private readonly IFileService _fileService;
        private readonly IDataProtectionProvider _provider;
        private readonly DocumentDeltaHelper _helper;

        public ParticipantHeaderService(IFileService fileService, IDataProtectionProvider provider,
            DocumentDeltaHelper helper)
        {
            _fileService = fileService;
            _provider = provider;
            _helper = helper;
        }

        private HeaderDefinitionFile GetFileContent(string userId, AccessMode mode = AccessMode.Write)
        {
            var headerDirectory = _fileService.Join($"data/{userId}/");
            Directory.CreateDirectory(headerDirectory);
            var headerFile = _fileService.Join($"data/{userId}/{IParticipantHeaderService.FileName}");
            var accessor = ConcurrentFileAccessHelper.GetAccessor(headerFile);
            if (mode == AccessMode.Read)
            {
                accessor.EnterReadLock();
            }
            else
            {
                accessor.EnterWriteLock();
            }

            HeaderDefinitionFile definitionFile;
            if (System.IO.File.Exists(headerFile))
            {
                var fileContent = System.IO.File.ReadAllText(headerFile);
                var protector = _provider.CreateProtector($"{HeaderPurpose}{userId}");
                var decryptedFileContent = protector.Unprotect(fileContent);
                definitionFile = JsonConvert.DeserializeObject<HeaderDefinitionFile>(decryptedFileContent);
            }
            else
            {
                definitionFile = new HeaderDefinitionFile();
            }

            if (mode == AccessMode.Read)
            {
                accessor.ExitReadLock();
            }

            return definitionFile;
        }

        private void WriteFileContent(string userId, HeaderDefinitionFile content)
        {
            var headerFile = _fileService.Join($"data/{userId}/{IParticipantHeaderService.FileName}");
            var accessor = ConcurrentFileAccessHelper.GetAccessor(headerFile);
            var definitionFile = JsonConvert.SerializeObject(content);
            var protector = _provider.CreateProtector($"{HeaderPurpose}{userId}");
            var encryptedFileContent = protector.Protect(definitionFile);
            System.IO.File.WriteAllText(headerFile, encryptedFileContent);
            if (accessor.IsWriteLockHeld)
            {
                accessor.ExitWriteLock();
            }
        }

        public void AddToParticipantHeader(string userId, string participantId, string path)
        {
            var content = GetFileContent(userId);
            if (!content.ParticipantHeader.ContainsKey(participantId))
            {
                content.ParticipantHeader.Add(participantId, new List<string>());
            }

            var headerList = content.ParticipantHeader[participantId];
            if (!headerList.Contains(path))
            {
                headerList.Add(path);
            }

            WriteFileContent(userId, content);
        }

        public void RemoveFromParticipantHeader(string userId, string participantId, string path)
        {
            var content = GetFileContent(userId);
            if (content.ParticipantHeader.ContainsKey(participantId))
            {
                var headerList = content.ParticipantHeader[participantId];
                if (headerList.Contains(path))
                {
                    headerList.Remove(path);
                }
            }

            WriteFileContent(userId, content);
        }

        public void AddToProjectHeader(string userId, string projectId, string path)
        {
            var content = GetFileContent(userId);
            if (!content.ProjectHeader.ContainsKey(projectId))
            {
                content.ProjectHeader.Add(projectId, new List<string>());
            }

            var headerList = content.ProjectHeader[projectId];
            if (!headerList.Contains(path))
            {
                headerList.Add(path);
            }

            WriteFileContent(userId, content);
        }

        public void RemoveFromProjectHeader(string userId, string projectId, string path)
        {
            var content = GetFileContent(userId);
            if (content.ProjectHeader.ContainsKey(projectId))
            {
                var headerList = content.ProjectHeader[projectId];
                if (headerList.Contains(path))
                {
                    headerList.Remove(path);
                }
            }

            WriteFileContent(userId, content);
        }

        public ParticipantModel CalculateHeader(string userId, ParticipantModel model)
        {
            var content = GetFileContent(userId, AccessMode.Read);
            // recalculate header
            var participantProject = model.Additional[IParticipantTransformationService.DocumentedBy];
            var projectHeader = content.ProjectHeader.ContainsKey(participantProject)
                ? content.ProjectHeader[participantProject]
                : new List<string>();
            var participantHeader = content.ParticipantHeader.ContainsKey(model.Id)
                ? content.ParticipantHeader[model.Id]
                : new List<string>();
            var keySplit = ParticipantTransformationService.EscapeKeys.Split(",");
            var entries = model.Header
                .Where(k => keySplit.Contains(k.Key))
                .Select(kv => (kv.Key, kv.Value))
                .ToArray();
            model.Header.Clear();
            foreach (var entry in entries)
            {
                model.Header.Add(entry.Key, entry.Value);
            }
            foreach (var path in participantHeader)
            {
                var pathItems = path.Split(".");
                var field = _helper.ResolveField(model, pathItems);
                if (field == null)
                {
                    continue;
                }

                if (model.Header.ContainsKey(field.Id))
                {
                    model.Header[field.Id].Add(field.Value);
                }
                else
                {
                    model.Header.Add(field.Id, new List<string> {field.Value});
                }
            }

            foreach (var path in projectHeader)
            {
                var pathItems = path.Split(".");
                var fields = _helper.ResolveFieldsWithProjectPath(model, pathItems);
                foreach (var field in fields)
                {
                    if (model.Header.ContainsKey(field.Id))
                    {
                        model.Header[field.Id].Add(field.Value);
                    }
                    else
                    {
                        model.Header.Add(field.Id, new List<string> {field.Value});
                    }
                }
            }

            return model;
        }
    }
}