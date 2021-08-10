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

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LotsenApp.Client.DataFormat.Definition;
using LotsenApp.Client.DataFormat.Display;
using LotsenApp.Client.DataFormat.Migrations.V1;
using MoreLinq;

namespace LotsenApp.Client.DataFormat.Migrations
{
    [ExcludeFromCodeCoverage]
    public class LegacyMigratorV1V2
    {
        public Project MigrateProject(OldProjectFormat oldFormat)
        {
            var documents = oldFormat
                .Documents
                .Select(MigrateDocument)
                .ToArray();
            var documentationEvents = oldFormat
                .DocumentationEvents
                .Select(MigrateDocumentationEvents)
                .ToArray();
            var documentEventsDocuments = oldFormat
                .DocumentationEvents
                .SelectMany(de => de.ValidDocuments)
                .Select(MigrateDocument)
                .ToArray();

            var uniqueDocuments = documents
                .Select(d => d.Item1)
                .Concat(documentEventsDocuments.Select(de => de.Item1))
                .DistinctBy(d => d.Id)
                .ToList();
            var uniqueGroups = documents
                .SelectMany(d => d.Item2)
                .Concat(documentEventsDocuments.SelectMany(de => de.Item2))
                .DistinctBy(d => d.Id)
                .ToList();
            var uniqueDataFields = documents
                .SelectMany(d => d.Item3)
                .Concat(documentEventsDocuments.SelectMany(de => de.Item3))
                .DistinctBy(d => d.Id)
                .ToList();
            var uniqueDataTypes = documents
                .SelectMany(d => d.Item4)
                .Concat(documentEventsDocuments.SelectMany(de => de.Item4))
                .DistinctBy(d => d.Id)
                .ToList();
            var uniqueDisplayDocuments = documents
                .Select(d => d.Item5)
                .Concat(documentEventsDocuments.Select(d => d.Item5))
                .DistinctBy(d => d.Id)
                .ToList();
            var uniqueDisplayGroups = documents
                .SelectMany(d => d.Item6)
                .Concat(documentEventsDocuments.SelectMany(d => d.Item6))
                .DistinctBy(d => d.Id)
                .ToList();
            var uniqueDisplayDataFields = documents
                .SelectMany(d => d.Item7)
                .Concat(documentEventsDocuments.SelectMany(d => d.Item7))
                .DistinctBy(d => d.Id)
                .ToList();
            var uniqueDisplayDataTypes = documents
                .SelectMany(d => d.Item8)
                .Concat(documentEventsDocuments.SelectMany(d => d.Item8))
                .DistinctBy(d => d.Id)
                .ToList();
            return new Project
            {
                Id = oldFormat.Id,
                Name = oldFormat.Name,
                I18NKey = oldFormat.I18NKey,
                OpenForDocumentation = oldFormat.OpenForDocumentation,
                DataDefinition = new DataDefinition
                {
                    Documents = uniqueDocuments,
                    DocumentationEvents = documentationEvents
                        .Select(d => d.Item1)
                        .ToList(),
                    Groups = uniqueGroups,
                    DataFields = uniqueDataFields,
                    DataTypes = uniqueDataTypes
                },
                DataDisplay = new DataDisplay
                {
                    Documents = uniqueDisplayDocuments,
                    Groups = uniqueDisplayGroups,
                    DataFields = uniqueDisplayDataFields,
                    DataTypes = uniqueDisplayDataTypes,
                    DocumentationEvents = documentationEvents
                        .Select(de => de.Item2)
                        .ToList()
                }
            };
        }

        internal (Document, Group[], DataField[], DataType[], DocumentDisplay, GroupDisplay[], DataFieldDisplay[], DataTypeDisplay[])
            MigrateDocument(OldDocumentFormat oldDocumentFormat)
        {
            var dataFields = oldDocumentFormat.Fields?
                .Select(MigrateDataField)
                .ToArray();
            var groups = oldDocumentFormat.Groups?
                .Select(MigrateGroup)
                .ToArray();
            var document = new Document
            {
                Id = oldDocumentFormat.Id,
                DocumentType = oldDocumentFormat.Type,
                Groups = oldDocumentFormat.Groups?.Select(g => g.Id).ToList(),
                DataFields = oldDocumentFormat.Fields?.Select(f => f.Id).ToList()
            };
            return (document, 
                groups?.SelectMany(g => g.Item1)
                    .ToArray(),
                (dataFields?.Select(d => d.Item1) ?? Enumerable.Empty<DataField>())
                .Concat(groups?.SelectMany(g => g.Item2) ?? Enumerable.Empty<DataField>())
                .ToArray(),
                (dataFields?.Select(d => d.Item2) ?? Enumerable.Empty<DataType>())
                .Concat(groups?.SelectMany(g => g.Item3) ?? Enumerable.Empty<DataType>())
                    .ToArray(),
                new DocumentDisplay
                {
                    Id = document.Id,
                    Ordinal = oldDocumentFormat.Ordinal ?? 999,
                    Groups = groups?.Select((g, i) => new DocumentGroupDisplay
                    {
                        Id = g.Item4.Id,
                        Ordinal = i
                    }).ToList(),
                    DataFields = dataFields?.Select((f, i) => new DocumentDataFieldDisplay
                    {
                        Id = f.Item1.Id,
                        Ordinal = i
                    }).ToList()
                }, groups?
                    .Select(g => g.Item4)
                    .ToArray(),
                dataFields?
                    .Select(f => f.Item3)
                    .Concat(groups?.SelectMany(g => g.Item5) ?? Enumerable.Empty<DataFieldDisplay>())
                    .ToArray(),
                dataFields?
                    .Select(f => f.Item4)
                    .Concat(groups?.SelectMany(g => g.Item6) ?? Enumerable.Empty<DataTypeDisplay>())
                    .ToArray());
        }

        internal (DocumentationEvent, DocumentationEventDisplay)
            MigrateDocumentationEvents(OldDocumentationEventFormat oldDocumentationEventFormat)
        {
            var documentationEvent = new DocumentationEvent
            {
                Id = oldDocumentationEventFormat.Id,
                Name = oldDocumentationEventFormat.Name,
                DocumentId = oldDocumentationEventFormat.DocumentId,
                DaysAfterIncident = oldDocumentationEventFormat.DaysAfterIncident,
                ValidDocuments = oldDocumentationEventFormat.ValidDocuments?.Select(v => v.Id)?.ToList()
            };
            return (documentationEvent, new DocumentationEventDisplay
            {
                Id = oldDocumentationEventFormat.Id,
                I18NKey = oldDocumentationEventFormat.I18NKey,
                Ordinal = oldDocumentationEventFormat.Ordinal,
                ValidDocuments = oldDocumentationEventFormat.ValidDocuments?
                    .Select((v, i) => new DocumentDocumentationEventDisplay
                    {
                        Id = v.Id,
                        Ordinal = i
                    }).ToList()
            });
        }

        internal (Group[], DataField[], DataType[], GroupDisplay, DataFieldDisplay[], DataTypeDisplay[]) MigrateGroup(
            OldGroupFormat oldGroupFormat)
        {
            (DataField, DataType, DataFieldDisplay, DataTypeDisplay)[] dataFields = null;
            (Group[], DataField[], DataType[], GroupDisplay, DataFieldDisplay[], DataTypeDisplay[])[] children = null;
            if (oldGroupFormat.Fields != null)
            {
                dataFields = oldGroupFormat.Fields.Select(MigrateDataField).ToArray();
            }

            if (oldGroupFormat.Children != null)
            {
                children = oldGroupFormat.Children.Select(MigrateGroup).ToArray();
            }

            var groups = new[]
            {
                new Group
                {
                    Id = oldGroupFormat.Id,
                    Name = oldGroupFormat.Name,
                    Cardinality = oldGroupFormat.Cardinality,
                    Children = oldGroupFormat.Children?.Select(c => c.Id).ToList(),
                    Fields = oldGroupFormat.Fields?.Select(d => d.Id).ToList()
                }
            };
            return (groups.Concat(children?.SelectMany(c => c.Item1) ?? Enumerable.Empty<Group>()).ToArray(),
                dataFields?.Select(d => d.Item1).ToArray(),
                dataFields?.Select(d => d.Item2).ToArray(),
                new GroupDisplay
                {
                    Id = oldGroupFormat.Id,
                    Ordinal = 999,
                    I18NKey = oldGroupFormat.I18NKey,
                    Children = children?.Select(g => g.Item4).ToList(),
                    DataFields = oldGroupFormat.Fields?.Select((f, i) => new GroupDataFieldDisplay
                    {
                        Id = f.Id,
                        Ordinal = i
                    }).ToList()
                }, dataFields?.Select(d => d.Item3).ToArray(),
                dataFields?.Select(d => d.Item4).ToArray());
        }

        internal (DataField, DataType, DataFieldDisplay, DataTypeDisplay) MigrateDataField(
            OldDataFieldFormat oldDataFieldFormat)
        {
            DataType dataType = null;
            DataTypeDisplay dataTypeDisplay = null;
            if (oldDataFieldFormat.Type != null)
            {
                (dataType, dataTypeDisplay) = MigrateDataType(oldDataFieldFormat.Type);
            }

            return (new DataField
                {
                    Id = oldDataFieldFormat.Id,
                    Name = oldDataFieldFormat.Name,
                    Expression = oldDataFieldFormat.Expression,
                    DataType = dataType?.Id
                },
                dataType,
                new DataFieldDisplay
                {
                    Id = oldDataFieldFormat.Id,
                    Expression = oldDataFieldFormat.Expression,
                    I18NKey = oldDataFieldFormat.I18NKey
                }, dataTypeDisplay);
        }

        internal (DataType, DataTypeDisplay) MigrateDataType(OldDataTypeFormat oldDataTypeFormat)
        {
            var values = oldDataTypeFormat.Values?.Split(",");
            var displayValues1 = oldDataTypeFormat.DisplayValue1?.Split(",");
            var displayValues2 = oldDataTypeFormat.DisplayValue2?.Split(",");
            var displayValues3 = oldDataTypeFormat.DisplayValue3?.Split(",");
            return (new DataType
            {
                Id = oldDataTypeFormat.Id,
                Type = oldDataTypeFormat.Type,
                Values = oldDataTypeFormat.Values
            }, new DataTypeDisplay
            {
                Id = oldDataTypeFormat.Id,
                Expression = oldDataTypeFormat.Values,
                DisplayValues = values?
                    .Select((v, i) => (v,
                        new[]
                        {
                            i < displayValues1?.Length ? displayValues1[i] : null,
                            i < displayValues2?.Length ? displayValues2[i] : null,
                            i < displayValues3?.Length ? displayValues3[i] : null
                        }.Where(dv => dv != null)))
                    .ToDictionary(v => v.v, v => v.Item2.ToArray())
            });
        }
    }
}