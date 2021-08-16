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
using LotsenApp.Client.Authentication.DataPassword;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.DataFormat;
using LotsenApp.Client.DataFormat.Definition;
using LotsenApp.Client.DataFormat.Display;
using LotsenApp.Client.Participant.Delta;
using LotsenApp.Client.Participant.Dto;
using LotsenApp.Client.Participant.Model;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Document = LotsenApp.Client.Participant.Model.Document;

namespace LotsenApp.Client.Participant.Test
{
    [ExcludeFromCodeCoverage]
    public class ParticipantTransformationServiceTest
    {
        private Mock<DataPasswordService> _dataPasswordMock = new Mock<DataPasswordService>();
        private Mock<IConfigurationStorage> _configuration = new Mock<IConfigurationStorage>();
        private Mock<ParticipantCryptographyService> _cryptography;
        private ParticipantTransformationService CreateInstance()
        {
            
            var helper = new DocumentDeltaHelper();
            var storageMock = new Mock<IDataFormatStorage>();
            _cryptography = new Mock<ParticipantCryptographyService>(_dataPasswordMock.Object, _configuration.Object);
            _cryptography
                .Setup(s => s.EncryptModel(It.IsAny<ParticipantModel>(), It.IsAny<string>()).Result)
                .Returns(new EncryptedParticipantModel());

            storageMock.Setup(s => s.Projects).Returns(new[]
            {
                new Project
                {
                    DataDefinition = new DataDefinition
                    {
                        DataFields = new List<DataField>
                        {
                            new DataField
                            {
                                Id = "field1",
                                Name = "Field1",
                                DataType = "type1"
                            },
                            new DataField
                            {
                                Id = "field2",
                                DataType = "type2"
                            }
                        },
                        DataTypes = new List<DataType>
                        {
                            new DataType
                            {
                                Id = "type1",
                                Values = "val1,val2"
                            },
                            new DataType
                            {
                                Id = "type2",
                                Values = "val3,val4"
                            }
                        }
                    },
                    DataDisplay = new DataDisplay
                    {
                        DataFields = new List<DataFieldDisplay>
                        {
                            new DataFieldDisplay
                            {
                                Id = "field1",
                                I18NKey = "Application.Test.Key"
                            }
                        },
                        DataTypes = new List<DataTypeDisplay>
                        {
                            new DataTypeDisplay
                            {
                                Id = "type1",
                                DisplayValues = new Dictionary<string, string[]>
                                {
                                    {"val1", new[] {"Application.Test.Val1.Key"}}
                                }
                            }
                        }
                    }
                }
            });
            return new ParticipantTransformationService(_cryptography.Object, helper, storageMock.Object);
        }
        [Fact]
        public void ShouldCreateHeaderDto()
        {
            var transformationService = CreateInstance();

            #region ObjectInitialisation

            var model = new EncryptedParticipantModel
            {
                Id = "model-id",
                OnlineId = "online-id",
                CreatedAt = DateTime.Now,
                Synchronized = false,
                SynchronizedAt = DateTime.Today,
                Additional = new Dictionary<string, string>
                {
                    {"projectId", "project-id"},
                    {"documentedBy", "documented-id"}
                }
            };

            var header = new Dictionary<string, List<string>>
            {
                {"key", new List<string> {"value"}}
            };

            #endregion

            var dto = transformationService.CreateHeaderDto(model, header);
            
            Assert.Equal(model.Id, dto.Id);
            Assert.Equal(model.OnlineId, dto.OnlineId);
            Assert.Equal(model.CreatedAt, dto.CreatedAt);
            Assert.Equal(model.Synchronized, dto.Synchronized);
            Assert.Equal(model.SynchronizedAt, dto.SynchronizedAt);
            Assert.Equal("project-id", dto.ProjectId);
            Assert.Equal("documented-id", dto.DocumentedBy);
            Assert.Equal(header, dto.Header);
        }
        
        [Fact]
        public void ShouldGetDocuments()
        {

            var transformationService = CreateInstance();

            #region ObjectInitialisation

            var model = new ParticipantModel
            {
                Id = "model-id",
                OnlineId = "online-id",
                Data = new DataBody
                {
                    DefaultDocument = "default-document",
                    Documents =
                    {
                        {"document-1", new Document
                        {
                            Id = "document-1",
                            DocumentId = "some-id",
                            Name = "document-update",
                            Ordinal = 0
                        }},
                        {"document-2", new Document
                        {
                            Id = "document-2",
                            DocumentId = "some-id",
                            Name = "document-delete",
                            Ordinal = 1
                        }},
                        {"document-3", new Document
                        {
                            Id = "document-3",
                            DocumentId = "some-id",
                            Name = "document-unchanged",
                            Ordinal = 3
                        }},
                    }
                }
            };

            var delta = new DeltaFile
            {
                Documents =
                {
                    {"document-1", new DocumentDelta
                    {
                        Id = "document-1",
                        Type = DeltaType.Update,
                        Value = "updated-name",
                        Ordinal = 5,
                    }},
                    {"document-2", new DocumentDelta
                    {
                        Id = "document-2",
                        Type = DeltaType.Delete,
                        Value = "updated-name",
                        Ordinal = 5,
                    }},
                    {"document-3", new DocumentDelta
                    {
                        Id = "document-3",
                        Type = DeltaType.Unchanged,
                        Value = "updated-name",
                        Ordinal = 5,
                    }},
                    {"document-4", new DocumentDelta
                    {
                        Id = "document-4",
                        Type = DeltaType.Create,
                        Value = "created-document",
                        Ordinal = 4,
                    }},
                },
                DocumentTree =
                {
                    {"document-1", new TreeItem
                    {
                        Id = "document-1",
                        Children =
                        {
                            {"document-2", new TreeItem
                            {
                                Id = "document-2"
                            }},
                            {"document-3", new TreeItem
                            {
                                Id = "document-3"
                            }}
                        }
                    }},
                    {"document-4", new TreeItem
                    {
                        Id = "document-4"
                    }}
                }
            };

            #endregion

            var dto = transformationService.GetDocuments(model, delta);
            
            Assert.Equal(model.Id, dto.ParticipantId);
            Assert.Equal(model.OnlineId, dto.OnlineId);
            Assert.Equal(model.Data.DefaultDocument, dto.DefaultDocument);
            Assert.Equal(2, dto.Documents.Length);
            var secondDocument = dto.Documents[0];
            Assert.Equal("document-4", secondDocument.Id);
            Assert.Null(secondDocument.DocumentId);
            Assert.True(secondDocument.IsDelta);
            Assert.Equal("created-document", secondDocument.Name);
            Assert.Empty(secondDocument.Documents);
            var firstDocument = dto.Documents[1];
            Assert.Equal("document-1", firstDocument.Id);
            Assert.Equal("some-id", firstDocument.DocumentId);
            Assert.Equal("updated-name", firstDocument.Name);
            Assert.True(firstDocument.IsDelta);
            Assert.Single(firstDocument.Documents);
            var childDocument = firstDocument.Documents[0];
            Assert.Equal("document-3", childDocument.Id);
            Assert.Equal("some-id", childDocument.DocumentId);
            Assert.Equal("document-unchanged", childDocument.Name);
            Assert.False(childDocument.IsDelta);
            Assert.Empty(childDocument.Documents);
        }

        [Fact]
        public void ShouldGetDocument()
        {
            var transformationService = CreateInstance();

            #region ObjectInitialisation

            var model = new ParticipantModel
            {
                Id = "model-id",
                OnlineId = "online-id",
                Data = new DataBody
                {
                    DefaultDocument = "default-document",
                    Documents =
                    {
                        {"document-1", new Document
                        {
                            Id = "document-1",
                            DocumentId = "some-id",
                            Name = "document-update",
                            Ordinal = 0
                        }},
                        {"document-2", new Document
                        {
                            Id = "document-2",
                            DocumentId = "some-id",
                            Name = "document-delete",
                            Ordinal = 1
                        }},
                        {"document-3", new Document
                        {
                            Id = "document-3",
                            DocumentId = "some-id",
                            Name = "document-unchanged",
                            Ordinal = 3
                        }},
                    }
                }
            };

            var delta = new DeltaFile
            {
                Documents =
                {
                    {"document-1", new DocumentDelta
                    {
                        Id = "document-1",
                        Type = DeltaType.Update,
                        Value = "updated-name",
                        Ordinal = 5,
                    }},
                    {"document-2", new DocumentDelta
                    {
                        Id = "document-2",
                        Type = DeltaType.Delete,
                        Value = "updated-name",
                        Ordinal = 5,
                    }},
                    {"document-3", new DocumentDelta
                    {
                        Id = "document-3",
                        Type = DeltaType.Unchanged,
                        Value = "updated-name",
                        Ordinal = 5,
                    }},
                    {"document-4", new DocumentDelta
                    {
                        Id = "document-4",
                        Type = DeltaType.Create,
                        Value = "created-document",
                        Ordinal = 4,
                    }},
                },
                DocumentTree =
                {
                    {"document-1", new TreeItem
                    {
                        Id = "document-1",
                        Children =
                        {
                            {"document-2", new TreeItem
                            {
                                Id = "document-2"
                            }},
                            {"document-3", new TreeItem
                            {
                                Id = "document-3"
                            }}
                        }
                    }},
                    {"document-4", new TreeItem
                    {
                        Id = "document-4"
                    }}
                }
            };

            #endregion

            var dto = transformationService.GetDocument(model, delta, "document-1");


            Assert.Equal("document-1", dto.Id);
            Assert.Equal("some-id", dto.DocumentId);
            Assert.Equal("updated-name", dto.Name);
            Assert.True(dto.IsDelta);
            Assert.Empty(dto.Documents);
        }
        
        [Fact]
        public void ShouldCreateValueDto()
        {
            var transformationService = CreateInstance();

            #region ObjectInitialisation

            var model = new ParticipantModel
            {
                Id = "model-id",
                OnlineId = "online-id",
                Data = new DataBody
                {
                    DefaultDocument = "default-document",
                    Documents =
                    {
                        {"document-1", new Document
                        {
                            Id = "document-1",
                            DocumentId = "some-id",
                            Name = "document-update",
                            Ordinal = 0,
                            Groups =
                            {
                                {"unchanged-group", new DocumentGroup
                                {
                                    Id = "unchanged-group",
                                    Ordinal = 0,
                                    GroupId = "group-id",
                                }},
                                {"deleted-group", new DocumentGroup
                                {
                                    Id = "deleted-group",
                                    Ordinal = 1,
                                    GroupId = "group-id",
                                }},
                                {"updated-group", new DocumentGroup
                                {
                                    Id = "updated-group",
                                    Ordinal = 2,
                                    GroupId = "group-id",
                                }},
                            },
                            Values =
                            {
                                {"unchanged-field", new DocumentField
                                {
                                    Id = "unchanged-field",
                                    Value = "value"
                                }},
                                {"deleted-field", new DocumentField
                                {
                                    Id = "deletedField",
                                    Value = "value"
                                }},
                                {"updated-field", new DocumentField
                                {
                                    Id = "updated-field",
                                    Value = "value"
                                }},
                            }
                        }},
                    }
                }
            };

            var delta = new DeltaFile
            {
                Documents =
                {
                    {"document-1", new DocumentDelta
                    {
                        Id = "document-1",
                        Type = DeltaType.Update,
                        Value = "updated-name",
                        Ordinal = 5,
                        Groups =
                        {
                            {"deleted-group", new GroupDelta
                            {
                                Id = "deleted-group",
                                Type = DeltaType.Delete
                            }},
                            {"updated-group", new GroupDelta
                            {
                                Id = "updated-group",
                                Type = DeltaType.Update,
                                Ordinal = 99,
                                GroupId = "group-id",
                            }},
                            {"created-group", new GroupDelta
                            {
                                Id = "created-group",
                                Type = DeltaType.Create,
                                Ordinal = 10,
                            }}
                        },
                        Values = {
                            {"deletedField", new ValueDelta
                            {
                                Id = "deletedField",
                                Type = DeltaType.Delete
                            }},
                            {"updated-field", new ValueDelta
                            {
                                Id = "updated-field",
                                Type = DeltaType.Update,
                                Value = "newValue"
                            }},
                            {"created-field", new ValueDelta
                            {
                                Id = "created-field",
                                Type = DeltaType.Create,
                                Value = "deltaValue",
                                UseDisplay = 0
                            }}
                        },
                    }},
                },
                DocumentTree =
                {
                    {"document-1", new TreeItem
                    {
                        Id = "document-1",
                    }},
                }
            };

            var expectedDto = new DocumentValueDto
            {
                Id = "document-1",
                DocumentId = "some-id",
                Name = "updated-name",
                IsDelta = true,
                Fields = new[]
                {
                    new FieldDto
                    {
                        Id = "updated-field",
                        Value = "newValue",
                        IsDelta = true
                    },
                    new FieldDto
                    {
                        Id = "created-field",
                        Value = "deltaValue",
                        IsDelta = true,
                        UseDisplay = 0
                    },
                    new FieldDto
                    {
                        Id = "unchanged-field",
                        IsDelta = false,
                        Value = "value"
                    },
                    
                },
                Groups = new[]
                {
                    new GroupDto
                    {
                        Id = "unchanged-group",
                        GroupId = "group-id",
                        IsDelta = false,
                    },
                    new GroupDto
                    {
                        Id = "created-group",
                        IsDelta = true,
                    },
                    new GroupDto
                    {
                        Id = "updated-group",
                        GroupId = "group-id",
                        IsDelta = true,
                    },
                }
            };

            #endregion

            var dto = transformationService.CreateValueDto(model, delta, "document-1");

            var dtoString = JsonConvert.SerializeObject(dto);
            var expectedString = JsonConvert.SerializeObject(expectedDto);

            Assert.Equal(expectedString, dtoString);
        }

        [Fact]
        public void ShouldCreateEncryptedModel()
        {
            var transformationService = CreateInstance();

            #region ObjectInitialization

            var dto = new CreateParticipantDto
            {
                Icon = "add",
                Name = "John Doe",
                Tint = "#f000",
                DocumentedBy = "project",
                ProjectId = "project"
            };

            #endregion

            transformationService.CreateEncryptedModel(dto, "userId");
            
            _cryptography.Verify(c => c.EncryptModel(It.IsAny<ParticipantModel>(), It.IsAny<string>()), Times.Exactly(1));
            
        }
        
        [Fact]
        public void ShouldCreateCorrectHeaderDto()
        {
            var service = CreateInstance();
            IDictionary<string, List<string>>[] header = {
                new Dictionary<string, List<string>>
                {
                    {"field1", new List<string> {"val1", "val2"}},
                    {"field2", new List<string>{"val3"}}
                },
                new Dictionary<string, List<string>>
                {
                    {"field2", new List<string>{"val3"}}
                }
            };
            var dto = service.CreateHeaderEntryDtos(header);
            
            Assert.Equal(2, dto.Length);
            Assert.Equal("field1", dto[0].FieldId);
            Assert.Equal("Field1", dto[0].Name);
            Assert.Equal("Application.Test.Key", dto[0].I18NKey);
            Assert.Equal(2, dto[0].Values.Length);
            Assert.Equal("Application.Test.Val1.Key", dto[0].Values[0].I18NKey);
            Assert.Null(dto[0].Values[1].I18NKey);
            Assert.Single(dto[1].Values);

        }

        [Fact]
        public void ShouldUpdateHeader()
        {
            var service = CreateInstance();
            IDictionary<string, List<string>> header = new Dictionary<string, List<string>>
            {
                {"name", new List<string> {"val1", "val2"}},
                {"tint", new List<string> {"val3"}},
                {"icon", new List<string> {"val4"}}
            };
            var dto = new HeaderEditDto
            {
                Name = "newVal1",
                Icon = "newVal2",
                Tint = "newVal3"
            };
            var newHeader = service.UpdateParticipantHeader(header, dto);

            Assert.Single(newHeader["name"]);
            Assert.Single(newHeader["tint"]);
            Assert.Single(newHeader["icon"]);
            Assert.Equal("newVal1", newHeader["name"][0]);
            Assert.Equal("newVal2", newHeader["icon"][0]);
            Assert.Equal("newVal3", newHeader["tint"][0]);
        }

        [Fact]
        public void ShouldAddNewFields()
        {
            var service = CreateInstance();

            var fields1 = new[]
            {
                new FieldDto
                {
                    Id = "field1",
                    Value = "value1"
                }
            };
            var fields2 = new[]
            {
                new FieldDto
                {
                    Id = "field2",
                    Value = "value2"
                }
            };
            var result = service.CopyFields(fields1, fields2, false);
            // New field has been added
            Assert.Equal(2, result.Length);
            
            Assert.Same(fields1[0], result[0]);
            Assert.Same(fields2[0], result[1]);
        }
        
        [Fact]
        public void ShouldReplaceField()
        {
            var service = CreateInstance();

            var fields1 = new[]
            {
                new FieldDto
                {
                    Id = "field1",
                    Value = "value1"
                }
            };
            var fields2 = new[]
            {
                new FieldDto
                {
                    Id = "field1",
                    Value = "value2",
                    UseDisplay = 1
                }
            };
            var result = service.CopyFields(fields1, fields2, false);
            // New field has been added
            Assert.Single(result);

            var result1 = result[0];
            Assert.Equal("value2", result1.Value);
            Assert.Equal(1, result1.UseDisplay);
        }
        
        [Fact]
        public void ShouldKeepOldFieldValues()
        {
            var service = CreateInstance();

            var fields1 = new[]
            {
                new FieldDto
                {
                    Id = "field1",
                    Value = "value1"
                }
            };
            var fields2 = new[]
            {
                new FieldDto
                {
                    Id = "field1",
                    Value = "value2",
                    UseDisplay = 1
                }
            };
            var result = service.CopyFields(fields1, fields2, true);
            // New field has been added
            Assert.Single(result);

            var result1 = result[0];
            Assert.Equal("value1", result1.Value);
            Assert.Null(result1.UseDisplay);
        }
        
        [Fact]
        public void ShouldAddNewGroups()
        {
            var service = CreateInstance();

            var group1 = new[]
            {
                new GroupDto
                {
                    Id = "group1",
                }
            };
            var group2 = new[]
            {
                new GroupDto
                {
                    Id = "group2",
                }
            };
            var result = service.CopyGroups(group1, group2, true);
         
            Assert.Equal(2, result.Length);
            
            Assert.Same(group1[0], result[0]);
            Assert.Same(group2[0], result[1]);
        }
        
        [Fact]
        public void ShouldReplaceGroup()
        {
            var service = CreateInstance();

            var group1 = new[]
            {
                new GroupDto
                {
                    Id = "group1",
                }
            };
            var group2 = new[]
            {
                new GroupDto
                {
                    Id = "group2",
                }
            };
            var result = service.CopyGroups(group1, group2, false);
            
            Assert.Single(result);
            var resultGroup = result[0];
            Assert.Equal(group2[0].Id, resultGroup.Id);
            Assert.Equal(group2[0].GroupId, resultGroup.GroupId);
            Assert.Equal(group2[0].Fields, resultGroup.Fields);
            Assert.Equal(group2[0].Children, resultGroup.Children);
        }

        [Fact]
        public async Task ShouldReplaceDocumentValues()
        {
            var service = CreateInstance();

            var document1 = new DocumentValueDto
            {
                Id = "document1",
                Name = "document"
            };
            var document2 = new DocumentValueDto
            {
                Id = "document2",
                Name = "document2"
            };
            var result = await service.CopyValues(document1, document2, false);
            // The name should not be replaced.
            Assert.Equal("document", result.Name);
            Assert.Equal("document1", result.Id);
        }
        
        [Fact]
        public async Task ShouldReturnDocumentOnIdMismatch()
        {
            var service = CreateInstance();

            var document1 = new DocumentValueDto
            {
                Id = "document1",
                DocumentId = "doc",
                Name = "document"
            };
            var document2 = new DocumentValueDto
            {
                Id = "document2",
                DocumentId = "doc2",
                Name = "document2"
            };
            var result = await service.CopyValues(document1, document2, false);
            
            Assert.Same(document1, result);
        }
    }
}