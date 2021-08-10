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
using LotsenApp.Client.Authentication.DataPassword;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.Participant.Delta;
using LotsenApp.Client.Participant.Dto;
using LotsenApp.Client.Participant.Model;
using Moq;
using Xunit;

namespace LotsenApp.Client.Participant.Test.Delta
{
    [ExcludeFromCodeCoverage]
    public class DeltaServiceTest
    {
        #region UpdateDocument Tests

        [Fact]
        public void ShouldUpdateDocumentName()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new DeltaFile
            {
                Documents =
                {
                    {
                        "id", new DocumentDelta
                        {
                            Id = "id",
                            DocumentId = "documentId",
                            Type = DeltaType.Create,
                            Value = "value",
                            OldValue = "oldValue",
                            TimeStamp = DateTime.UtcNow,
                        }
                    }
                },
                DocumentTree =
                {
                    {
                        "id", new TreeItem
                        {
                            Id = "id"
                        }
                    }
                }
            };
            var model = new ParticipantModel
            {
                Id = "modelId",
                Data = new DataBody
                {
                }
            };

            var dto = new UpdateDocumentDto
            {
                Id = "id",
                Name = "newValue"
            };

            #endregion

            service.UpdateDocument(delta, model, dto);

            var documentDelta = delta.Documents["id"];

            Assert.Equal("newValue", documentDelta.Value);
            Assert.Equal("value", documentDelta.OldValue);
        }
        
        [Fact]
        public void ShouldNotUpdateDeletedDocument()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new DeltaFile
            {
                Documents =
                {
                    {
                        "id", new DocumentDelta
                        {
                            Id = "id",
                            DocumentId = "documentId",
                            Type = DeltaType.Delete,
                            Value = "value",
                            OldValue = "oldValue",
                            TimeStamp = DateTime.UtcNow,
                        }
                    }
                },
                DocumentTree =
                {
                    {
                        "id", new TreeItem
                        {
                            Id = "id"
                        }
                    }
                }
            };
            var model = new ParticipantModel
            {
                Id = "modelId",
                Data = new DataBody
                {
                }
            };

            var dto = new UpdateDocumentDto
            {
                Id = "id",
                Name = "newValue"
            };

            #endregion

            service.UpdateDocument(delta, model, dto);

            var documentDelta = delta.Documents["id"];

            Assert.Equal("value", documentDelta.Value);
            Assert.Equal("oldValue", documentDelta.OldValue);
        }

        [Fact]
        public void ShouldUpdateGroup()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new Dictionary<string, GroupDelta>
            {
                {
                    "groupId", new GroupDelta
                    {
                        Id = "groupId",
                        Fields =
                        {
                            {
                                "fieldId", new ValueDelta
                                {
                                    Id = "fieldId",
                                    Value = "value",
                                    UseDisplay = 1,
                                }
                            }
                        }
                    }
                }
            };
            var model = new Dictionary<string, DocumentGroup>();

            var dto = new[]
            {
                new UpdateGroupDto
                {
                    Id = "groupId",
                    Fields = new[]
                    {
                        new UpdateFieldDto
                        {
                            Id = "fieldId",
                            UseDisplay = 2,
                            Value = "newValue"
                        }
                    }
                }
            };

            #endregion

            service.UpdateGroups(delta, model, dto);

            var groupDelta = delta["groupId"];

            Assert.Equal("newValue", groupDelta.Fields["fieldId"].Value);
            Assert.Equal(2, groupDelta.Fields["fieldId"].UseDisplay);
            Assert.Equal(0, groupDelta.Ordinal);
        }

        [Fact]
        public void ShouldUpdateGroupWithoutDelta()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new Dictionary<string, GroupDelta>();
            var model = new Dictionary<string, DocumentGroup>();

            var dto = new[]
            {
                new UpdateGroupDto
                {
                    Id = "groupId",
                    Fields = new[]
                    {
                        new UpdateFieldDto
                        {
                            Id = "fieldId",
                            UseDisplay = 2,
                            Value = "newValue"
                        }
                    }
                }
            };

            #endregion

            service.UpdateGroups(delta, model, dto);

            var groupDelta = delta["groupId"];

            Assert.Equal("newValue", groupDelta.Fields["fieldId"].Value);
            Assert.Equal(2, groupDelta.Fields["fieldId"].UseDisplay);
            Assert.Equal(0, groupDelta.Ordinal);
        }

        [Fact]
        public void ShouldUpdateField()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new Dictionary<string, ValueDelta>
            {
                {
                    "fieldId", new ValueDelta
                    {
                        Id = "fieldId",
                        Value = "value",
                        UseDisplay = 1,
                        Type = DeltaType.Create
                    }
                }
            };
            var model = new Dictionary<string, DocumentField>();

            var dto = new[]
            {
                new UpdateFieldDto
                {
                    Id = "fieldId",
                    UseDisplay = 2,
                    Value = "newValue"
                }
            };

            #endregion

            service.UpdateFields(delta, model, dto);

            var fieldDelta = delta["fieldId"];

            Assert.Equal("newValue", fieldDelta.Value);
            Assert.Equal(2, fieldDelta.UseDisplay);
        }

        [Fact]
        public void ShouldUpdateFieldWithoutValue()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new Dictionary<string, ValueDelta>();
            var model = new Dictionary<string, DocumentField>();

            var dto = new[]
            {
                new UpdateFieldDto
                {
                    Id = "fieldId",
                    UseDisplay = 2,
                    Value = "newValue"
                }
            };

            #endregion

            var resultDelta = service.UpdateFields(delta, model, dto);

            var fieldDelta = resultDelta["fieldId"];

            Assert.Equal("newValue", fieldDelta.Value);
            Assert.Equal(2, fieldDelta.UseDisplay);
            Assert.Equal(DeltaType.Create, fieldDelta.Type);
        }

        [Fact]
        public void ShouldNotUpdateFieldWithSameValue()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new Dictionary<string, ValueDelta>();
            var model = new Dictionary<string, DocumentField>
            {
                {
                    "fieldId", new DocumentField
                    {
                        Id = "fieldId",
                        Value = "newValue",
                        UseDisplay = 2
                    }
                }
            };

            var dto = new[]
            {
                new UpdateFieldDto
                {
                    Id = "fieldId",
                    UseDisplay = 2,
                    Value = "newValue"
                }
            };

            #endregion

            var resultDelta = service.UpdateFields(delta, model, dto);
            // No delta should have been added, because both values are not new.
            Assert.Empty(resultDelta);
        }

        #endregion

        #region ReorderGroups Tests

        [Fact]
        public void ShouldReorderTopLevelGroups()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var deltas = new Dictionary<string, GroupDelta>
            {
                {
                    "group1", new GroupDelta
                    {
                        Id = "group1",
                        Ordinal = 0
                    }
                },
                {
                    "group2", new GroupDelta
                    {
                        Id = "group2",
                        Ordinal = 1
                    }
                }
            };

            var dto = new[]
            {
                new ReOrderDto
                {
                    Id = "group2"
                },
                new ReOrderDto
                {
                    Id = "group1"
                },
            };

            #endregion

            var updatedDeltas = service.ReArrangeGroups(deltas, dto);

            Assert.Equal(1, updatedDeltas["group1"].Ordinal);
            Assert.Equal(0, updatedDeltas["group2"].Ordinal);
        }

        [Fact]
        public void ShouldReorderGroupsDownTheLine()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var deltas = new Dictionary<string, GroupDelta>
            {
                {
                    "group1", new GroupDelta
                    {
                        Id = "group1",
                        Ordinal = 0,
                        Children =
                        {
                            {
                                "child1", new GroupDelta
                                {
                                    Id = "child1",
                                    Ordinal = 0
                                }
                            },
                            {
                                "child2", new GroupDelta
                                {
                                    Id = "child2",
                                    Ordinal = 1
                                }
                            }
                        }
                    }
                }
            };

            var dto = new[]
            {
                new ReOrderDto
                {
                    Id = "group1",
                    Documents = new[]
                    {
                        new ReOrderDto
                        {
                            Id = "child2",
                        },
                        new ReOrderDto
                        {
                            Id = "child1"
                        }
                    }
                },
            };

            #endregion

            var updatedDeltas = service.ReArrangeGroups(deltas, dto);

            Assert.Equal(0, updatedDeltas["group1"].Ordinal);
            Assert.Equal(1, updatedDeltas["group1"].Children["child1"].Ordinal);
            Assert.Equal(0, updatedDeltas["group1"].Children["child2"].Ordinal);
        }
        
        [Fact]
        public void ShouldReorderGroupsWithoutDeltas()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var deltas = new Dictionary<string, GroupDelta>
            {
            };

            var dto = new[]
            {
                new ReOrderDto
                {
                    Id = "group2"
                },
                new ReOrderDto
                {
                    Id = "group1"
                },
            };

            #endregion

            var updatedDeltas = service.ReArrangeGroups(deltas, dto);

            Assert.Equal(1, updatedDeltas["group1"].Ordinal);
            Assert.Equal(0, updatedDeltas["group2"].Ordinal);
        }

        #endregion

        #region AddGroup Tests

        [Fact]
        public void ShouldAddNewGroup()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new DeltaFile
            {
                Documents =
                {
                    {
                        "document", new DocumentDelta
                        {
                            Id = "document",
                        }
                    }
                },
                DocumentTree =
                {
                    {
                        "document", new TreeItem
                        {
                            Id = "document"
                        }
                    }
                }
            };

            var dto = new CreateGroupDto
            {
                DocumentId = "document",
                GroupId = "group"
            };

            #endregion

            var newId = service.AddGroup(delta, dto);

            // The group has been added
            Assert.NotEmpty(delta.Documents["document"].Groups.Values);
            // The id that is returned is also the id of the group
            Assert.Equal(newId, delta.Documents["document"].Groups[newId].Id);
            // The provided group id has been set
            Assert.Equal("group", delta.Documents["document"].Groups[newId].GroupId);
            // The delta type is Create
            Assert.Equal(DeltaType.Create, delta.Documents["document"].Groups[newId].Type);

            // The group has been added to the tree
            Assert.NotEmpty(delta.DocumentTree["document"].Children);
            // The id that is returned is also the id of the group in the tree
            Assert.Equal(newId, delta.DocumentTree["document"].Children[newId].Id);
        }

        [Fact]
        public void ShouldAddNewChildGroup()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new DeltaFile
            {
                Documents =
                {
                    {
                        "document", new DocumentDelta
                        {
                            Id = "document",
                            Groups =
                            {
                                {
                                    "parent", new GroupDelta
                                    {
                                        Id = "parent",
                                    }
                                }
                            }
                        }
                    }
                },
                DocumentTree =
                {
                    {
                        "document", new TreeItem
                        {
                            Id = "document",
                            Children =
                            {
                                {
                                    "parent", new TreeItem
                                    {
                                        Id = "parent"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var dto = new CreateGroupDto
            {
                DocumentId = "document",
                ParentGroupId = "parent",
                GroupId = "child"
            };

            #endregion

            var newId = service.AddGroup(delta, dto);

            // The group has been added
            Assert.NotEmpty(delta.Documents["document"].Groups["parent"].Children.Values);
            // The id that is returned is also the id of the group
            Assert.Equal(newId, delta.Documents["document"].Groups["parent"].Children[newId].Id);
            // The provided group id has been set
            Assert.Equal("child", delta.Documents["document"].Groups["parent"].Children[newId].GroupId);
            // The delta type is Create
            Assert.Equal(DeltaType.Create, delta.Documents["document"].Groups["parent"].Children[newId].Type);

            // The group has been added to the tree
            Assert.NotEmpty(delta.DocumentTree["document"].Children["parent"].Children);
            // The id that is returned is also the id of the group in the tree
            Assert.Equal(newId, delta.DocumentTree["document"].Children["parent"].Children[newId].Id);
        }

        #endregion

        #region RemoveGroup Tests

        [Fact]
        public void ShouldRemoveGroup()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new DeltaFile
            {
                Documents =
                {
                    {
                        "document", new DocumentDelta
                        {
                            Id = "document",
                            Groups =
                            {
                                {
                                    "group", new GroupDelta
                                    {
                                        Id = "group",
                                        Type = DeltaType.Create,
                                    }
                                }
                            }
                        }
                    }
                },
                DocumentTree =
                {
                    {
                        "document", new TreeItem
                        {
                            Id = "document",
                            Children =
                            {
                                {
                                    "group", new TreeItem
                                    {
                                        Id = "group"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            #endregion

            service.RemoveGroup(delta, "document", "group");

            // The group still exists
            Assert.NotEmpty(delta.Documents["document"].Groups.Values);
            // The id that is returned is also the id of the group
            Assert.Equal("group", delta.Documents["document"].Groups["group"].Id);
            // The delta type is Delete
            Assert.Equal(DeltaType.Delete, delta.Documents["document"].Groups["group"].Type);

            // The group has been removed from the tree
            Assert.Empty(delta.DocumentTree["document"].Children);
        }

        [Fact]
        public void ShouldRemoveChildGroup()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new DeltaFile
            {
                Documents =
                {
                    {
                        "document", new DocumentDelta
                        {
                            Id = "document",
                            Groups =
                            {
                                {
                                    "parent", new GroupDelta
                                    {
                                        Id = "parent",
                                        Type = DeltaType.Create,
                                        Children =
                                        {
                                            {
                                                "child", new GroupDelta
                                                {
                                                    Id = "child"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                DocumentTree =
                {
                    {
                        "document", new TreeItem
                        {
                            Id = "document",
                            Children =
                            {
                                {
                                    "parent", new TreeItem
                                    {
                                        Id = "parent",
                                        Children =
                                        {
                                            {
                                                "child", new TreeItem
                                                {
                                                    Id = "child"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            #endregion

            service.RemoveGroup(delta, "document", "child");

            // The group still exists
            Assert.NotEmpty(delta.Documents["document"].Groups["parent"].Children.Values);
            // The id that is returned is also the id of the group
            Assert.Equal("child", delta.Documents["document"].Groups["parent"].Children["child"].Id);
            // The delta type is Delete
            Assert.Equal(DeltaType.Delete, delta.Documents["document"].Groups["parent"].Children["child"].Type);

            // The group has been removed from the tree
            Assert.Empty(delta.DocumentTree["document"].Children["parent"].Children);
        }

        #endregion

        #region ReorderDocuments Tests

        [Fact]
        public void ShouldReorderTopLevelDocuments()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new DeltaFile
            {
                Documents =
                {
                    {
                        "document1", new DocumentDelta
                        {
                            Id = "document1",
                            Ordinal = 0
                        }
                    },
                    {
                        "document2", new DocumentDelta
                        {
                            Id = "document2",
                            Ordinal = 1
                        }
                    }
                },
                DocumentTree =
                {
                    {
                        "document1", new TreeItem
                        {
                            Id = "document1"
                        }
                    },
                    {
                        "document2",
                        new TreeItem
                        {
                            Id = "document2"
                        }
                    }
                }
            };

            var dto = new[]
            {
                new ReOrderDto
                {
                    Id = "document2"
                },
                new ReOrderDto
                {
                    Id = "document1"
                },
            };

            #endregion

            var updatedDelta = service.ReorderDocuments(delta, dto);

            // Deltas have been updated
            Assert.Equal(1, updatedDelta.Documents["document1"].Ordinal);
            Assert.Equal(0, updatedDelta.Documents["document2"].Ordinal);
            // Tree is unchanged
            Assert.Contains(updatedDelta.DocumentTree, i => i.Key == "document1");
            Assert.Contains(updatedDelta.DocumentTree, i => i.Key == "document2");
        }

        [Fact]
        public void ShouldReorderDocumentsDownTheLine()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new DeltaFile
            {
                Documents =
                {
                    {
                        "document1", new DocumentDelta
                        {
                            Id = "document1",
                            Ordinal = 0
                        }
                    },
                    {
                        "document2", new DocumentDelta
                        {
                            Id = "document2",
                            Ordinal = 1
                        }
                    },
                    {
                        "document3", new DocumentDelta
                        {
                            Id = "document3",
                            Ordinal = 0
                        }
                    }
                },
                DocumentTree =
                {
                    {
                        "document3",
                        new TreeItem
                        {
                            Id = "document3",
                            Children =
                            {
                                {
                                    "document1", new TreeItem
                                    {
                                        Id = "document1"
                                    }
                                },
                                {
                                    "document2",
                                    new TreeItem
                                    {
                                        Id = "document2"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var dto = new[]
            {
                new ReOrderDto
                {
                    Id = "document3",
                    Documents = new[]
                    {
                        new ReOrderDto
                        {
                            Id = "document2"
                        },
                        new ReOrderDto
                        {
                            Id = "document1"
                        },
                    }
                }
            };

            #endregion

            var updatedDelta = service.ReorderDocuments(delta, dto);

            // Deltas have been updated
            Assert.Equal(1, updatedDelta.Documents["document1"].Ordinal);
            Assert.Equal(0, updatedDelta.Documents["document2"].Ordinal);
            // Tree is unchanged
            Assert.Contains(updatedDelta.DocumentTree["document3"].Children, i => i.Key == "document1");
            Assert.Contains(updatedDelta.DocumentTree["document3"].Children, i => i.Key == "document2");
        }

        [Fact]
        public void ShouldMoveDocumentToCorrectLevel()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new DeltaFile
            {
                Documents =
                {
                    {
                        "document1", new DocumentDelta
                        {
                            Id = "document1",
                            Ordinal = 0
                        }
                    },
                    {
                        "document2", new DocumentDelta
                        {
                            Id = "document2",
                            Ordinal = 1
                        }
                    },
                    {
                        "document3", new DocumentDelta
                        {
                            Id = "document3",
                            Ordinal = 0
                        }
                    }
                },
                DocumentTree =
                {
                    {
                        "document3",
                        new TreeItem
                        {
                            Id = "document3",
                            Children =
                            {
                                {
                                    "document1", new TreeItem
                                    {
                                        Id = "document1"
                                    }
                                },
                                {
                                    "document2",
                                    new TreeItem
                                    {
                                        Id = "document2"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var dto = new[]
            {
                new ReOrderDto
                {
                    Id = "document2"
                },
                new ReOrderDto
                {
                    Id = "document3",
                    Documents = new[]
                    {
                        new ReOrderDto
                        {
                            Id = "document1"
                        },
                    }
                }
            };

            #endregion

            var updatedDelta = service.ReorderDocuments(delta, dto);

            // Deltas have been updated
            Assert.Equal(0, updatedDelta.Documents["document1"].Ordinal);
            Assert.Equal(0, updatedDelta.Documents["document2"].Ordinal);
            Assert.Equal(1, updatedDelta.Documents["document3"].Ordinal);
            // Tree is changed. Document2 is on the top level
            Assert.Contains(updatedDelta.DocumentTree["document3"].Children,
                i => i.Key == "document1");
            Assert.Contains(updatedDelta.DocumentTree,
                i => i.Key == "document2");
            Assert.DoesNotContain(updatedDelta.DocumentTree["document3"].Children,
                i => i.Key == "document2");
        }
        
        [Fact]
        public void ShouldReorderDocumentsWithoutDeltas()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new DeltaFile
            {
                DocumentTree =
                {
                    {
                        "document1", new TreeItem
                        {
                            Id = "document1"
                        }
                    },
                    {
                        "document2",
                        new TreeItem
                        {
                            Id = "document2"
                        }
                    }
                }
            };

            var dto = new[]
            {
                new ReOrderDto
                {
                    Id = "document2"
                },
                new ReOrderDto
                {
                    Id = "document1"
                },
            };

            #endregion

            var updatedDelta = service.ReorderDocuments(delta, dto);

            // Deltas have been updated
            Assert.Equal(1, updatedDelta.Documents["document1"].Ordinal);
            Assert.Equal(0, updatedDelta.Documents["document2"].Ordinal);
            // Tree is unchanged
            Assert.Contains(updatedDelta.DocumentTree, i => i.Key == "document1");
            Assert.Contains(updatedDelta.DocumentTree, i => i.Key == "document2");
        }

        #endregion

        #region AddDocument Tests

        [Fact]
        public void ShouldAddTopLevelDocument()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new DeltaFile
            {
            };

            var dto = new CreateDocumentDto
            {
                Name = "Document",
                DocumentId = "document",
            };

            #endregion

            var newId = service.AddDocument(delta, dto);


            // The document has been added
            Assert.NotEmpty(delta.Documents);
            // The id that is returned is also the id of the document
            Assert.Equal(newId, delta.Documents[newId].Id);
            // The name that is returned is also the name of the document
            Assert.Equal("Document", delta.Documents[newId].Value);
            // The provided document id has been set
            Assert.Equal("document", delta.Documents[newId].DocumentId);
            // The delta type is Create
            Assert.Equal(DeltaType.Create, delta.Documents[newId].Type);

            // The document has been added to the tree
            Assert.NotEmpty(delta.DocumentTree);
            // The id that is returned is also the id of the document in the tree
            Assert.Equal(newId, delta.DocumentTree[newId].Id);
        }

        [Fact]
        public void ShouldAddChildDocument()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new DeltaFile
            {
                Documents =
                {
                    {
                        "parent", new DocumentDelta
                        {
                            Id = "parent"
                        }
                    }
                },
                DocumentTree =
                {
                    {
                        "parent", new TreeItem
                        {
                            Id = "parent"
                        }
                    }
                }
            };

            var dto = new CreateDocumentDto
            {
                Name = "Child-Document",
                DocumentId = "child",
                ParentDocumentId = "parent"
            };

            #endregion

            var newId = service.AddDocument(delta, dto);


            // The document has been added
            Assert.Equal(2, delta.Documents.Count);
            // The id that is returned is also the id of the document
            Assert.Equal(newId, delta.Documents[newId].Id);
            // The name that is returned is also the name of the document
            Assert.Equal("Child-Document", delta.Documents[newId].Value);
            // The provided document id has been set
            Assert.Equal("child", delta.Documents[newId].DocumentId);
            // The delta type is Create
            Assert.Equal(DeltaType.Create, delta.Documents[newId].Type);

            // The document has been added to the tree
            Assert.NotEmpty(delta.DocumentTree["parent"].Children);
            // The id that is returned is also the id of the document in the tree
            Assert.Equal(newId, delta.DocumentTree["parent"].Children[newId].Id);
        }

        #endregion

        #region RemoveDocument Tests

        [Fact]
        public void ShouldRemoveTopLevelDocument()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new DeltaFile
            {
                Documents =
                {
                    {
                        "document", new DocumentDelta
                        {
                            Id = "document"
                        }
                    }
                },
                DocumentTree =
                {
                    {
                        "document", new TreeItem
                        {
                            Id = "document"
                        }
                    }
                }
            };

            #endregion

            service.DeleteDocument(delta, "document");


            // The document has been removed, but the delta should exist
            Assert.Single(delta.Documents);
            // The id is unchanged
            Assert.Equal("document", delta.Documents["document"].Id);
            // The delta type is Delete
            Assert.Equal(DeltaType.Delete, delta.Documents["document"].Type);

            // The document has been removed from the tree
            Assert.Empty(delta.DocumentTree);
        }

        [Fact]
        public void ShouldRemoveChildDocument()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new DeltaFile
            {
                Documents =
                {
                    {
                        "parent", new DocumentDelta
                        {
                            Id = "parent"
                        }
                    },
                    {
                        "child", new DocumentDelta
                        {
                            Id = "child"
                        }
                    }
                },
                DocumentTree =
                {
                    {
                        "parent", new TreeItem
                        {
                            Id = "parent",
                            Children =
                            {
                                {
                                    "child", new TreeItem
                                    {
                                        Id = "child"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            #endregion

            service.DeleteDocument(delta, "child");


            // The document has been removed, but the delta should exist
            Assert.Equal(2, delta.Documents.Count);
            // The id is unchanged
            Assert.Equal("child", delta.Documents["child"].Id);
            // The delta type is Delete
            Assert.Equal(DeltaType.Delete, delta.Documents["child"].Type);

            // The document has been removed from the tree
            Assert.Empty(delta.DocumentTree["parent"].Children);
        }

        #endregion

        #region SeedDelta Tests

        [Fact]
        public void ShouldSeedDeltaWithDocumentTree()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new DeltaFile
            {
            };

            var model = new ParticipantModel
            {
                Data = new DataBody
                {
                    DocumentTree =
                    {
                        {
                            "document", new TreeItem
                            {
                                Id = "document"
                            }
                        }
                    }
                }
            };

            #endregion

            service.SeedDeltaFile(delta, model);


            // The document tree was updated
            Assert.Single(delta.DocumentTree);
            // The id is the same as the original
            Assert.Equal("document", delta.DocumentTree["document"].Id);
            // Trees should not be the same instance
            Assert.NotEqual(delta.DocumentTree, model.Data.DocumentTree);
        }

        [Fact]
        public void ShouldNotSeedDeltaWithDocumentTree()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var delta = new DeltaFile
            {
                DocumentTree =
                {
                    {
                        "document2", new TreeItem
                        {
                            Id = "document2"
                        }
                    }
                }
            };

            var model = new ParticipantModel
            {
                Data = new DataBody
                {
                    DocumentTree =
                    {
                        {
                            "document", new TreeItem
                            {
                                Id = "document"
                            }
                        }
                    }
                }
            };

            #endregion

            service.SeedDeltaFile(delta, model);


            // The document tree was not updated
            Assert.Single(delta.DocumentTree);
            Assert.DoesNotContain(delta.DocumentTree, t => t.Key == "document");
            // The id is the same as the original
            Assert.Equal("document2", delta.DocumentTree["document2"].Id);
            // Trees should not be the same instance
            Assert.NotEqual(delta.DocumentTree, model.Data.DocumentTree);
        }

        #endregion

        #region UpdateTree Tests

        [Fact]
        public void ShouldNotUpdateTreeWithInvalidPathNode()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var service = new DeltaService(cryptography.Object, new DocumentDeltaHelper());

            #region ObjectInitialization

            var tree = new Dictionary<string, TreeItem>();
            var updatePath = new[]
            {
                "invalid-parent",
                "child is not added"
            };

            #endregion

            var updatedTree = service.UpdateTree(updatePath, DeltaType.Create, tree);

            // The document tree was not updated
            Assert.Empty(updatedTree);
        }

        #endregion
    }
}