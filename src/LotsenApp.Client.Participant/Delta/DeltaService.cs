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
using LotsenApp.Client.Participant.Dto;
using LotsenApp.Client.Participant.Model;

namespace LotsenApp.Client.Participant.Delta
{
    public class DeltaService : IDeltaService
    {
        private readonly ParticipantCryptographyService _participantCryptographyService;
        private readonly DocumentDeltaHelper _helper;

        public DeltaService(ParticipantCryptographyService participantCryptographyService, DocumentDeltaHelper helper)
        {
            _participantCryptographyService = participantCryptographyService;
            _helper = helper;
        }

        public async Task<EncryptedDeltaFile> UpdateDocument(string userId, EncryptedParticipantModel model,
            EncryptedDeltaFile currentDelta,
            IDocumentChange dto)
        {
            var decryptedDelta = await _participantCryptographyService.DecryptDeltaFile(currentDelta, userId);
            var decryptedModel = await _participantCryptographyService.DecryptDataAsync(model, userId);

            UpdateDocument(decryptedDelta, decryptedModel, dto);

            return await _participantCryptographyService.EncryptDeltaFile(decryptedDelta, userId);
        }

        internal void UpdateDocument(DeltaFile delta, ParticipantModel model, IDocumentChange dto)
        {
            var relatedDocument = model.Data.Documents.ContainsKey(dto.Id) ? model.Data.Documents[dto.Id] : null;
            var targetDelta = _helper.ResolveDelta(delta.Documents, dto.Id);

            if (targetDelta.Type == DeltaType.Delete)
            {
                return;
            }

            var targetType = relatedDocument?.Name == dto.Name ? DeltaType.Unchanged : DeltaType.Update;
            targetDelta.Type = targetDelta.Type != DeltaType.Create ? targetType : DeltaType.Create;
            targetDelta.OldValue = relatedDocument?.Name ?? targetDelta.Value;
            targetDelta.Value = dto.Name;
            targetDelta.DocumentId = dto.DocumentId ?? targetDelta.DocumentId;
            targetDelta.Groups = UpdateGroups(targetDelta.Groups, relatedDocument?.Groups, dto.Groups);
            targetDelta.Values = UpdateFields(targetDelta.Values, relatedDocument?.Values, dto.Fields);
            CalculateTree(delta);
        }

        internal IDictionary<string, GroupDelta> UpdateGroups(IDictionary<string, GroupDelta> deltas,
            IDictionary<string, DocumentGroup> relatedGroups, IGroupChange[] dtos)
        {
            foreach (var dto in dtos)
            {
                GroupDelta relatedDelta;
                var relatedGroup = relatedGroups?.ContainsKey(dto.Id) ?? false ? relatedGroups[dto.Id] : null;
                if (deltas.ContainsKey(dto.Id))
                {
                    relatedDelta = deltas[dto.Id];
                }
                else
                {
                    relatedDelta = new GroupDelta
                    {
                        Id = dto.Id,
                        TimeStamp = DateTime.UtcNow,
                        GroupId = relatedGroup?.GroupId,
                        Type = relatedGroup != null ? DeltaType.Update : DeltaType.Create,
                    };
                    deltas.Add(dto.Id, relatedDelta);
                }

                relatedDelta.GroupId = dto.GroupId ?? relatedDelta.GroupId;
                relatedDelta.Fields = UpdateFields(relatedDelta.Fields, relatedGroup?.Fields, dto.Fields);
                relatedDelta.Children = UpdateGroups(relatedDelta.Children, relatedGroup?.Children, dto.Children);
                relatedDelta.Ordinal = dtos.IndexOf(dto);
            }

            return deltas
                .Where(d => dtos.Any(dto => dto.Id == d.Key))
                .ToDictionary(k => k.Key, v => v.Value);
        }

        internal IDictionary<string, ValueDelta> UpdateFields(IDictionary<string, ValueDelta> deltas,
            IDictionary<string, DocumentField> relatedFields, IFieldChange[] dtos)
        {
            IEnumerable<ValueDelta> currentDeltas = deltas.Values;
            foreach (var dto in dtos)
            {
                var relatedDelta = deltas.ContainsKey(dto.Id) ? deltas[dto.Id] : null;
                var relatedField = relatedFields?.ContainsKey(dto.Id) ?? false ? relatedFields[dto.Id] : null;
                if (relatedField?.Value == dto.Value && relatedField?.UseDisplay == dto.UseDisplay)
                {
                    continue;
                }

                if (relatedDelta == null)
                {
                    relatedDelta = new ValueDelta
                    {
                        Id = dto.Id,
                        Type = DeltaType.Create,
                        Timestamp = DateTime.UtcNow,
                    };
                    currentDeltas = currentDeltas.Append(relatedDelta);
                }

                var deltaType = dto.Value == relatedField?.Value ? DeltaType.Unchanged : DeltaType.Update;
                relatedDelta.Type = relatedDelta.Type != DeltaType.Create ? deltaType : DeltaType.Create;
                relatedDelta.Value = dto.Value;
                relatedDelta.OldValue = relatedField?.Value;
                relatedDelta.OldUseDisplay = relatedField?.UseDisplay;
                relatedDelta.UseDisplay = dto.UseDisplay;
            }

            return currentDeltas
                .Where(d => d.Type != DeltaType.Unchanged) // Unchanged value deltas are not needed.
                .ToDictionary(k => k.Id, v => v);
        }

        public async Task<EncryptedDeltaFile> ReorderGroups(string userId, EncryptedDeltaFile encryptedDelta,
            string documentId, ReOrderDto[] orderDtos)
        {
            var decryptedDelta = await _participantCryptographyService.DecryptDeltaFile(encryptedDelta, userId);
            var relatedDelta = _helper.ResolveDelta(decryptedDelta.Documents, documentId);
            relatedDelta.Groups = ReArrangeGroups(relatedDelta.Groups, orderDtos);
            return await _participantCryptographyService.EncryptDeltaFile(decryptedDelta, userId);
        }

        internal IDictionary<string, GroupDelta> ReArrangeGroups(IDictionary<string, GroupDelta> deltas,
            ReOrderDto[] orderDtos)
        {
            IEnumerable<GroupDelta> currentDeltas = deltas.Values;
            foreach (var orderDto in orderDtos)
            {
                var relatedDelta = deltas.ContainsKey(orderDto.Id) ? deltas[orderDto.Id] : null;
                if (relatedDelta == null)
                {
                    relatedDelta = new GroupDelta()
                    {
                        Id = orderDto.Id,
                    };
                    currentDeltas = currentDeltas.Append(relatedDelta);
                }

                relatedDelta.Ordinal = orderDtos.IndexOf(orderDto);
                relatedDelta.Children = ReArrangeGroups(relatedDelta.Children, orderDto.Documents);
            }

            return currentDeltas.ToDictionary(k => k.Id, v => v);
        }

        public async Task<(EncryptedDeltaFile, string groupId)> AddGroup(string userId, EncryptedDeltaFile deltaFile,
            CreateGroupDto dto)
        {
            var decryptedDelta = await _participantCryptographyService.DecryptDeltaFile(deltaFile, userId);
            var newId = AddGroup(decryptedDelta, dto);

            return (await _participantCryptographyService.EncryptDeltaFile(decryptedDelta, userId), newId);
        }

        internal string AddGroup(DeltaFile delta, CreateGroupDto dto)
        {
            var newId = Guid.NewGuid().ToString("N");
            var newGroup = new GroupDelta
            {
                Id = newId,
                Type = DeltaType.Create,
                GroupId = dto.GroupId,
                TimeStamp = DateTime.UtcNow
            };
            // Find document
            var documentPath = _helper.FindTreeItem(delta.DocumentTree, dto.DocumentId);
            var relatedDelta = _helper.ResolveDelta(delta.Documents, dto.DocumentId);

            if (dto.ParentGroupId == null)
            {
                relatedDelta.Groups.Add(newGroup.Id, newGroup);
                delta.DocumentTree = UpdateTree(documentPath.Append(newGroup.Id).ToArray(),
                    DeltaType.Create, delta.DocumentTree);
            }
            else
            {
                // Find parent group
                var relatedDocument = _helper.ResolveTreeItem(delta.DocumentTree, documentPath);

                var groupPath = _helper.FindTreeItem(relatedDocument.Children,
                    dto.ParentGroupId);
                // Add group delta
                relatedDelta.Groups = _helper.ResolveGroup(relatedDelta.Groups,
                    targetGroup => targetGroup.Children.Add(newGroup.Id, newGroup),
                    groupPath);
                delta.DocumentTree = UpdateTree(documentPath
                    .Concat(groupPath)
                    .Append(newGroup.Id)
                    .ToArray(), DeltaType.Create, delta.DocumentTree);
            }

            return newId;
        }

        public async Task<EncryptedDeltaFile> RemoveGroup(string userId,
            EncryptedDeltaFile deltaFile, string documentId, string groupId)
        {
            var decryptedDelta = await _participantCryptographyService.DecryptDeltaFile(deltaFile, userId);

            RemoveGroup(decryptedDelta, documentId, groupId);

            return await _participantCryptographyService.EncryptDeltaFile(decryptedDelta, userId);
        }

        internal DeltaFile RemoveGroup(DeltaFile delta, string documentId, string groupId)
        {
            // Find document
            var documentPath = _helper.FindTreeItem(delta.DocumentTree, documentId);
            var relatedDelta = _helper.ResolveDelta(delta.Documents, documentId);

            // Find parent group
            var relatedDocument = _helper.ResolveTreeItem(delta.DocumentTree, documentPath);

            var groupPath = _helper.FindTreeItem(relatedDocument.Children,
                groupId);
            // Add group delta
            relatedDelta.Groups = _helper.ResolveGroup(relatedDelta.Groups,
                targetGroup => { targetGroup.Type = DeltaType.Delete; },
                groupPath);

            delta.DocumentTree = UpdateTree(documentPath.Concat(groupPath).ToArray(), DeltaType.Delete,
                delta.DocumentTree);
            return delta;
        }

        public async Task<EncryptedDeltaFile> ReorderDocuments(string userId, EncryptedDeltaFile encryptedDelta,
            ReOrderDto[] orderDtos)
        {
            var decryptedDelta = await _participantCryptographyService.DecryptDeltaFile(encryptedDelta, userId);

            ReorderDocuments(decryptedDelta, orderDtos);
            return await _participantCryptographyService.EncryptDeltaFile(decryptedDelta, userId);
        }

        internal DeltaFile ReorderDocuments(DeltaFile delta, ReOrderDto[] orderDtos)
        {
            delta.Documents = ReArrangeDocuments(delta.Documents, orderDtos);
            // Deep copies of the document tree
            var treeClone = delta.DocumentTree.Values
                .Select(CreateTreeItem)
                .ToDictionary(k => k.Id, v => v);
            // Synchronize tree
            delta.DocumentTree =
                UpdateTree(treeClone, orderDtos);
            return delta;
        }

        internal IDictionary<string, TreeItem> UpdateTree(IDictionary<string, TreeItem> completeTree,
            ReOrderDto[] orderDtos)
        {
            var currentLevel = new List<TreeItem>();
            foreach (var orderDto in orderDtos)
            {
                var relatedTreeItemPath = _helper.FindTreeItem(completeTree, orderDto.Id);
                // Deep copy that will be manipulated
                var relatedTreeItem = CreateTreeItem(_helper.ResolveTreeItem(completeTree, relatedTreeItemPath));
                relatedTreeItem.Children = UpdateTree(completeTree, orderDto.Documents);
                currentLevel.Add(relatedTreeItem);
            }

            return currentLevel.ToDictionary(k => k.Id, v => v);
        }

        internal IDictionary<string, DocumentDelta> ReArrangeDocuments(IDictionary<string, DocumentDelta> deltas,
            ReOrderDto[] orderDtos)
        {
            foreach (var orderDto in orderDtos)
            {
                // Find related delta in the entire delta tree, if it exists
                var relatedDelta = deltas.ContainsKey(orderDto.Id) ? deltas[orderDto.Id] : null;
                if (relatedDelta == null)
                {
                    relatedDelta = new DocumentDelta
                    {
                        Id = orderDto.Id,
                        Type = DeltaType.Unchanged
                    };
                    deltas.Add(relatedDelta.Id, relatedDelta);
                }

                relatedDelta.Type = relatedDelta.Type != DeltaType.Create ? DeltaType.Update : DeltaType.Create;
                relatedDelta.Ordinal = orderDtos.IndexOf(orderDto);
                ReArrangeDocuments(deltas, orderDto.Documents);
            }

            return deltas;
        }

        public async Task<(EncryptedDeltaFile nextDelta, string documentId)> AddDocument(string userId,
            EncryptedDeltaFile currentDelta, CreateDocumentDto dto)
        {
            var decryptedDelta = await _participantCryptographyService.DecryptDeltaFile(currentDelta, userId);
            var documentId = AddDocument(decryptedDelta, dto);
            return (await _participantCryptographyService.EncryptDeltaFile(decryptedDelta, userId), documentId);
        }

        internal string AddDocument(DeltaFile delta, CreateDocumentDto dto)
        {
            var documentId = Guid.NewGuid().ToString("N");
            var newDelta = new DocumentDelta
            {
                Id = documentId,
                Type = DeltaType.Create,
                Ordinal = 99,
                DocumentId = dto.DocumentId,
                Value = dto.Name,
                TimeStamp = DateTime.UtcNow
            };
            // Always add the delta to the flat dictionary
            delta.Documents.Add(newDelta.Id, newDelta);

            // If no parent document exist, then add it to the first level of the document tree
            if (dto.ParentDocumentId == null)
            {
                delta.DocumentTree =
                    UpdateTree(new[] { newDelta.Id }, DeltaType.Create, delta.DocumentTree);
                return documentId;
            }

            // Otherwise look for the parent document
            var parentDocumentPath = _helper.FindTreeItem(delta.DocumentTree,
                dto.ParentDocumentId);
            var pathWithChild = parentDocumentPath.Append(documentId).ToArray();
            // And add the new child to the document tree
            delta.DocumentTree = UpdateTree(pathWithChild, DeltaType.Create, delta.DocumentTree);
            return documentId;
        }

        public async Task<EncryptedDeltaFile> DeleteDocument(string userId,
            EncryptedDeltaFile currentDelta,
            string documentId)
        {
            var decryptedDelta = await _participantCryptographyService.DecryptDeltaFile(currentDelta, userId);
            DeleteDocument(decryptedDelta, documentId);
            return await _participantCryptographyService.EncryptDeltaFile(decryptedDelta, userId);
        }

        internal DeltaFile DeleteDocument(DeltaFile delta, string documentId)
        {
            var path = _helper.FindTreeItem(delta.DocumentTree, documentId);
            var relatedDelta = _helper.ResolveDelta(delta.Documents, documentId);
            relatedDelta.Type = DeltaType.Delete;
            delta.DocumentTree = UpdateTree(path, DeltaType.Delete, delta.DocumentTree);
            return delta;
        }

        internal IDictionary<string, TreeItem> UpdateTree(string[] path, DeltaType type,
            IDictionary<string, TreeItem> tree)
        {
            var currentId = path[0];
            if (path.Length == 1 && DeltaType.Create == type)
            {
                tree.Add(currentId, new TreeItem
                {
                    Id = currentId
                });
                return tree;
            }

            if (path.Length == 1 && DeltaType.Delete == type)
            {
                tree.Remove(currentId);
                return tree;
            }

            if (!tree.ContainsKey(currentId))
            {
                return tree;
            }

            tree[currentId].Children = UpdateTree(path.Skip(1).ToArray(), type, tree[currentId].Children);
            return tree;
        }

        public async Task<EncryptedDeltaFile> SeedDeltaFile(string userId, EncryptedParticipantModel encryptedModel,
            EncryptedDeltaFile encryptedDelta)
        {
            var model = await _participantCryptographyService.DecryptDataAsync(encryptedModel, userId);
            var delta = await _participantCryptographyService.DecryptDeltaFile(encryptedDelta, userId);
            SeedDeltaFile(delta, model);
            return await _participantCryptographyService.EncryptDeltaFile(delta, userId);
        }

        internal DeltaFile SeedDeltaFile(DeltaFile delta, ParticipantModel model)
        {
            // The delta is already seeded
            if (delta.DocumentTree.Count > 0)
            {
                return delta;
            }

            delta.DocumentTree = model.Data.DocumentTree
                .Select(d => CreateTreeItem(d.Value))
                .ToDictionary(k => k.Id, v => v);
            return delta;
        }

        internal TreeItem CreateTreeItem(TreeItem treeItem)
        {
            return new()
            {
                Id = treeItem.Id,
                Children = treeItem.Children
                    .Select(d => CreateTreeItem(d.Value))
                    .ToDictionary(k => k.Id, v => v),
            };
        }

        internal void CalculateTree(DeltaFile deltaFile)
        {
            var existingDocuments = FlattenTree(deltaFile.DocumentTree);
            var missingDocuments = deltaFile.Documents
                .Where(d => !existingDocuments.Contains(d.Key))
                .Select(d => d.Value)
                .ToList();
            foreach (var document in missingDocuments)
            {
                deltaFile.DocumentTree.Add(document.Id, new TreeItem
                {
                    Id = document.Id
                });
            }
        }

        internal List<string> FlattenTree(IDictionary<string, TreeItem> tree)
        {
            return tree.Keys.Concat(tree.SelectMany(t => FlattenTree(t.Value.Children))).ToList();
        }
    }
}