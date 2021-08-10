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
using System.Linq;
using System.Threading.Tasks;
using LotsenApp.Client.DataFormat.Access;

namespace LotsenApp.Client.DataFormat
{
    public class DataFormatService
    {
        private readonly IDataFormatStorage _storage;

        public DataFormatService(IDataFormatStorage storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Determines whether or not a user is part of a project.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <param name="projectId">The id of the project</param>
        /// <returns>true, if the user is part of the project, false otherwise</returns>
        public Task<bool> IsUserInProject(string userId, string projectId)
        {
            if (_storage.Associations == null || !_storage.Associations.ContainsKey(projectId))
            {
                return Task.FromResult(false);
            }
            var isInProject = _storage.Associations[projectId].Contains(userId);
            return Task.FromResult(isInProject);
        }
        
        /// <summary>
        /// Determines whether or not a user has access to a project.
        ///
        /// Having access to a project means either being part of the project or that the project is open for documentation.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <param name="projectId">The id of the project</param>
        /// <returns>true, if the user has access to the project, false otherwise</returns>
        public async Task<bool> UserHasAccessToProject(string userId, string projectId)
        {
            var isInProject = await IsUserInProject(userId, projectId);
            var project = _storage.GetSpecificProject(projectId);
            return isInProject || project.OpenForDocumentation;
        }

        /// <summary>
        /// Receive all project with the user as participant
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Displayable[]> GetUserProjects(string userId)
        {
            return await
                Task.WhenAll(
                    _storage.Associations.Where(kv => kv.Value.Contains(userId))
                .Select(kv => kv.Key)
                .Select(GetProject));
            
        }

        /// <summary>
        /// Receive all projects that a user may use for documentation purposes.
        ///
        /// This includes all project in which a user participates as well as all projects that are open for documentation. 
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>A list of all project that are open for documentation or with participation of the given user.</returns>
        public async Task<Displayable[]> GetDocumentationProjects(string userId)
        {
            var userProjects = (await GetUserProjects(userId)).Select(d => d.Id).ToArray();
            var documentationProjects = _storage.Projects
                .Where(p => p.OpenForDocumentation || userProjects.Contains(p.Id))
                .Select(p => new Displayable
                {
                    Id = p.Id,
                    Name = p.Name,
                    I18NKey = p.I18NKey
                }).ToArray();
            return documentationProjects;
        }

        /// <summary>
        /// Get common information for a document
        /// </summary>
        /// <param name="projectId">The id of the containing project</param>
        /// <param name="documentId">The id of the document</param>
        /// <returns></returns>
        public Task<Displayable> GetDocumentHeader(string projectId, string documentId)
        {
            var project = _storage.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                throw new ProjectNotFoundException();
            }

            var documentDefinition = project.DataDefinition.Documents.FirstOrDefault(d => d.Id == documentId);
            if (documentDefinition == null)
            {
                throw new DocumentNotFoundException();
            }

            var documentDisplay = project.DataDisplay.Documents.FirstOrDefault(d => d.Id == documentId);
            var displayable = new Displayable
            {
                Id = documentId,
                Name = documentDefinition.Name,
                I18NKey = documentDisplay?.I18NKey
            };
            return Task.FromResult(displayable);
        }

        /// <summary>
        /// Get common information for a project
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <returns></returns>
        public Task<Displayable> GetProject(string projectId)
        {
            var project = _storage.GetSpecificProject(projectId);
            var displayable = new Displayable
            {
                Id = project.Id,
                I18NKey = project.I18NKey,
                Name = project.Name
            };
            return Task.FromResult(displayable);
        }
        /// <summary>
        /// Get header information for all displayable entities (documents and documentation events) in a project
        /// </summary>
        /// <param name="projectId">The project id</param>
        /// <returns></returns>
        public Task<IEnumerable<Displayable>> GetDisplayablesInProject(string projectId)
        {
            var project = _storage.GetSpecificProject(projectId);
            var documentationEvents = project.DataDefinition.DocumentationEvents
                .Select(de =>
                {
                    var displayInformation = project.DataDisplay.DocumentationEvents
                        .FirstOrDefault(ded => ded.Id == de.Id);
                    return (new Displayable
                    {
                        Id = de.Id,
                        Name = de.Name,
                        I18NKey = displayInformation?.I18NKey
                    }, displayInformation?.Ordinal ?? 999);
                });
            var documents = project.DataDefinition.Documents
                .Where(d => project.DataDisplay.TopLevelDocuments.Contains(d.Id))
                .Select(d =>
            {
                var displayInformation = project.DataDisplay.Documents
                    .FirstOrDefault(dd => dd.Id == d.Id);
                return (new Displayable
                {
                    Id = d.Id,
                    Name = d.Name,
                    I18NKey = displayInformation?.I18NKey
                }, displayInformation?.Ordinal ?? 999);
            });
            return Task.FromResult(documentationEvents
                .Concat(documents)
                .OrderBy(d => d.Item2)
                .Select(d => d.Item1));
        }

        /// <summary>
        /// Get header information for all displayable entities (documents) in a documentation event.
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="documentationEventId">The id of the documentation event</param>
        /// <returns></returns>
        public Task<IEnumerable<Displayable>> GetDisplayablesInDocumentationEvent(string projectId, string documentationEventId)
        {
            var project = _storage.GetSpecificProject(projectId);
            var documentationEvent =
                project.DataDefinition.DocumentationEvents.FirstOrDefault(de => de.Id == documentationEventId);
            if (documentationEvent == null)
            {
                var document = project.DataDefinition.Documents.FirstOrDefault(d => d.Id == documentationEventId);
                // Request for a document might exist. Documents do not have child documents
                if (document != null)
                {
                    return Task.FromResult(Enumerable.Empty<Displayable>());
                }
                throw new DocumentationEventNotFoundException();
            }

            var displayInformation =
                project.DataDisplay.DocumentationEvents.FirstOrDefault(ded => ded.Id == documentationEventId);
            var displayables = documentationEvent.ValidDocuments.Select(vd =>
                {
                    var document = project.DataDefinition.Documents.FirstOrDefault(d => d.Id == vd);
                    if (document == null)
                    {
                        throw new DocumentNotFoundException(vd);
                    }
                    var displayable = new Displayable
                    {
                        Id = document.Id,
                        Name = document.Name,
                        I18NKey = project.DataDisplay.Documents.FirstOrDefault(dd => dd.Id == vd)?.I18NKey
                    };
                    return (displayable,
                        displayInformation?.ValidDocuments.FirstOrDefault(vdd => vdd.Id == vd)?.Ordinal ?? 999);
                }).OrderBy(d => d.Item2)
                .Select(d => d.displayable);
            return Task.FromResult(displayables);
        }

        /// <summary>
        /// Get the format object for a specific document or documentation event
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="DocumentNotFoundException"></exception>
        /// <exception cref="DocumentationEventNotFoundException"></exception>
        public DocumentDataFormatDto GetDocumentFormat(string projectId, string documentId = null)
        {
            // If both ids are null
            if (documentId == null)
            {
                throw new NullReferenceException();
            }
            var project = _storage.GetSpecificProject(projectId);
            // Return a top level document
            var document = project.DataDefinition.Documents.FirstOrDefault(d => d.Id == documentId);
            if (document == null)
            {
                // Try to return the documentation event document and not a child document
                var documentationEvent = project.DataDefinition.DocumentationEvents.FirstOrDefault(de => de.Id == documentId);
                if (documentationEvent == null)
                {
                    throw new DocumentationEventNotFoundException();
                }
                
                var eventDocumentId = documentationEvent.DocumentId;
                document = project.DataDefinition.Documents.FirstOrDefault(d => d.Id == eventDocumentId);
                if (document == null)
                {
                    throw new DocumentNotFoundException(eventDocumentId);
                }
            }

            var documentDisplay = project.DataDisplay.Documents.FirstOrDefault(dd => dd.Id == document.Id);
            return new DocumentDataFormatDto(document, documentDisplay, project);
        }

        public async Task<DataDefinitionDto[]> GetDataDefinitionsForUser(string userId)
        {
            var documentationProjects = await GetDocumentationProjects(userId);
            var userProjects = await GetUserProjects(userId);
            var userProjectIds = userProjects.Select(d => d.Id).ToArray();
            var dtoTasks = documentationProjects.Select(async d =>
            {
                var locales = await _storage.GetLocalesForProject(d.Id);
                var userInProject = userProjectIds.Contains(d.Id);
                var project = _storage.Projects.First(p => p.Id == d.Id);
                return new DataDefinitionDto
                {
                    ProjectId = d.Id,
                    Name = d.Name,
                    Version = project.Version,
                    Locales = locales,
                    IsParticipant = userInProject,
                    I18NKey = d.I18NKey
                };
            });
            return (await Task.WhenAll(dtoTasks)).ToArray();
        }
    }
}