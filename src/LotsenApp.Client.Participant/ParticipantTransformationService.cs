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
using LotsenApp.Client.DataFormat;
using LotsenApp.Client.DataFormat.Definition;
using LotsenApp.Client.DataFormat.Display;
using LotsenApp.Client.Participant.Delta;
using LotsenApp.Client.Participant.Dto;
using LotsenApp.Client.Participant.Header;
using LotsenApp.Client.Participant.Model;
using Document = LotsenApp.Client.Participant.Model.Document;

namespace LotsenApp.Client.Participant
{
    public class ParticipantTransformationService : IParticipantTransformationService
    {
        public const string EscapeKeys = "name,tint,icon";
        private readonly ParticipantCryptographyService _participantCryptographyService;
        private readonly DocumentDeltaHelper _helper;
        private readonly IDataFormatStorage _dataFormatStorage;

        public ParticipantTransformationService(ParticipantCryptographyService participantCryptographyService,
            DocumentDeltaHelper helper, IDataFormatStorage dataFormatStorage)
        {
            _participantCryptographyService = participantCryptographyService;
            _helper = helper;
            _dataFormatStorage = dataFormatStorage;
        }

        public async Task<ParticipantHeaderDto> CreateHeaderDto(string userId, EncryptedParticipantModel model)
        {
            var decryptedHeader = await _participantCryptographyService.DecryptHeaderAsync(model, userId);
            return CreateHeaderDto(model, decryptedHeader);
        }

        internal ParticipantHeaderDto CreateHeaderDto(EncryptedParticipantModel model,
            IDictionary<string, List<string>> header)
        {
            var additional = model.Additional;
            return new()
            {
                Id = model.Id,
                OnlineId = model.OnlineId,
                CreatedAt = model.CreatedAt,
                Synchronized = model.Synchronized,
                SynchronizedAt = model.SynchronizedAt,
                ProjectId = additional.ContainsKey("projectId") ? additional["projectId"] : null,
                DocumentedBy = additional.ContainsKey("documentedBy")
                    ? additional["documentedBy"]
                    : null,
                Header = header // TODO Transform header with i18nValues
            };
        }

        public async Task<ParticipantDocumentDto> GetDocuments(string userId, EncryptedParticipantModel model,
            EncryptedDeltaFile currentDelta)
        {
            var decryptedModel = await _participantCryptographyService.DecryptDataAsync(model, userId);
            var decryptedDelta = await _participantCryptographyService.DecryptDeltaFile(currentDelta, userId);
            return GetDocuments(decryptedModel, decryptedDelta);
        }

        internal ParticipantDocumentDto GetDocuments(ParticipantModel model, DeltaFile delta)
        {
            var documentDtos = CreateDocumentDtos(model.Data.Documents, delta.Documents, delta.DocumentTree);
            return new ParticipantDocumentDto
            {
                ParticipantId = model.Id,
                OnlineId = model.OnlineId,
                DefaultDocument = model.Data.DefaultDocument,
                Documents = documentDtos.ToArray(),
            };
        }

        public async Task<SpecificDocumentDto> GetDocument(string userId, string documentId,
            EncryptedParticipantModel model,
            EncryptedDeltaFile currentDelta)
        {
            var decryptedModel = await _participantCryptographyService.DecryptDataAsync(model, userId);
            var decryptedDelta = await _participantCryptographyService.DecryptDeltaFile(currentDelta, userId);
            return GetDocument(decryptedModel, decryptedDelta, documentId);
        }

        internal SpecificDocumentDto GetDocument(ParticipantModel model, DeltaFile delta, string documentId)
        {
            var documentDtos = CreateDocumentDtos(model.Data.Documents, delta.Documents,
                delta.DocumentTree, true);
            return documentDtos.First(d => d.Id == documentId);
        }

        private IEnumerable<SpecificDocumentDto> CreateDocumentDtos(IDictionary<string, Document> model,
            IDictionary<string, DocumentDelta> delta, IDictionary<string, TreeItem> tree, bool flatten = false)
        {
            var dtos = new Dictionary<string, (SpecificDocumentDto dto, int ordinal)>();
            // Get all documents that are unchanged or have been updated.
            foreach (var document in model.Values)
            {
                var relatedDelta = (delta.ContainsKey(document.Id) ? delta[document.Id] : null) ?? new DocumentDelta
                {
                    Id = document.Id,
                    Type = DeltaType.Unchanged
                };
                // Skip deleted documents
                if (relatedDelta.Type == DeltaType.Delete)
                {
                    continue;
                }

                var documentName = relatedDelta.Type == DeltaType.Update
                    ? relatedDelta.Value ?? document.Name
                    : document.Name;
                dtos.Add(document.Id, (new SpecificDocumentDto
                {
                    Id = document.Id,
                    DocumentId = document.DocumentId,
                    IsDelta = relatedDelta.Type != DeltaType.Unchanged,
                    Name = documentName,
                }, relatedDelta.Type == DeltaType.Update ? (relatedDelta.Ordinal ?? 99) : document.Ordinal));
            }

            // Get all new documents
            foreach (var newDocument in delta.Where(d => d.Value.Type == DeltaType.Create))
            {
                dtos.Add(newDocument.Value.Id, (new SpecificDocumentDto
                {
                    Id = newDocument.Value.Id,
                    DocumentId = newDocument.Value.DocumentId,
                    Name = newDocument.Value.Value,
                    IsDelta = true,
                }, newDocument.Value.Ordinal ?? 99));
            }

            if (flatten)
            {
                return dtos.Values.OrderBy(d => d.ordinal).Select(d => d.dto);
            }

            // Order the dtos as tree
            var treeDtos = CreateTree(dtos, tree);
            return treeDtos.OrderBy(d => d.ordinal).Select(d => d.dto);
        }

        private IEnumerable<(SpecificDocumentDto dto, int ordinal)> CreateTree(
            IDictionary<string, (SpecificDocumentDto dto, int ordinal)> dtos,
            IDictionary<string, TreeItem> tree, string parentDocumentName = null)
        {
            var currentLevel = new List<(SpecificDocumentDto dto, int ordinal)>();
            foreach (var treeItem in tree.Values)
            {
                if (!dtos.ContainsKey(treeItem.Id))
                {
                    continue;
                }

                var dto = dtos[treeItem.Id];
                dto.dto.ParentDocumentName = parentDocumentName;
                currentLevel.Add(dto);
                dto.dto.Documents = CreateTree(dtos, treeItem.Children, dto.dto.Name)
                    .OrderBy(d => d.ordinal)
                    .Select(d => d.dto)
                    .ToArray();
            }

            return currentLevel;
        }

        public async Task<DocumentValueDto> CreateValueDto(string userId, EncryptedParticipantModel encryptedModel,
            EncryptedDeltaFile encryptedDelta, string documentId)
        {
            var delta = await _participantCryptographyService.DecryptDeltaFile(encryptedDelta, userId);
            var model = await _participantCryptographyService.DecryptDataAsync(encryptedModel, userId);

            return CreateValueDto(model, delta, documentId);
        }

        internal DocumentValueDto CreateValueDto(ParticipantModel model, DeltaFile delta, string documentId)
        {
            var relatedDocument =
                model.Data.Documents.ContainsKey(documentId) ? model.Data.Documents[documentId] : null;
            var relatedDelta = _helper.ResolveDelta(delta.Documents, documentId);
            var noDelta = relatedDelta == null;
            var documentDto = new DocumentValueDto
            {
                Id = documentId,
                DocumentId = relatedDelta?.DocumentId ?? relatedDocument?.DocumentId,
                Name = relatedDelta?.Value ?? relatedDocument?.Name,
                IsDelta = !noDelta && relatedDelta.Type != DeltaType.Unchanged,
                Fields = CreateFieldDtos(relatedDocument?.Values, relatedDelta?.Values),
                Groups = CreateGroupDtos(relatedDocument?.Groups, relatedDelta?.Groups)
            };
            return documentDto;
        }

        private FieldDto[] CreateFieldDtos(IDictionary<string, DocumentField> fields,
            IDictionary<string, ValueDelta> deltas)
        {
            var deltaFields = deltas?
                .Select(d => d.Value)
                // Possible values are Create, Update or Delete. Delete will be applied later
                .Where(d => d.Type != DeltaType.Delete)
                .Select(d => new FieldDto
                {
                    Id = d.Id,
                    Value = d.Value,
                    IsDelta = true,
                    UseDisplay = d.UseDisplay
                }).ToList() ?? Enumerable.Empty<FieldDto>();
            var documentFields = fields?
                .Select(f => f.Value)
                .Where(f => deltaFields.All(d => d.Id != f.Id) &&
                            (
                                !(deltas?.ContainsKey(f.Id) ?? false) || deltas[f.Id].Type != DeltaType.Delete
                            ))
                .Select(f => new FieldDto
                {
                    Id = f.Id,
                    IsDelta = false,
                    Value = f.Value,
                    UseDisplay = f.UseDisplay
                }) ?? Enumerable.Empty<FieldDto>();
            return deltaFields.Concat(documentFields).ToArray();
        }

        private GroupDto[] CreateGroupDtos(IDictionary<string, DocumentGroup> groups,
            IDictionary<string, GroupDelta> deltas)
        {
            var deltaGroups = deltas?
                .Select(d => d.Value)
                // DeltaType.Update only happens on reorder as groups do not have values
                .Where(d => d.Type == DeltaType.Create || d.Type == DeltaType.Update)
                .Select(d => (new GroupDto
                {
                    Id = d.Id,
                    GroupId = d.GroupId,
                    IsDelta = d.Type is DeltaType.Create or DeltaType.Update,
                    Fields = CreateFieldDtos(groups?.Select(g => g.Value).FirstOrDefault(g => g.Id == d.Id)?.Fields,
                        d.Fields),
                    Children = CreateGroupDtos(groups?.Select(g => g.Value).FirstOrDefault(g => g.Id == d.Id)?.Children,
                        d.Children)
                }, d.Ordinal ?? 99)).ToList() ?? Enumerable.Empty<(GroupDto, int)>().ToList();
            var documentGroups = groups?
                .Select(g => g.Value)
                .Where(g => deltaGroups.All(d => d.Item1?.Id != g.Id) &&
                            (
                                !(deltas?.ContainsKey(g.Id) ?? false) || deltas[g.Id].Type != DeltaType.Delete
                            )
                )
                .Select(g => (new GroupDto
                {
                    Id = g.Id,
                    GroupId = g.GroupId,
                    IsDelta = false,
                    Children = CreateGroupDtos(g.Children,
                        deltas?.Select(d => d.Value).FirstOrDefault(d => d.Id == g.Id)?.Children),
                    Fields = CreateFieldDtos(g.Fields,
                        deltas?.Select(d => d.Value).FirstOrDefault(d => d.Id == g.Id)?.Fields)
                }, g.Ordinal)) ?? Enumerable.Empty<(GroupDto, int)>();
            return deltaGroups.Concat(documentGroups)
                .OrderBy(d => d.Item2)
                .Select(d => d.Item1)
                .ToArray();
        }

        public async Task<EncryptedParticipantModel> CreateEncryptedModel(CreateParticipantDto participantDto,
            string userId)
        {
            var header = new Dictionary<string, List<string>>
            {
                {"name", new List<string> {participantDto.Name}},
                {"icon", new List<string> {participantDto.Icon}},
                {"tint", new List<string> {participantDto.Tint}}
            };
            var additional = new Dictionary<string, string>
            {
                {IParticipantTransformationService.ProjectId, participantDto.ProjectId},
                {IParticipantTransformationService.DocumentedBy, participantDto.DocumentedBy}
            };
            var plainModel = new ParticipantModel()
            {
                Id = Guid.NewGuid().ToString("N"),
                Synchronized = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                Header = header,
                Additional = additional,
            };
            return await _participantCryptographyService.EncryptModel(plainModel, userId);
        }

        public async Task<HeaderEntryDto[]> CreateHeaderEntryDtos(string userId, IParticipantStorage storage)
        {
            var headerTasks = storage.GetParticipants(userId)
                .Select(m => _participantCryptographyService.DecryptHeaderAsync(m, userId));
            var header = await Task.WhenAll(headerTasks);
            return CreateHeaderEntryDtos(header);
        }

        public async Task<EncryptedParticipantModel> UpdateParticipantHeader(string userId, HeaderEditDto dto,
            EncryptedParticipantModel model)
        {
            var decryptedHeader = await _participantCryptographyService.DecryptHeaderAsync(model, userId);
            decryptedHeader = UpdateParticipantHeader(decryptedHeader, dto);

            return await _participantCryptographyService.EncryptHeaderAsync(model, userId, decryptedHeader);
        }

        internal IDictionary<string, List<string>> UpdateParticipantHeader(
            IDictionary<string, List<string>> decryptedHeader, HeaderEditDto dto)
        {
            var keys = EscapeKeys.Split(",");
            foreach (var key in keys)
            {
                var value = decryptedHeader[key];
                var oldValue = value[0];
                value.Clear();
                var newValue = dto.GetType().GetProperty(key.Capitalise())?.GetValue(dto);
                value.Add(newValue?.ToString() ?? oldValue);
            }

            return decryptedHeader;
        }

        internal HeaderEntryDto[] CreateHeaderEntryDtos(IDictionary<string, List<string>>[] participantHeader)
        {
            var aggregatedHeader = CreateAggregate(participantHeader);

            return CreateDtos(aggregatedHeader).Where(d => d.Values.Length > 0).ToArray();
        }

        internal IDictionary<string, HashSet<string>> CreateAggregate(
            IDictionary<string, List<string>>[] participantHeader)
        {
            return participantHeader.Aggregate(new Dictionary<string, HashSet<string>>(),
                (aggregate, current) =>
                {
                    foreach (var (key, value) in current)
                    {
                        if (!aggregate.ContainsKey(key))
                        {
                            var newSet = new HashSet<string>(value);
                            aggregate.Add(key, newSet);
                            continue;
                        }

                        foreach (var s in value)
                        {
                            aggregate[key].Add(s);
                        }
                    }

                    return aggregate;
                });
        }

        internal HeaderEntryDto[] CreateDtos(IDictionary<string, HashSet<string>> aggregatedHeader)
        {
            var escapeList = EscapeKeys.Split(",");
            var dataFields = _dataFormatStorage.Projects
                .SelectMany(p => p.DataDefinition.DataFields)
                .ToDictionary(f => f.Id, f => f);
            var dataFieldDisplay = _dataFormatStorage.Projects
                .SelectMany(p => p.DataDisplay.DataFields)
                .ToDictionary(d => d.Id, d => d);
            var dataTypes = _dataFormatStorage.Projects
                .SelectMany(p => p.DataDefinition.DataTypes)
                .ToDictionary(d => d.Id, d => d);
            var dataTypeDisplay = _dataFormatStorage.Projects
                .SelectMany(p => p.DataDisplay.DataTypes)
                .ToDictionary(d => d.Id, d => d);
            var dataFieldHeader = aggregatedHeader.Where(k => !escapeList.Contains(k.Key)).Select(kv =>
                new HeaderEntryDto
                {
                    FieldId = kv.Key,
                    Name = dataFields[kv.Key].Name,
                    I18NKey = dataFieldDisplay.ContainsKey(kv.Key) ? dataFieldDisplay[kv.Key].I18NKey : null,
                    DataType = dataTypes[dataFields[kv.Key].DataType].Type,
                    Values = kv.Value.Select(v => new HeaderValueDto
                    {
                        Value = v,
                        I18NKey = GetI18NKey(dataTypeDisplay, v, dataFields[kv.Key].DataType)
                    }).ToArray()
                }).ToArray();
            var metadataHeader = escapeList.Select(key => new HeaderEntryDto
            {
                FieldId = key,
                Name = key,
                I18NKey = $"Application.DataFormat.Metadata.{key}",
                DataType = Types.CUSTOM,
                Values = aggregatedHeader.ContainsKey(key)
                    ? aggregatedHeader[key].Select(v => new HeaderValueDto
                    {
                        Value = v
                    }).ToArray()
                    : Array.Empty<HeaderValueDto>()
            });
            return metadataHeader.Concat(dataFieldHeader).ToArray();
        }

        internal string GetI18NKey(IDictionary<string, DataTypeDisplay> dataTypeDisplays, string value,
            string dataTypeId)
        {
            if (dataTypeDisplays.ContainsKey(dataTypeId) &&
                dataTypeDisplays[dataTypeId].DisplayValues.ContainsKey(value))
            {
                return dataTypeDisplays[dataTypeId].DisplayValues[value].FirstOrDefault();
            }

            return null;
        }
    }
}