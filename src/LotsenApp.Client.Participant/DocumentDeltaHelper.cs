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
using LotsenApp.Client.Participant.Delta;
using LotsenApp.Client.Participant.Model;

namespace LotsenApp.Client.Participant
{
    public class DocumentDeltaHelper
    {
        internal string[] FindTreeItem(IDictionary<string, TreeItem> tree, string documentId)
        {
            if (tree.ContainsKey(documentId))
            {
                return new[] {documentId};
            }

            foreach (var document in tree.Values)
            {
                var result = FindTreeItem(document.Children, documentId);
                if (result.Length > 0)
                {
                    return result.Prepend(document.Id).ToArray();
                }
            }

            return Array.Empty<string>();
        }

        [ExcludeFromCodeCoverage]
        internal DocumentDelta FindDelta(IDictionary<string, DocumentDelta> deltas, params string[] path)
        {
            var targetDocumentId = path.LastOrDefault();
            if (targetDocumentId == null)
            {
                return null;
            }

            return deltas.ContainsKey(targetDocumentId) ? deltas[targetDocumentId] : null;
        }

        [ExcludeFromCodeCoverage]
        internal string[] FindGroup(IDictionary<string, DocumentGroup> groups, IDictionary<string, GroupDelta> deltas,
            string groupId)
        {
            var deltaSearch = deltas.ContainsKey(groupId) ? deltas[groupId] : null;
            var documentSearch = groups.ContainsKey(groupId) ? groups[groupId] : null;
            if (deltaSearch != null || documentSearch != null)
            {
                return new[] {groupId};
            }

            foreach (var group in groups.Values)
            {
                var relatedDelta = deltas.ContainsKey(group.Id)
                    ? deltas[group.Id]
                    : new GroupDelta
                    {
                        Type = DeltaType.Unchanged,
                    };

                var foundDeltas = FindGroup(group.Children, relatedDelta.Children, groupId);
                if (foundDeltas.Length > 0)
                {
                    return foundDeltas.Prepend(group.Id).ToArray();
                }
            }

            foreach (var delta in deltas.Values)
            {
                var relatedDocument = groups.ContainsKey(delta.Id) ? groups[delta.Id] : new DocumentGroup();

                var foundDeltas = FindGroup(relatedDocument.Children, delta.Children, groupId);
                if (foundDeltas.Length > 0)
                {
                    return foundDeltas.Prepend(delta.Id).ToArray();
                }
            }

            return new string[0];
        }

        internal DocumentDelta ResolveDelta(IDictionary<string, DocumentDelta> deltas, string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var delta = deltas.ContainsKey(id) ? deltas[id] : null;
            if (delta == null)
            {
                delta = new DocumentDelta
                {
                    Id = id,
                    Type = DeltaType.Unchanged,
                    TimeStamp = DateTime.UtcNow,
                };
                deltas.Add(delta.Id, delta);
            }

            return delta;
        }

        internal TreeItem ResolveTreeItem(IDictionary<string, TreeItem> tree, params string[] path)
        {
            while (path.Length >= 1)
            {
                var currentLevelId = path[0];
                var treeItem = tree.ContainsKey(currentLevelId) ? tree[currentLevelId] : null;
                if (treeItem == null)
                {
                    break;
                }

                if (path.Length == 1)
                {
                    return treeItem;
                }

                tree = treeItem.Children;
                path = path.Skip(1).ToArray();
            }

            return null;
        }

        internal IDictionary<string, GroupDelta> ResolveGroup(IDictionary<string, GroupDelta> deltas,
            Action<GroupDelta> operation, params string[] path)
        {
            if (path.Length < 1)
            {
                return deltas;
            }

            var newDeltas = new List<GroupDelta>(deltas.Values);

            var currentLevelId = path[0];
            var delta = deltas.ContainsKey(currentLevelId) ? deltas[currentLevelId] : null;
            if (delta == null)
            {
                delta = new GroupDelta
                {
                    Id = currentLevelId,
                    Type = DeltaType.Unchanged,
                    TimeStamp = DateTime.UtcNow,
                };
                newDeltas.Add(delta);
            }

            if (path.Length == 1)
            {
                operation.Invoke(delta);
            }
            else
            {
                delta.Children = ResolveGroup(delta.Children, operation, path.Skip(1).ToArray());
            }

            return newDeltas.ToDictionary(k => k.Id, v => v);
        }

        internal DocumentField ResolveField(ParticipantModel model, params string[] path)
        {
            if (path.Length <= 0)
            {
                return null;
            }

            var documentId = path[0];
            return ResolveField(model.Data.Documents[documentId], path.Skip(1).ToArray());
        }

        internal DocumentField ResolveField(Document document, params string[] path)
        {
            if (path.Length == 0)
            {
                return null;
            }

            var currentId = path[0];
            return path.Length > 1
                ? ResolveField(document.Groups[currentId], path.Skip(1).ToArray())
                : document.Values.ContainsKey(currentId) ? document.Values[currentId] : null;
        }

        internal DocumentField ResolveField(DocumentGroup group, params string[] path)
        {
            if (path.Length == 0)
            {
                return null;
            }


            while (path.Length > 1)
            {
                var currentId = path[0];
                if (!group.Children.ContainsKey(currentId))
                {
                    return null;
                }
                group = group.Children[currentId];
                path = path.Skip(1).ToArray();
            }

            var fieldId = path[0];
            return !group.Fields.ContainsKey(fieldId) ? null : group.Fields[fieldId];
        }
        
        internal DocumentField[] ResolveFieldsWithProjectPath(ParticipantModel model, params string[] path)
        {
            if (path.Length <= 0)
            {
                return Array.Empty<DocumentField>();
            }

            var documentId = path[0];
            var validDocuments = model.Data.Documents.Values.Where(d => d.DocumentId == documentId);
            var fields = validDocuments.SelectMany(v => ResolveFieldsWithProjectPath(v, path.Skip(1).ToArray()));
            return fields.ToArray();
        }

        internal DocumentField[] ResolveFieldsWithProjectPath(Document document, params string[] path)
        {
            if (path.Length == 0 || document == null)
            {
                return Array.Empty<DocumentField>();
            }

            var currentId = path[0];
            if (path.Length > 1)
            {
                var validGroups = document.Groups.Values.Where(g => g.GroupId == currentId);
                return validGroups.SelectMany(g => ResolveFieldsWithProjectPath(g, path.Skip(1).ToArray())).ToArray();
            }

            return document.Values.Values.Where(v => v.Id == currentId).ToArray();
        }

        internal DocumentField[] ResolveFieldsWithProjectPath(DocumentGroup group, params string[] path)
        {
            if (path.Length == 0)
            {
                return Array.Empty<DocumentField>();
            }


            while (path.Length > 1)
            {
                var currentId = path[0];
                group = group.Children.Values.FirstOrDefault(g => g.GroupId == currentId);
                if (group == null)
                {
                    return Array.Empty<DocumentField>();
                }
                path = path.Skip(1).ToArray();
            }

            var fieldId = path[0];
            return group.Fields.Values.Where(f => f.Id == fieldId).ToArray();
        }
    }
}