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
using DeepEqual.Syntax;
using LotsenApp.Client.DataFormat.Definition;
using LotsenApp.Client.DataFormat.Display;
using LotsenApp.Client.DataFormat.Migrations.V1;
using Xunit;

namespace LotsenApp.Client.DataFormat.Migrations.Test
{
    [ExcludeFromCodeCoverage]
    public class LegacyMigratorV1V2Test
    {
        private readonly LegacyMigratorV1V2 _migrator = new();

        [Fact]
        public void TestDataTypeMigration()
        {
            var oldDataType = new OldDataTypeFormat
            {
                Id = "22402179-04e9-4137-9993-0640341b9084",
                Type = Types.STRING,
                Values = "size=large",
                DisplayValue1 = null,
                DisplayValue2 = null,
                DisplayValue3 = null
            };
            var expectedMigration = new DataType
            {
                Id = "22402179-04e9-4137-9993-0640341b9084",
                Name = null,
                Type = Types.STRING,
                Values = "size=large"
            };
            var expectedMigrationDisplay = new DataTypeDisplay
            {
                Id = "22402179-04e9-4137-9993-0640341b9084",
                Expression = "size=large",
                DisplayValues = new Dictionary<string, string[]>
                {
                    {"size=large", Array.Empty<string>()}
                }
            };
            var (dataType, dataTypeDisplay) = _migrator.MigrateDataType(oldDataType);
            
            expectedMigration.ShouldDeepEqual(dataType);
            expectedMigrationDisplay.ShouldDeepEqual(dataTypeDisplay);
        }
        
        [Fact]
        public void TestDataTypeMigrationWithEmptyValues()
        {
            var oldDataType = new OldDataTypeFormat
            {
                Id = "22402179-04e9-4137-9993-0640341b9084",
                Type = Types.STRING,
                Values = null,
                DisplayValue1 = null,
                DisplayValue2 = null,
                DisplayValue3 = null
            };
            var expectedMigration = new DataType
            {
                Id = "22402179-04e9-4137-9993-0640341b9084",
                Name = null,
                Type = Types.STRING,
                Values = null
            };
            var expectedMigrationDisplay = new DataTypeDisplay
            {
                Id = "22402179-04e9-4137-9993-0640341b9084",
                DisplayValues = null
            };
            var (dataType, dataTypeDisplay) = _migrator.MigrateDataType(oldDataType);
            
            expectedMigration.ShouldDeepEqual(dataType);
            expectedMigrationDisplay.ShouldDeepEqual(dataTypeDisplay);
        }
        
        [Fact]
        public void TestDataTypeMigrationWithFewerDisplayValues()
        {
            var oldDataType = new OldDataTypeFormat
            {
                Id = "22402179-04e9-4137-9993-0640341b9084",
                Type = Types.ENUMERABLE,
                Values = "banana,apple",
                DisplayValue1 = "banana",
                DisplayValue2 = "B,A",
                DisplayValue3 = null
            };
            var expectedMigration = new DataType
            {
                Id = "22402179-04e9-4137-9993-0640341b9084",
                Name = null,
                Type = Types.ENUMERABLE,
                Values = "banana,apple"
            };
            var expectedMigrationDisplay = new DataTypeDisplay
            {
                Id = "22402179-04e9-4137-9993-0640341b9084",
                Expression = "banana,apple",
                DisplayValues = new Dictionary<string, string[]>
                {
                    {"banana", new []{"banana", "B"}},
                    {"apple", new []{"A"}}
                }
            };
            var (dataType, dataTypeDisplay) = _migrator.MigrateDataType(oldDataType);
            
            expectedMigration.ShouldDeepEqual(dataType);
            expectedMigrationDisplay.ShouldDeepEqual(dataTypeDisplay);
        }

        [Fact]
        public void TestDataFieldMigration()
        {
            var dataField = new OldDataFieldFormat
            {
                Id = "98f47308-5390-4943-ba0b-fed886be53b1",
                Name = "Specials",
                Expression = null,
                I18NKey = "Project.StrokeOwl.DataField.Specials",
            };

            var expectedMigration = new DataField
            {
                Id = "98f47308-5390-4943-ba0b-fed886be53b1",
                Name = "Specials",
                Expression = null,
            };
            var expectedMigrationDisplay = new DataFieldDisplay
            {
                Id = "98f47308-5390-4943-ba0b-fed886be53b1",
                I18NKey = "Project.StrokeOwl.DataField.Specials",
                Expression = null
            };
            var migration = _migrator.MigrateDataField(dataField);
            
            expectedMigration.ShouldDeepEqual(migration.Item1);
            expectedMigrationDisplay.ShouldDeepEqual(migration.Item3);
        }
        
        [Fact]
        public void TestMigrateGroup()
        {
            var oldGroup = new OldGroupFormat
            {
                Id = "c375279b-87b7-4294-b324-43232f12cb74",
                Name = "Medication",
                Cardinality = Cardinality.Many,
                I18NKey = "Project.StrokeOwl.Group.Medication",
                Children = new List<OldGroupFormat>
                {
                    new OldGroupFormat
                    {
                        Id = "67baa99d-6021-49b1-ac5f-d758b45462ce",
                        Name = "Dosage Noon",
                        I18NKey = "Project.StrokeOwl.DataField.DosageNoon",
                        Cardinality = Cardinality.One
                    }
                },
                Fields = new List<OldDataFieldFormat>
                {
                    new OldDataFieldFormat
                    {
                        Id = "49983f63-3c59-4629-b598-5168ce2c8c48",
                        Name = "Commercial Name",
                        I18NKey = "Project.StrokeOwl.DataField.CommercialName",
                        Expression = null
                    }
                }
            };
            var expectedMigration = new Group
            {
                Id = "c375279b-87b7-4294-b324-43232f12cb74",
                Name = "Medication",
                Cardinality = Cardinality.Many,
                Children = new List<string>
                {
                    "67baa99d-6021-49b1-ac5f-d758b45462ce",
                },
                Fields = new List<string>
                {
                    "49983f63-3c59-4629-b598-5168ce2c8c48",
                }
            };
            var expectedMigrationDisplay = new GroupDisplay
            {
                Id = "c375279b-87b7-4294-b324-43232f12cb74",
                Ordinal = 999,
                I18NKey = "Project.StrokeOwl.Group.Medication",
                Children = new List<GroupDisplay>
                {
                    new GroupDisplay
                    {
                        Id = "67baa99d-6021-49b1-ac5f-d758b45462ce",
                        Ordinal = 999,
                        I18NKey = "Project.StrokeOwl.DataField.DosageNoon"
                    }
                },
                DataFields = new List<GroupDataFieldDisplay>
                {
                    new GroupDataFieldDisplay
                    {
                        Id = "49983f63-3c59-4629-b598-5168ce2c8c48",
                        Ordinal = 0
                    }
                }
            };
            var migration = _migrator.MigrateGroup(oldGroup);
            
            expectedMigration.ShouldDeepEqual(migration.Item1[0]);
            expectedMigrationDisplay.ShouldDeepEqual(migration.Item4);
        }

        [Fact]
        public void MigrateDocumentationEvent()
        {
            var oldDocumentationEventFormat = new OldDocumentationEventFormat
            {
                Id = "b7728188-f560-4658-bb2c-cd3f72791e4c",
                Name = "T1",
                ProjectId = "a163d5f4-65dc-451b-a776-ee2329f0f69e",
                DaysAfterIncident = 90,
                I18NKey = "Project.StrokeOwl.T1",
                Version = 0,
                Ordinal = 1,
                DocumentId = "00eb3a04-dbc9-4711-8f77-be5576e1bcca",
                ValidDocuments = new List<OldDocumentFormat>()
            };
            var expectedMigration = new DocumentationEvent
            {
                Id = "b7728188-f560-4658-bb2c-cd3f72791e4c",
                Name = "T1",
                DocumentId = "00eb3a04-dbc9-4711-8f77-be5576e1bcca",
                DaysAfterIncident = 90,
                ValidDocuments = new List<string>()
            };
            var expectedMigrationDisplay = new DocumentationEventDisplay
            {
                Id = "b7728188-f560-4658-bb2c-cd3f72791e4c",
                Ordinal = 1,
                I18NKey = "Project.StrokeOwl.T1",
            };
            var migration = _migrator.MigrateDocumentationEvents(oldDocumentationEventFormat);
            
            expectedMigration.ShouldDeepEqual(migration.Item1);
            expectedMigrationDisplay.ShouldDeepEqual(migration.Item2);
        }

        [Fact]
        public void TestDocumentMigration()
        {
            var oldDocumentFormat = new OldDocumentFormat
            {
                Id = "00eb3a04-dbc9-4711-8f77-be5576e1bcca",
                Type = DocumentType.Project,
                Fields = new List<OldDataFieldFormat>(),
                Groups = new List<OldGroupFormat>()
            };
            var expectedMigration = new Document
            {
                Id = "00eb3a04-dbc9-4711-8f77-be5576e1bcca",
                DocumentType = DocumentType.Project,
                Groups = new List<string>(),
                DataFields = new List<string>()
            };
            var expectedMigrationDisplay = new DocumentDisplay
            {
                Id = "00eb3a04-dbc9-4711-8f77-be5576e1bcca",
                Ordinal = 999,
                Groups = new List<DocumentGroupDisplay>(),
                DataFields = new List<DocumentDataFieldDisplay>()
            };
            var migration = _migrator.MigrateDocument(oldDocumentFormat);
            
            expectedMigration.ShouldDeepEqual(migration.Item1);
            expectedMigrationDisplay.ShouldDeepEqual(migration.Item5);
        }
    }
}