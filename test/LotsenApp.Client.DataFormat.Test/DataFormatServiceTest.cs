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
using LotsenApp.Client.DataFormat.Access;
using LotsenApp.Client.DataFormat.Definition;
using LotsenApp.Client.DataFormat.Display;
using Moq;
using Xunit;

namespace LotsenApp.Client.DataFormat.Test
{
    [ExcludeFromCodeCoverage]
    public class DataFormatServiceTest
    {
        private static readonly Project Project1 = new Project
        {
            Id = "proj-id",
            Name = "Test Project",
            OpenForDocumentation = false,
            DataDefinition = new DataDefinition
            {
                DocumentationEvents = new List<DocumentationEvent>
                {
                    new DocumentationEvent
                    {
                        Id = "doc-event-id",
                        Name = "Test Documentation Event",
                        DocumentId = "doc-id0",
                        ValidDocuments = new List<string>
                        {
                            "doc-id"
                        }
                    },
                    new DocumentationEvent
                    {
                        Id = "doc-event-id2",
                        DocumentId = "doc-id",
                        ValidDocuments = new List<string>
                        {
                            "doc-id0"
                        }
                    }
                },
                Documents = new List<Document>
                {
                    new()
                    {
                        Id = "doc-id",
                        Name = "Document",
                    }
                }
            },
            DataDisplay = new DataDisplay
            {
                DocumentationEvents = new List<DocumentationEventDisplay>
                {
                    new DocumentationEventDisplay
                    {
                        Id = "doc-event-id",
                        I18NKey = "Test.I18N",
                        ValidDocuments = new List<DocumentDocumentationEventDisplay>
                        {
                            new DocumentDocumentationEventDisplay
                            {
                                Id = "doc-id",
                                Ordinal = 9
                            }
                        }
                    }
                },
                Documents = new List<DocumentDisplay>
                {
                    new DocumentDisplay()
                    {
                        Id = "doc-id",
                        I18NKey = "Test.I18N"
                    }
                },
                TopLevelDocuments = new List<string>
                {
                    "doc-id"
                }
            }
        };

        private static readonly Project Project2 = new Project
        {
            Id = "proj-id2",
            Name = "Test Project 2",
            OpenForDocumentation = true
        };
        
        private readonly DataFormatService _service;
        public DataFormatServiceTest()
        {
            var storageMock = new Mock<IDataFormatStorage>();
            storageMock.Setup(sto => sto.Associations)
                .Returns(new Dictionary<string, string[]>
                {
                    {"proj-id", new[] {"id"}},
                    {"proj-id2", new []{"id2"}}
                });
            storageMock.Setup(sto => sto.Projects)
                .Returns(new []
                {
                    Project1,
                    Project2
                });
            storageMock.Setup(sto => sto.GetSpecificProject("proj-id"))
                .Returns(Project1);
            storageMock.Setup(sto => sto.GetSpecificProject("proj-id2"))
                .Returns(Project2);
            _service = new DataFormatService(storageMock.Object);
        }
        
        [Fact]
        public async Task UserShouldBeInProject()
        {
            Assert.True(await _service.IsUserInProject("id", "proj-id"));
        }
        
        [Fact]
        public async Task UserShouldNotBeInUnknownProject()
        {
            Assert.False(await _service.IsUserInProject("id", "proj-id3"));
        }
        
        [Fact]
        public async Task ShouldGetAllDocumentationProjects()
        {
            var projects = await _service.UserHasAccessToProject("id", "proj-id2");
            
            Assert.True(projects);
        }
        
        [Fact]
        public async Task ShouldGetAllProjectsOfUser()
        {
            var projects = await _service.GetUserProjects("id");
            
            Assert.Single(projects);
        }
        
        [Fact]
        public async Task ShouldGetAllDocumentationProjectsOfUser()
        {
            var projects = await _service.GetDocumentationProjects("id");
            
            Assert.Equal(2, projects.Length);
        }
        
        [Fact]
        public async Task ShouldThrowOnUnknownProject()
        {
            await Assert.ThrowsAsync<ProjectNotFoundException>(() => _service.GetDocumentHeader("proj-id0", "doc-id"));
        }
        
        [Fact]
        public async Task ShouldThrowOnUnknownDocument()
        {
            await Assert.ThrowsAsync<DocumentNotFoundException>(() => _service.GetDocumentHeader("proj-id", "doc-id0"));
        }
        
        [Fact]
        public async Task ShouldReturnDisplayable()
        {
            var result = await _service.GetDocumentHeader("proj-id", "doc-id");
            
            Assert.Equal("doc-id", result.Id);
            Assert.Equal("Document", result.Name);
            Assert.Equal("Test.I18N", result.I18NKey);
        }
        
        [Fact]
        public async Task ShouldReturnDisplayables()
        {
            var result = await _service.GetDisplayablesInProject("proj-id");
            
            Assert.Equal(3, result.Count());
        }
        
        [Fact]
        public async Task ShouldReturnChildrenDisplayables()
        {
            var result = await _service.GetDisplayablesInDocumentationEvent("proj-id", "doc-event-id");
            
            Assert.Single(result);
        }
        
        [Fact]
        public async Task ShouldThrowOnUnknownChildDocument()
        {
            await Assert.ThrowsAsync<DocumentNotFoundException>(async () =>
                (await _service.GetDisplayablesInDocumentationEvent("proj-id", "doc-event-id2")).ToList());
        }
        
        [Fact]
        public async Task ShouldThrowOnUnknownDocumentationEvent()
        {
            await Assert.ThrowsAsync<DocumentationEventNotFoundException>(async () =>
                (await _service.GetDisplayablesInDocumentationEvent("proj-id", "doc-event-id0")).ToList());
        }
        
        [Fact]
        public async Task ShouldReturnEmptyEnumerableOnDocumentRequest()
        {
            var result = await _service.GetDisplayablesInDocumentationEvent("proj-id", "doc-id");
            
            Assert.Empty(result);
        }
        
        [Fact]
        public void ShouldThrowOnNullParameter()
        {
            Assert.Throws<NullReferenceException>(() => _service.GetDocumentFormat("proj-id"));
        }
        
        [Fact]
        public void ShouldReturnDocumentOnDocumentId()
        {
             var result = _service.GetDocumentFormat("proj-id", "doc-id");
             Assert.Equal("doc-id", result.Id);
             Assert.Equal("Document", result.Name);
        }
        
        [Fact]
        public void FormatShouldThrowOnUnknownDocumentationEvent()
        {
            Assert.Throws<DocumentationEventNotFoundException>(() => _service.GetDocumentFormat("proj-id", "doc-event-id0"));
        }
        [Fact]
        public void FormatShouldThrowOnUnknownDocument()
        {
            Assert.Throws<DocumentNotFoundException>(() => _service.GetDocumentFormat("proj-id", "doc-event-id"));
        }
        
        [Fact]
        public void FormatShouldReturnDocumentInsteadOfDocumentationEvent()
        {
            var result = _service.GetDocumentFormat("proj-id", "doc-event-id2");
            Assert.Equal("doc-id", result.Id);
            Assert.Equal("Document", result.Name);
        }
        
        [Fact]
        public async Task ShouldReturnDataDefinitions()
        {
            var result = await _service.GetDataDefinitionsForUser("id");
            Assert.Equal(2, result.Length);
        }
    }
}