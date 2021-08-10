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
using System.Diagnostics.CodeAnalysis;
using LotsenApp.Client.Authentication.DataPassword;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.Participant.Delta;
using LotsenApp.Client.Participant.Header;
using LotsenApp.Client.Participant.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LotsenApp.Client.Participant.Test.Delta
{

    [ExcludeFromCodeCoverage]
    public class MergeServiceTest
    {
        public MergeService CreateInstance()
        {
            var dataPasswordMock = new Mock<DataPasswordService>();
            var configuration = new Mock<IConfigurationStorage>();
            var headerService = new Mock<IParticipantHeaderService>();
            var cryptography = new Mock<ParticipantCryptographyService>(dataPasswordMock.Object, configuration.Object);
            var logger = new Mock<ILogger<MergeService>>();
            headerService.Setup(h => h.CalculateHeader(It.IsAny<string>(), It.IsAny<ParticipantModel>()))
                .Returns((string userId, ParticipantModel m) => m);
            return new MergeService(cryptography.Object, headerService.Object, logger.Object);
        }

        #region MergeField Tests

        [Fact]
        public void ShouldCreateFieldWithCreateDelta()
        {
            var mergeService = CreateInstance();

            var delta = new ValueDelta
            {
                Id = "delta",
                Type = DeltaType.Create,
                Value = "value",
                UseDisplay = 1
            };

            // Delta type is created. No field should be needed
            var createdField = mergeService.MergeField(null, delta);

            Assert.Equal(delta.Id, createdField.Id);
            Assert.Equal(delta.Value, createdField.Value);
            Assert.Equal(delta.UseDisplay, createdField.UseDisplay);
        }

        [Fact]
        public void ShouldReturnNullWithDeleteDelta()
        {
            var mergeService = CreateInstance();

            var delta = new ValueDelta
            {
                Id = "delta",
                Type = DeltaType.Delete,
                Value = "value",
                UseDisplay = 1
            };

            // Delta type is delete. No field should be needed
            var createdField = mergeService.MergeField(null, delta);

            Assert.Null(createdField);
        }

        [Fact]
        public void ShouldReturnFieldWithUnchangedDelta()
        {
            var mergeService = CreateInstance();

            var delta = new ValueDelta
            {
                Id = "delta",
                Type = DeltaType.Unchanged,
                Value = "value",
                UseDisplay = 1
            };

            var field = new DocumentField
            {
                Id = "delta",
                Value = "oldValue",
                UseDisplay = 0
            };


            var createdField = mergeService.MergeField(field, delta);

            Assert.Equal(field, createdField);
            Assert.Equal("delta", createdField.Id);
            Assert.Equal("oldValue", createdField.Value);
            Assert.Equal(0, createdField.UseDisplay);
        }

        [Fact]
        public void ShouldUpdateFieldWithUpdateDelta()
        {
            var mergeService = CreateInstance();

            var delta = new ValueDelta
            {
                Id = "delta",
                Type = DeltaType.Update,
                Value = "value",
                UseDisplay = 1
            };

            var field = new DocumentField
            {
                Id = "delta",
                Value = "oldValue",
                UseDisplay = 0
            };

            // Delta type is created. No field should be needed
            var createdField = mergeService.MergeField(field, delta);

            Assert.Equal("delta", createdField.Id);
            Assert.Equal(delta.Value, createdField.Value);
            Assert.Equal(delta.UseDisplay, createdField.UseDisplay);
        }

        [Fact]
        public void ShouldReturnNullWithUnknownDelta()
        {
            var mergeService = CreateInstance();

            // Delta type is unknown. No field should be needed
            var createdField = mergeService.MergeField(null, null);

            Assert.Null(createdField);
        }

        [Fact]
        public void ShouldThrowWithNullFieldAndUpdateDelta()
        {
            var mergeService = CreateInstance();

            var delta = new ValueDelta
            {
                Id = "delta",
                Type = DeltaType.Update,
                Value = "value",
                UseDisplay = 1
            };

            Assert.Throws<NullReferenceException>(() => mergeService.MergeField(null, delta));
        }

        #endregion

        #region UpdateGroupChildren Tests

        [Fact]
        public void ShouldUpdateAllChildrenGroups()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new DocumentGroup
            {
                Id = "document-group",
                Ordinal = 0,
                GroupId = "group-id",
                Children =
                {
                    {
                        "child-1", new DocumentGroup
                        {
                            Id = "child-1",
                            Ordinal = 0,
                            GroupId = "group-id"
                        }
                    }
                }
            };
            var delta = new GroupDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
                Children =
                {
                    {
                        "child-1", new GroupDelta
                        {
                            Id = "child-1",
                            Ordinal = 1,
                            GroupId = "group-id",
                            Type = DeltaType.Update
                        }
                    }
                }
            };

            #endregion

            mergeService.UpdateGroupChildren(documentGroup, delta);

            var updatedGroup = documentGroup.Children["child-1"];
            Assert.Equal(1, updatedGroup.Ordinal);
        }

        [Fact]
        public void ShouldDeleteAllChildrenGroups()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new DocumentGroup
            {
                Id = "document-group",
                Ordinal = 0,
                GroupId = "group-id",
                Children =
                {
                    {
                        "child-1", new DocumentGroup
                        {
                            Id = "child-1",
                            Ordinal = 0,
                            GroupId = "group-id"
                        }
                    }
                }
            };
            var delta = new GroupDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
                Children =
                {
                    {
                        "child-1", new GroupDelta
                        {
                            Id = "child-1",
                            Ordinal = 1,
                            GroupId = "group-id",
                            Type = DeltaType.Delete
                        }
                    }
                }
            };

            #endregion

            mergeService.UpdateGroupChildren(documentGroup, delta);

            Assert.Empty(documentGroup.Children);
        }

        [Fact]
        public void ShouldCreateNewChildrenGroups()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new DocumentGroup
            {
                Id = "document-group",
                Ordinal = 0,
                GroupId = "group-id",
                Children =
                {
                    {
                        "child-1", new DocumentGroup
                        {
                            Id = "child-1",
                            Ordinal = 0,
                            GroupId = "group-id"
                        }
                    }
                }
            };
            var delta = new GroupDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
                Children =
                {
                    {
                        "child-2", new GroupDelta
                        {
                            Id = "child-2",
                            Ordinal = 1,
                            GroupId = "group-id",
                            Type = DeltaType.Create
                        }
                    }
                }
            };

            #endregion

            mergeService.UpdateGroupChildren(documentGroup, delta);

            var createdGroup = documentGroup.Children["child-2"];
            Assert.Equal(2, documentGroup.Children.Count);
            Assert.Equal(1, createdGroup.Ordinal);
            Assert.Equal("child-2", createdGroup.Id);
            Assert.Equal("group-id", createdGroup.GroupId);
        }

        [Fact]
        public void ShouldLeaveChildrenGroupsUnchanged()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new DocumentGroup
            {
                Id = "document-group",
                Ordinal = 0,
                GroupId = "group-id",
                Children =
                {
                    {
                        "child-1", new DocumentGroup
                        {
                            Id = "child-1",
                            Ordinal = 0,
                            GroupId = "group-id"
                        }
                    }
                }
            };
            var delta = new GroupDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
            };

            #endregion

            mergeService.UpdateGroupChildren(documentGroup, delta);

            var unchangedGroup = documentGroup.Children["child-1"];
            Assert.Equal("child-1", unchangedGroup.Id);
            Assert.Equal(0, unchangedGroup.Ordinal);
            Assert.Equal("group-id", unchangedGroup.GroupId);
        }

        [Fact]
        public void ShouldUpdateAllChildrenFields()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new DocumentGroup
            {
                Id = "document-group",
                Ordinal = 0,
                GroupId = "group-id",
                Fields =
                {
                    {
                        "child-1", new DocumentField
                        {
                            Id = "child-1",
                            Value = "oldValue"
                        }
                    }
                }
            };
            var delta = new GroupDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
                Fields =
                {
                    {
                        "child-1", new ValueDelta
                        {
                            Id = "child-1",
                            Type = DeltaType.Update,
                            Value = "value"
                        }
                    }
                }
            };

            #endregion

            mergeService.UpdateGroupChildren(documentGroup, delta);

            var updatedField = documentGroup.Fields["child-1"];
            Assert.Equal("value", updatedField.Value);
        }

        [Fact]
        public void ShouldDeleteAllChildrenFields()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new DocumentGroup
            {
                Id = "document-group",
                Ordinal = 0,
                GroupId = "group-id",
                Fields =
                {
                    {
                        "child-1", new DocumentField
                        {
                            Id = "child-1",
                            Value = "oldValue"
                        }
                    }
                }
            };
            var delta = new GroupDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
                Fields =
                {
                    {
                        "child-1", new ValueDelta
                        {
                            Id = "child-1",
                            Type = DeltaType.Delete,
                            Value = "value"
                        }
                    }
                }
            };

            #endregion

            mergeService.UpdateGroupChildren(documentGroup, delta);

            Assert.Empty(documentGroup.Fields);
        }
        
        [Fact]
        public void ShouldCreateChildrenFields()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new DocumentGroup
            {
                Id = "document-group",
                Ordinal = 0,
                GroupId = "group-id",
                Fields =
                {
                    {
                        "child-1", new DocumentField
                        {
                            Id = "child-1",
                            Value = "oldValue"
                        }
                    }
                }
            };
            var delta = new GroupDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
                Fields = 
                {
                    {
                        "child-1", new ValueDelta
                        {
                            Id = "child-1",
                            Type = DeltaType.Update,
                            Value = "value"
                        }
                    }
                }
            };

            #endregion
            
            mergeService.UpdateGroupChildren(documentGroup, delta);

            var updatedField = documentGroup.Fields["child-1"];
            Assert.Equal("value", updatedField.Value);
        }
        
        [Fact]
        public void ShouldLeaveUnchangedChildrenFields()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new DocumentGroup
            {
                Id = "document-group",
                Ordinal = 0,
                GroupId = "group-id",
                Fields =
                {
                    {
                        "child-1", new DocumentField
                        {
                            Id = "child-1",
                            Value = "oldValue"
                        }
                    }
                }
            };
            var delta = new GroupDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
                Fields = 
                {
                    {
                        "child-1", new ValueDelta
                        {
                            Id = "child-1",
                            Type = DeltaType.Unchanged,
                            Value = "value"
                        }
                    }
                }
            };

            #endregion
            
            mergeService.UpdateGroupChildren(documentGroup, delta);

            var updatedField = documentGroup.Fields["child-1"];
            Assert.Equal("oldValue", updatedField.Value);
        }
        
        [Fact]
        public void ShouldLeaveChildrenFieldsWithNoDelta()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new DocumentGroup
            {
                Id = "document-group",
                Ordinal = 0,
                GroupId = "group-id",
                Fields =
                {
                    {
                        "child-1", new DocumentField
                        {
                            Id = "child-1",
                            Value = "oldValue"
                        }
                    }
                }
            };
            var delta = new GroupDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
            };

            #endregion
            
            mergeService.UpdateGroupChildren(documentGroup, delta);

            var updatedField = documentGroup.Fields["child-1"];
            Assert.Equal("oldValue", updatedField.Value);
        }

        #endregion
        
        #region MergeGroup Tests
        
        [Fact]
        public void ShouldCreateNewGroup()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var delta = new GroupDelta
            {
                Id = "id",
                GroupId = "group-id",
                Ordinal = 0,
                Type = DeltaType.Create,
            };

            #endregion
            
            // No document group should be needed
            var updatedGroup = mergeService.MergeGroup(null, delta);

            Assert.Equal("id", updatedGroup.Id);
            Assert.Equal("group-id", updatedGroup.GroupId);
            Assert.Equal(0, updatedGroup.Ordinal);
        }
        
        [Fact]
        public void ShouldCreateNewGroupWithChildren()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var delta = new GroupDelta
            {
                Id = "id",
                GroupId = "group-id",
                Ordinal = 0,
                Type = DeltaType.Create,
                Children =
                {
                    {"child-1", new GroupDelta
                    {
                        Id = "child-1",
                        Type = DeltaType.Create,
                        Ordinal = 0,
                        GroupId = "group-id",
                    }}
                },
                Fields =
                {
                    {"field-1", new ValueDelta
                    {
                        Id = "field-1",
                        Value = "value",
                        Type = DeltaType.Create,
                    }}
                }
            };

            #endregion
            
            // No document group should be needed
            var createdGroup = mergeService.MergeGroup(null, delta);

            Assert.Equal("id", createdGroup.Id);
            Assert.Equal("group-id", createdGroup.GroupId);
            Assert.Equal(0, createdGroup.Ordinal);
            Assert.Single(createdGroup.Children);
            Assert.Single(createdGroup.Fields);
        }
        
        [Fact]
        public void ShouldUpdateGroup()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var delta = new GroupDelta
            {
                Id = "id",
                GroupId = "group-id",
                Ordinal = 0,
                Type = DeltaType.Update,
            };

            var group = new DocumentGroup
            {
                Id = "id",
                GroupId = "group-id",
                Ordinal = 1,
            };

            #endregion
            
            var updatedGroup = mergeService.MergeGroup(group, delta);

            Assert.Equal("id", updatedGroup.Id);
            Assert.Equal("group-id", updatedGroup.GroupId);
            Assert.Equal(0, updatedGroup.Ordinal);
        }
        
        [Fact]
        public void ShouldDeleteGroup()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var delta = new GroupDelta
            {
                Id = "id",
                GroupId = "group-id",
                Ordinal = 0,
                Type = DeltaType.Delete,
            };

            #endregion
            
            // No document group should be needed
            var updatedGroup = mergeService.MergeGroup(null, delta);

            Assert.Null(updatedGroup);
        }
        
        [Fact]
        public void ShouldLeaveGroupAsIs()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var delta = new GroupDelta
            {
                Id = "id",
                GroupId = "group-id",
                Ordinal = 0,
                Type = DeltaType.Unchanged,
            };
            
            var group = new DocumentGroup
            {
                Id = "id",
                GroupId = "group-id",
                Ordinal = 1,
            };

            #endregion
            
            // No document group should be needed
            var updatedGroup = mergeService.MergeGroup(group, delta);

            Assert.Equal("id", updatedGroup.Id);
            Assert.Equal("group-id", updatedGroup.GroupId);
            Assert.Equal(1, updatedGroup.Ordinal);
        }
        
        [Fact]
        public void ShouldReturnNullOnUnknownDelta()
        {
            var mergeService = CreateInstance();

            // No document group should be needed
            var updatedGroup = mergeService.MergeGroup(null, null);

            Assert.Null(updatedGroup);
        }

        #endregion
        
        #region UpdateChildren Tests

        [Fact]
        public void DocumentShouldUpdateAllChildrenGroups()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new Document
            {
                Id = "document-group",
                Ordinal = 0,
                Groups = 
                {
                    {
                        "child-1", new DocumentGroup
                        {
                            Id = "child-1",
                            Ordinal = 0,
                            GroupId = "group-id"
                        }
                    }
                }
            };
            var delta = new DocumentDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
                Groups = 
                {
                    {
                        "child-1", new GroupDelta
                        {
                            Id = "child-1",
                            Ordinal = 1,
                            GroupId = "group-id",
                            Type = DeltaType.Update
                        }
                    }
                }
            };

            #endregion

            mergeService.UpdateChildren(documentGroup, delta);

            var updatedGroup = documentGroup.Groups["child-1"];
            Assert.Equal(1, updatedGroup.Ordinal);
        }

        [Fact]
        public void DocumentShouldDeleteAllChildrenGroups()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new Document
            {
                Id = "document-group",
                Ordinal = 0,
                Groups = 
                {
                    {
                        "child-1", new DocumentGroup
                        {
                            Id = "child-1",
                            Ordinal = 0,
                            GroupId = "group-id"
                        }
                    }
                }
            };
            var delta = new DocumentDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
                Groups = 
                {
                    {
                        "child-1", new GroupDelta
                        {
                            Id = "child-1",
                            Ordinal = 1,
                            GroupId = "group-id",
                            Type = DeltaType.Delete
                        }
                    }
                }
            };

            #endregion

            mergeService.UpdateChildren(documentGroup, delta);

            Assert.Empty(documentGroup.Groups);
        }

        [Fact]
        public void DocumentShouldCreateNewChildrenGroups()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new Document
            {
                Id = "document-group",
                Ordinal = 0,
                Groups = 
                {
                    {
                        "child-1", new DocumentGroup
                        {
                            Id = "child-1",
                            Ordinal = 0,
                            GroupId = "group-id"
                        }
                    }
                }
            };
            var delta = new DocumentDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
                Groups = 
                {
                    {
                        "child-2", new GroupDelta
                        {
                            Id = "child-2",
                            Ordinal = 1,
                            GroupId = "group-id",
                            Type = DeltaType.Create
                        }
                    }
                }
            };

            #endregion

            mergeService.UpdateChildren(documentGroup, delta);

            var createdGroup = documentGroup.Groups["child-2"];
            Assert.Equal(2, documentGroup.Groups.Count);
            Assert.Equal(1, createdGroup.Ordinal);
            Assert.Equal("child-2", createdGroup.Id);
            Assert.Equal("group-id", createdGroup.GroupId);
        }

        [Fact]
        public void DocumentShouldLeaveChildrenGroupsUnchanged()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new Document
            {
                Id = "document-group",
                Ordinal = 0,
                Groups = 
                {
                    {
                        "child-1", new DocumentGroup
                        {
                            Id = "child-1",
                            Ordinal = 0,
                            GroupId = "group-id"
                        }
                    }
                }
            };
            var delta = new DocumentDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
            };

            #endregion

            mergeService.UpdateChildren(documentGroup, delta);

            var unchangedGroup = documentGroup.Groups["child-1"];
            Assert.Equal("child-1", unchangedGroup.Id);
            Assert.Equal(0, unchangedGroup.Ordinal);
            Assert.Equal("group-id", unchangedGroup.GroupId);
        }

        [Fact]
        public void DocumentShouldUpdateAllChildrenFields()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new Document
            {
                Id = "document-group",
                Ordinal = 0,
                Values = 
                {
                    {
                        "child-1", new DocumentField
                        {
                            Id = "child-1",
                            Value = "oldValue"
                        }
                    }
                }
            };
            var delta = new DocumentDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
                Values = 
                {
                    {
                        "child-1", new ValueDelta
                        {
                            Id = "child-1",
                            Type = DeltaType.Update,
                            Value = "value"
                        }
                    }
                }
            };

            #endregion

            mergeService.UpdateChildren(documentGroup, delta);

            var updatedField = documentGroup.Values["child-1"];
            Assert.Equal("value", updatedField.Value);
        }

        [Fact]
        public void DocumentShouldDeleteAllChildrenFields()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new Document
            {
                Id = "document-group",
                Ordinal = 0,
                Values =
                {
                    {
                        "child-1", new DocumentField
                        {
                            Id = "child-1",
                            Value = "oldValue"
                        }
                    }
                }
            };
            var delta = new DocumentDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
                Values = 
                {
                    {
                        "child-1", new ValueDelta
                        {
                            Id = "child-1",
                            Type = DeltaType.Delete,
                            Value = "value"
                        }
                    }
                }
            };

            #endregion

            mergeService.UpdateChildren(documentGroup, delta);

            Assert.Empty(documentGroup.Values);
        }
        
        [Fact]
        public void DocumentShouldCreateChildrenFields()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new Document
            {
                Id = "document-group",
                Ordinal = 0,
                Values =
                {
                    {
                        "child-1", new DocumentField
                        {
                            Id = "child-1",
                            Value = "oldValue"
                        }
                    }
                }
            };
            var delta = new DocumentDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
                Values = 
                {
                    {
                        "child-1", new ValueDelta
                        {
                            Id = "child-1",
                            Type = DeltaType.Update,
                            Value = "value"
                        }
                    }
                }
            };

            #endregion
            
            mergeService.UpdateChildren(documentGroup, delta);

            var updatedField = documentGroup.Values["child-1"];
            Assert.Equal("value", updatedField.Value);
        }
        
        [Fact]
        public void DocumentShouldLeaveUnchangedChildrenFields()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new Document
            {
                Id = "document-group",
                Ordinal = 0,
                Values =
                {
                    {
                        "child-1", new DocumentField
                        {
                            Id = "child-1",
                            Value = "oldValue"
                        }
                    }
                }
            };
            var delta = new DocumentDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
                Values = 
                {
                    {
                        "child-1", new ValueDelta
                        {
                            Id = "child-1",
                            Type = DeltaType.Unchanged,
                            Value = "value"
                        }
                    }
                }
            };

            #endregion
            
            mergeService.UpdateChildren(documentGroup, delta);

            var updatedField = documentGroup.Values["child-1"];
            Assert.Equal("oldValue", updatedField.Value);
        }
        
        [Fact]
        public void DocumentShouldLeaveChildrenFieldsWithNoDelta()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var documentGroup = new Document
            {
                Id = "document-group",
                Ordinal = 0,
                Values = 
                {
                    {
                        "child-1", new DocumentField
                        {
                            Id = "child-1",
                            Value = "oldValue"
                        }
                    }
                }
            };
            var delta = new DocumentDelta
            {
                Id = "document-group",
                Type = DeltaType.Unchanged,
            };

            #endregion
            
            mergeService.UpdateChildren(documentGroup, delta);

            var updatedField = documentGroup.Values["child-1"];
            Assert.Equal("oldValue", updatedField.Value);
        }

        #endregion
        
        #region MergeDocument Tests
        
        [Fact]
        public void ShouldCreateNewDocument()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var delta = new DocumentDelta
            {
                Id = "id",
                Ordinal = 0,
                Type = DeltaType.Create,
                DocumentId = "document-id",
                Value = "document",
            };

            #endregion
            
            // No document group should be needed
            var updatedGroup = mergeService.MergeDocument(null, delta);

            Assert.Equal("id", updatedGroup.Id);
            Assert.Equal("document-id", updatedGroup.DocumentId);
            Assert.Equal(0, updatedGroup.Ordinal);
            Assert.Equal("document", updatedGroup.Name);
        }
        
        [Fact]
        public void ShouldCreateNewDocumentWithValues()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var delta = new DocumentDelta
            {
                Id = "id",
                Ordinal = 0,
                Type = DeltaType.Create,
                DocumentId = "document-id",
                Value = "document",
                Values =
                {
                    {"field", new ValueDelta
                    {
                        Id = "field",
                        Value = "field-value",
                        Type = DeltaType.Create
                    }}
                },
                Groups =
                {
                    {"group", new GroupDelta
                    {
                        Id = "group",
                        Type = DeltaType.Create,
                        GroupId = "group-id",
                    }}
                }
            };

            #endregion
            
            // No document group should be needed
            var createdDocument = mergeService.MergeDocument(null, delta);

            Assert.Equal("id", createdDocument.Id);
            Assert.Equal("document-id", createdDocument.DocumentId);
            Assert.Equal(0, createdDocument.Ordinal);
            Assert.Equal("document", createdDocument.Name);
            Assert.Single(createdDocument.Values);
            Assert.Single(createdDocument.Groups);
        }
        
        [Fact]
        public void ShouldUpdateDocument()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var delta = new DocumentDelta
            {
                Id = "id",
                Value = "new-value",
                Ordinal = 0,
                Type = DeltaType.Update,
            };

            var group = new Document
            {
                Id = "id",
                Ordinal = 1,
                Name = "value"
            };

            #endregion
            
            var updatedGroup = mergeService.MergeDocument(group, delta);

            Assert.Equal("id", updatedGroup.Id);
            Assert.Equal("new-value", updatedGroup.Name);
            Assert.Equal(0, updatedGroup.Ordinal);
        }
        
        [Fact]
        public void ShouldDeleteDocument()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var delta = new DocumentDelta
            {
                Id = "id",
                Ordinal = 0,
                Type = DeltaType.Delete,
            };

            #endregion
            
            // No document group should be needed
            var updatedGroup = mergeService.MergeDocument(null, delta);

            Assert.Null(updatedGroup);
        }
        
        [Fact]
        public void ShouldLeaveDocumentAsIs()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var delta = new DocumentDelta
            {
                Id = "id",
                Ordinal = 0,
                Value = "newValue",
                Type = DeltaType.Unchanged,
            };
            
            var group = new Document
            {
                Id = "id",
                Ordinal = 1,
                Name = "document"
            };

            #endregion
            
            // No document group should be needed
            var updatedGroup = mergeService.MergeDocument(group, delta);

            Assert.Equal("id", updatedGroup.Id);
            Assert.Equal("document", updatedGroup.Name);
            Assert.Equal(1, updatedGroup.Ordinal);
        }
        
        [Fact]
        public void DocumentShouldReturnNullOnUnknownDelta()
        {
            var mergeService = CreateInstance();

            // No document group should be needed
            var updatedGroup = mergeService.MergeDocument(null, null);

            Assert.Null(updatedGroup);
        }

        #endregion
        
        #region MergeFile Tests

        [Fact]
        public void FileShouldUpdateAllChildren()
        {
            var mergeService = CreateInstance();
            
            #region ObjectInitialization

            var delta = new DeltaFile
            {
                Documents =
                {
                    {"document-1", new DocumentDelta
                    {
                        Id = "document-1",
                        Type = DeltaType.Update,
                        Value = "newValue",
                        Ordinal = 0
                    }}
                }
            };
            var model = new ParticipantModel
            {
                Data = new DataBody
                {
                    Documents =
                    {
                        {"document-1", new Document
                        {
                            Id = "document-1",
                            Name = "value",
                            Ordinal = 1,
                        }}
                    }
                }
            };

            #endregion

            var updatedFile = mergeService.MergeFile(model, delta);

            var updatedDocument = updatedFile.Data.Documents["document-1"];
            Assert.Equal(0, updatedDocument.Ordinal);
            Assert.Equal("newValue", updatedDocument.Name);
        }

        [Fact]
        public void FileShouldDeleteAllChildrenGroups()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var delta = new DeltaFile
            {
                Documents =
                {
                    {"document-1", new DocumentDelta
                    {
                        Id = "document-1",
                        Type = DeltaType.Delete,
                        Value = "newValue",
                        Ordinal = 0
                    }}
                }
            };
            var model = new ParticipantModel
            {
                Data = new DataBody
                {
                    Documents =
                    {
                        {"document-1", new Document
                        {
                            Id = "document-1",
                            Name = "value",
                            Ordinal = 1,
                        }}
                    }
                }
            };

            #endregion

            var updatedFile = mergeService.MergeFile(model, delta);

            Assert.Empty(updatedFile.Data.Documents);
        }

        [Fact]
        public void FileShouldCreateNewDocuments()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var delta = new DeltaFile
            {
                Documents =
                {
                    {"document-2", new DocumentDelta
                    {
                        Id = "document-2",
                        Type = DeltaType.Create,
                        Value = "newValue",
                        DocumentId = "document-id",
                        Ordinal = 0
                    }}
                }
            };
            var model = new ParticipantModel
            {
                Data = new DataBody
                {
                    Documents =
                    {
                        {"document-1", new Document
                        {
                            Id = "document-1",
                            Name = "value",
                            Ordinal = 1,
                        }}
                    }
                }
            };

            #endregion

            var updatedFile = mergeService.MergeFile(model, delta);

            var createdDocument = updatedFile.Data.Documents["document-2"];
            Assert.Equal(2, updatedFile.Data.Documents.Count);
            Assert.Equal(0, createdDocument.Ordinal);
            Assert.Equal("document-2", createdDocument.Id);
            Assert.Equal("document-id", createdDocument.DocumentId);
        }

        [Fact]
        public void FileShouldLeaveDocumentsUnchanged()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var delta = new DeltaFile
            {
                Documents =
                {
                    {"document-1", new DocumentDelta
                    {
                        Id = "document-1",
                        Type = DeltaType.Unchanged,
                        Value = "newValue",
                        DocumentId = "document-id",
                        Ordinal = 0
                    }}
                }
            };
            var model = new ParticipantModel
            {
                Data = new DataBody
                {
                    Documents =
                    {
                        {"document-1", new Document
                        {
                            Id = "document-1",
                            Name = "value",
                            Ordinal = 1,
                        }}
                    }
                }
            };

            #endregion

            var updatedFile = mergeService.MergeFile(model, delta);

            var createdDocument = updatedFile.Data.Documents["document-1"];
            Assert.Equal(1, createdDocument.Ordinal);
            Assert.Equal("document-1", createdDocument.Id);
            Assert.Null(createdDocument.DocumentId);
            Assert.Equal("value", createdDocument.Name);
        }
        
        [Fact]
        public void NoUpdatesOnEmptyDelta()
        {
            var mergeService = CreateInstance();

            #region ObjectInitialization

            var delta = new DeltaFile
            {
            };
            var model = new ParticipantModel
            {
                Data = new DataBody
                {
                    Documents =
                    {
                        {"document-1", new Document
                        {
                            Id = "document-1",
                            Name = "value",
                            Ordinal = 1,
                        }}
                    }
                }
            };

            #endregion

            var updatedFile = mergeService.MergeFile(model, delta);
            
            Assert.Equal(model, updatedFile);
        }

        #endregion
    }
}