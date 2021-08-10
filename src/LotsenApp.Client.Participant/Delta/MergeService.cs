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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using LotsenApp.Client.Participant.Header;
using LotsenApp.Client.Participant.Model;
using Microsoft.Extensions.Logging;

namespace LotsenApp.Client.Participant.Delta
{
    public class MergeService : IMergeService
    {
        private readonly ParticipantCryptographyService _participantCryptographyService;
        private readonly IParticipantHeaderService _headerService;
        private readonly ILogger<MergeService> _logger;

        public MergeService(ParticipantCryptographyService participantCryptographyService,
            IParticipantHeaderService headerService, ILogger<MergeService> logger)
        {
            _participantCryptographyService = participantCryptographyService;
            _headerService = headerService;
            _logger = logger;
        }
        
        public async Task<EncryptedParticipantModel> MergeModelWithDelta(string userId, EncryptedParticipantModel model,
            EncryptedDeltaFile deltaFile)
        {
            if (model.SaveFileTimestamp != deltaFile.SaveFileTimestamp)
            {
                throw new Exception("The delta is not applicable to the current save state");
            }

            var decryptedModel = await _participantCryptographyService.DecryptDataAsync(model, userId);
            decryptedModel.Header = await _participantCryptographyService.DecryptHeaderAsync(model, userId);
            var decryptedDelta = await _participantCryptographyService.DecryptDeltaFile(deltaFile, userId);
            var newModel = MergeFile(decryptedModel, decryptedDelta, userId);
            return await _participantCryptographyService.EncryptModel(newModel, userId);
        }

        internal ParticipantModel MergeFile(ParticipantModel model, DeltaFile delta, string userId = "")
        {
            var body = model.Data;
            if (delta.Documents.Count == 0)
            {
                _logger.LogDebug("No merge needs to be performed. There are no changes.");
                return model;
            }

            var changedDocuments = new List<Document>();
            foreach (var documentDelta in delta.Documents.Values)
            {
                var id = documentDelta.Id;
                var modelDocument = body.Documents.ContainsKey(id) ? body.Documents[id] : null;
                _logger.LogDebug($"Applying delta {documentDelta.Type} for document {id}" +
                                 (modelDocument != null ? "with preexisting document" : ""));
                changedDocuments.Add(MergeDocument(modelDocument, documentDelta));
            }

            changedDocuments = changedDocuments.Where(d => d != null).ToList();

            var unchangedDocuments = body.Documents.Select(d => d.Value)
                .Where(d => changedDocuments.All(cd => d.Id != cd.Id) &&
                            (
                                !delta.Documents.ContainsKey(d.Id) || delta.Documents[d.Id].Type != DeltaType.Delete
                            )
                );
            body.Documents = unchangedDocuments
                .Concat(changedDocuments)
                .ToDictionary(k => k.Id, v => v);
            body.DocumentTree = delta.DocumentTree;

            // rebuild header
            return _headerService.CalculateHeader(userId, model);
        }

        internal Document MergeDocument(Document document, DocumentDelta delta)
        {
            switch (delta?.Type)
            {
                case DeltaType.Create:
                    _logger.LogDebug($"Creating new document for {delta.Id}");
                    return new Document
                    {
                        Id = delta.Id,
                        DocumentId = delta.DocumentId,
                        Name = delta.Value,
                        Ordinal = delta.Ordinal ?? 99,
                        Values = delta.Values.Select(f => MergeField(null, f.Value))
                            .Where(f => f != null)
                            .ToDictionary(k => k.Id, v => v),
                        Groups = delta.Groups.Select(g => MergeGroup(null, g.Value))
                            .Where(g => g != null)
                            .ToDictionary(k => k.Id, v => v)
                    };
                case DeltaType.Delete:
                    _logger.LogDebug($"Deleting document {delta.Id}");
                    return null;
                case DeltaType.Unchanged:
                    _logger.LogDebug($"Document {delta.Id} was not touched");
                    UpdateChildren(document, delta);
                    break;
                case DeltaType.Update:
                    _logger.LogDebug($"Updating document {delta.Id}");
                    UpdateChildren(document, delta);
                    document.Ordinal = delta.Ordinal ?? document.Ordinal;
                    document.Name = delta.Value ?? document.Name;
                    break;
                default:
                    return null;
            }

            return document;
        }

        internal void UpdateChildren(Document document, DocumentDelta delta)
        {
            // fields
            var updatedValues = delta.Values.Select(d => d.Value)
                .Select(v => MergeField(document.Values.ContainsKey(v.Id) ? document.Values[v.Id] : null, v))
                .Where(f => f != null)
                .ToList();
            var unchangedValues = document.Values.Select(d => d.Value)
                .Where(f => updatedValues.All(cd => f.Id != cd.Id) &&
                            (
                                !delta.Values.ContainsKey(f.Id) || delta.Values[f.Id].Type != DeltaType.Delete
                            ));
            document.Values = unchangedValues
                .Concat(updatedValues)
                .ToDictionary(k => k.Id, v => v);

            // groups
            var updatedGroups = delta.Groups.Select(d => d.Value)
                .Select(v => MergeGroup(document.Groups.ContainsKey(v.Id) ? document.Groups[v.Id] : null, v))
                .Where(f => f != null)
                .ToList();
            var unchangedGroups = document.Groups.Select(g => g.Value)
                .Where(f => updatedGroups.All(cd => f.Id != cd.Id) &&
                            (
                                !delta.Groups.ContainsKey(f.Id) || delta.Groups[f.Id].Type != DeltaType.Delete
                            ));
            document.Groups = unchangedGroups
                .Concat(updatedGroups)
                .ToDictionary(k => k.Id, v => v);
        }

        internal DocumentGroup MergeGroup(DocumentGroup group, GroupDelta delta)
        {
            switch (delta?.Type)
            {
                case DeltaType.Create:
                    return new DocumentGroup
                    {
                        Id = delta.Id,
                        GroupId = delta.GroupId,
                        Ordinal = delta.Ordinal ?? 99,
                        Children = delta.Children.Select(c => MergeGroup(null, c.Value))
                            .ToDictionary(k => k.Id, v => v),
                        Fields = delta.Fields.Select(c => MergeField(null, c.Value))
                            .ToDictionary(k => k.Id, v => v),
                    };
                case DeltaType.Delete:
                    return null;
                case DeltaType.Unchanged:
                    UpdateGroupChildren(group, delta);
                    break;
                case DeltaType.Update:
                    UpdateGroupChildren(group, delta);
                    group.Ordinal = delta.Ordinal ?? group.Ordinal;
                    break;
                default:
                    return null;
            }

            return group;
        }

        internal void UpdateGroupChildren(DocumentGroup group, GroupDelta delta)
        {
            // children groups
            var updatedGroups = delta.Children.Select(c => c.Value)
                .Select(d => MergeGroup(group.Children.ContainsKey(d.Id) ? group.Children[d.Id] : null, d))
                .Where(d => d != null) // Remove deleted
                .ToList();
            var unchangedGroups = group.Children.Select(c => c.Value)
                .Where(d => updatedGroups.All(cd => d.Id != cd.Id) &&
                            (
                                !delta.Children.ContainsKey(d.Id) || delta.Children[d.Id].Type != DeltaType.Delete
                            ));
            group.Children = unchangedGroups
                .Concat(updatedGroups)
                .ToDictionary(k => k.Id, v => v);

            // fields
            var updatedValues = delta.Fields.Select(f => f.Value)
                .Select(v => MergeField(group.Fields.ContainsKey(v.Id) ? group.Fields[v.Id] : null, v))
                .Where(f => f != null)
                .ToList();
            var unchangedValues = group.Fields.Select(f => f.Value)
                .Where(f => updatedValues.All(cd => f.Id != cd.Id) &&
                            (
                                !delta.Fields.ContainsKey(f.Id) || delta.Fields[f.Id].Type != DeltaType.Delete
                            ));
            group.Fields = unchangedValues
                .Concat(updatedValues)
                .ToDictionary(k => k.Id, v => v);
        }

        internal DocumentField MergeField(DocumentField field, ValueDelta delta)
        {
            switch (delta?.Type)
            {
                case DeltaType.Create:
                    return new DocumentField
                    {
                        Id = delta.Id,
                        Value = delta.Value,
                        UseDisplay = delta.UseDisplay
                    };
                case DeltaType.Delete:
                    return null;
                case DeltaType.Unchanged:
                    return field;
                case DeltaType.Update:
                    field.Value = delta.Value;
                    field.UseDisplay = delta.UseDisplay;
                    break;
                default:
                    return null;
            }

            return field;
        }
    }
}