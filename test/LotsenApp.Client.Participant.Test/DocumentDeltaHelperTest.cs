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
using LotsenApp.Client.Participant.Delta;
using LotsenApp.Client.Participant.Model;
using Xunit;

namespace LotsenApp.Client.Participant.Test
{
    [ExcludeFromCodeCoverage]
    public class DocumentDeltaHelperTest
    {
        private static readonly IDictionary<string, TreeItem> ExampleTree = new Dictionary<string, TreeItem>
        {
            {"0", new TreeItem()},
            {"2", new TreeItem()},
            {"3", new TreeItem()},
        };

        private static readonly IDictionary<string, GroupDelta> ExampleGroupDeltas = new Dictionary<string, GroupDelta>
        {
            {
                "0", new GroupDelta
                {
                    Id = "0"
                }
            },
            {
                "1", new GroupDelta
                {
                    Id = "1"
                }
            },
            {
                "2", new GroupDelta
                {
                    Id = "2"
                }
            },
        };

        private static readonly IDictionary<string, DocumentDelta> ExampleDocumentDeltas =
            new Dictionary<string, DocumentDelta>
            {
                {"0", new DocumentDelta {Id = "0"}},
                {"1", new DocumentDelta {Id = "1"}},
                {"2", new DocumentDelta {Id = "2"}},
                {"3", new DocumentDelta {Id = "3"}},
            };

        [Fact]
        public void ShouldReturnEmptyArrayOnNotFound()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var result = documentDeltaHelper.FindTreeItem(ExampleTree, "1");

            Assert.Same(Array.Empty<string>(), result);
        }

        [Fact]
        public void ShouldReturnNullOnNotFound()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();
            var result = documentDeltaHelper.ResolveTreeItem(ExampleTree, "1");

            Assert.Null(result);
        }

        [Fact]
        public void ShouldReturnUnchangedDeltaOnNotFound()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();
            var invoked = false;
            var result = documentDeltaHelper.ResolveGroup(ExampleGroupDeltas, delta => invoked = true, "0", "1", "2");
            Assert.True(invoked);
        }

        [Fact]
        public void ShouldReturnInputWithNoPath()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();
            var invoked = false;
            var result = documentDeltaHelper.ResolveGroup(ExampleGroupDeltas, delta => invoked = true);
            Assert.False(invoked);
            Assert.Same(result, ExampleGroupDeltas);
        }

        [Fact]
        public void ShouldThrowWithNullId()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            Assert.Throws<ArgumentNullException>(() => documentDeltaHelper.ResolveDelta(ExampleDocumentDeltas, null));
        }

        [Fact]
        public void ShouldAddUnchangedDeltaWithNoPreexistingDelta()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var result = documentDeltaHelper.ResolveDelta(ExampleDocumentDeltas, "5");

            Assert.Equal("5", result.Id);
            Assert.Null(result.Value);
            Assert.Null(result.DocumentId);
            Assert.Null(result.OldValue);
            Assert.Empty(result.Groups);
            Assert.Empty(result.Values);
            Assert.Null(result.Ordinal);
            Assert.Equal(DeltaType.Unchanged, result.Type);
        }

        [Fact]
        public void ShouldReturnNullOnInvalidPath()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var result = documentDeltaHelper.ResolveField(new DocumentGroup(), "path");
            Assert.Null(result);
        }
        
        [Fact]
        public void ShouldReturnNullOnEmptyPath()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var result = documentDeltaHelper.ResolveField(new DocumentGroup());
            Assert.Null(result);
        }

        [Fact]
        public void ShouldReturnFieldWithGroupAndValidPath()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var group = new DocumentGroup
            {
                Fields = new Dictionary<string, DocumentField>
                {
                    {
                        "field", new DocumentField
                        {
                            Id = "field"
                        }
                    }
                }
            };

            var result = documentDeltaHelper.ResolveField(group, "field");
            Assert.Equal(group.Fields["field"], result);
        }
        
        [Fact]
        public void ShouldReturnFieldWithGroupChildrenAndValidPath()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var group = new DocumentGroup
            {
                Children = new Dictionary<string, DocumentGroup>
                {
                    {"child", new DocumentGroup
                    {
                        Id = "child",
                        Fields = new Dictionary<string, DocumentField>
                        {
                            {
                                "field", new DocumentField
                                {
                                    Id = "field"
                                }
                            }
                        }   
                    }}
                }
            };

            var result = documentDeltaHelper.ResolveField(group, "child", "field");
            Assert.Equal(group.Children["child"].Fields["field"], result);
        }
        
        [Fact]
        public void ShouldReturnNullWithGroupChildrenAndInvalidPath()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var group = new DocumentGroup
            {
                Children = new Dictionary<string, DocumentGroup>
                {
                    {"child", new DocumentGroup
                    {
                        Id = "child",
                        Fields = new Dictionary<string, DocumentField>
                        {
                            {
                                "field", new DocumentField
                                {
                                    Id = "field"
                                }
                            }
                        }   
                    }}
                }
            };

            var result = documentDeltaHelper.ResolveField(group, "child2", "field");
            Assert.Null(result);
        }
        
        [Fact]
        public void ShouldReturnNullOnEmptyPathWithDocument()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var result = documentDeltaHelper.ResolveField(new Document());
            Assert.Null(result);
        }
        
        [Fact]
        public void ShouldReturnFieldOnValidPathWithDocument()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();
            var document = new Document
            {
                Values = new Dictionary<string, DocumentField>
                {
                    {
                        "field", new DocumentField
                        {
                            Id = "field"
                        }
                    }
                }
            };
            var result = documentDeltaHelper.ResolveField(document, "field");
            Assert.Equal(document.Values["field"], result);
        }
        
        [Fact]
        public void ShouldReturnFieldOnValidPathWithDocumentGroup()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();
            var document = new Document
            {
                Groups = new Dictionary<string, DocumentGroup>
                {
                    {"child", new DocumentGroup
                    {
                        Fields = new Dictionary<string, DocumentField>
                        {
                            {
                                "field", new DocumentField
                                {
                                    Id = "field"
                                }
                            }
                        }
                    }}
                }
            };
            var result = documentDeltaHelper.ResolveField(document, "child", "field");
            Assert.Equal(document.Groups["child"].Fields["field"], result);
        }
        
        [Fact]
        public void ShouldReturnNullOnEmptyPathWithModel()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var result = documentDeltaHelper.ResolveField(new ParticipantModel());
            Assert.Null(result);
        }
        
        [Fact]
        public void ShouldReturnNullOnEmptyPathWithModelDocument()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var model = new ParticipantModel
            {
                Data = new DataBody
                {
                    Documents = new Dictionary<string, Document>
                    {
                        {"document", new Document()}
                    }
                }
            };
            
            var result = documentDeltaHelper.ResolveField(model, "document");
            Assert.Null(result);
        }
        
         [Fact]
        public void ShouldReturnEmptyArrayOnInvalidProjectPath()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var result = documentDeltaHelper.ResolveFieldsWithProjectPath(new DocumentGroup(), "path");
            Assert.Empty(result);
        }
        
        [Fact]
        public void ShouldReturnEmptyArrayOnEmptyProjectPath()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var result = documentDeltaHelper.ResolveFieldsWithProjectPath(new DocumentGroup());
            Assert.Empty(result);
        }

        [Fact]
        public void ShouldReturnFieldWithGroupAndValidProjectPath()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var group = new DocumentGroup
            {
                Fields = new Dictionary<string, DocumentField>
                {
                    {
                        "field", new DocumentField
                        {
                            Id = "field"
                        }
                    }
                }
            };

            var result = documentDeltaHelper.ResolveFieldsWithProjectPath(group, "field");
            Assert.Equal(group.Fields["field"], result[0]);
        }
        
        [Fact]
        public void ShouldReturnFieldWithGroupChildrenAndValidProjectPath()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var group = new DocumentGroup
            {
                Children = new Dictionary<string, DocumentGroup>
                {
                    {"child", new DocumentGroup
                    {
                        Id = "child",
                        GroupId = "group",
                        Fields = new Dictionary<string, DocumentField>
                        {
                            {
                                "field", new DocumentField
                                {
                                    Id = "field"
                                }
                            }
                        }   
                    }}
                }
            };

            var result = documentDeltaHelper.ResolveFieldsWithProjectPath(group, "group", "field");
            Assert.Equal(group.Children["child"].Fields["field"], result[0]);
        }
        
        [Fact]
        public void ShouldReturnEmptyArrayWithGroupChildrenAndInvalidProjectPath()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var group = new DocumentGroup
            {
                Children = new Dictionary<string, DocumentGroup>
                {
                    {"child", new DocumentGroup
                    {
                        Id = "child",
                        GroupId = "group",
                        Fields = new Dictionary<string, DocumentField>
                        {
                            {
                                "field", new DocumentField
                                {
                                    Id = "field"
                                }
                            }
                        }   
                    }}
                }
            };

            var result = documentDeltaHelper.ResolveFieldsWithProjectPath(group, "child", "field");
            Assert.Empty(result);
        }
        
        [Fact]
        public void ShouldReturnEmptyArrayOnEmptyProjectPathWithDocument()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var result = documentDeltaHelper.ResolveFieldsWithProjectPath(new Document());
            Assert.Empty(result);
        }
        
        [Fact]
        public void ShouldReturnFieldOnValidProjectPathWithDocument()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();
            var document = new Document
            {
                Values = new Dictionary<string, DocumentField>
                {
                    {
                        "field", new DocumentField
                        {
                            Id = "field"
                        }
                    }
                }
            };
            var result = documentDeltaHelper.ResolveFieldsWithProjectPath(document, "field");
            Assert.Equal(document.Values["field"], result[0]);
        }
        
        [Fact]
        public void ShouldReturnFieldOnValidProjectPathWithDocumentGroup()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();
            var document = new Document
            {
                Groups = new Dictionary<string, DocumentGroup>
                {
                    {"child", new DocumentGroup
                    {
                        GroupId = "group",
                        Fields = new Dictionary<string, DocumentField>
                        {
                            {
                                "field", new DocumentField
                                {
                                    Id = "field"
                                }
                            }
                        }
                    }}
                }
            };
            var result = documentDeltaHelper.ResolveFieldsWithProjectPath(document, "group", "field");
            Assert.Equal(document.Groups["child"].Fields["field"], result[0]);
        }
        
        [Fact]
        public void ShouldReturnEmptyArrayOnEmptyProjectPathWithModel()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var result = documentDeltaHelper.ResolveFieldsWithProjectPath(new ParticipantModel());
            Assert.Empty(result);
        }
        
        [Fact]
        public void ShouldReturnEmptyArrayOnEmptyProjectPathWithModelDocument()
        {
            var documentDeltaHelper = new DocumentDeltaHelper();

            var model = new ParticipantModel
            {
                Data = new DataBody
                {
                    Documents = new Dictionary<string, Document>
                    {
                        {"document", new Document
                        {
                            DocumentId = "doc"
                        }}
                    }
                }
            };
            
            var result = documentDeltaHelper.ResolveFieldsWithProjectPath(model, "doc");
            Assert.Empty(result);
        }
    }
}