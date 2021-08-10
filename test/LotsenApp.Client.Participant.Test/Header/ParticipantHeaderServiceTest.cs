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
using System.Text;
using LotsenApp.Client.File;
using LotsenApp.Client.Participant.Header;
using LotsenApp.Client.Participant.Model;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Document = LotsenApp.Client.Participant.Model.Document;

namespace LotsenApp.Client.Participant.Test.Header
{
    [ExcludeFromCodeCoverage]
    public class ParticipantHeaderServiceTest
    {
        private readonly IFileService _fileService = new ScopedFileService
        {
            Root = $"./{Guid.NewGuid().ToString()}"
        };
        private ParticipantHeaderService CreateInstance()
        {
            var protectionProviderMock = new Mock<IDataProtectionProvider>();
            var protectorMock = new Mock<IDataProtector>();
            protectorMock
                .Setup(p => p.Protect(It.IsAny<byte[]>()))
                .Returns((byte[] b) => b);
            protectorMock
                .Setup(p => p.Unprotect(It.IsAny<byte[]>()))
                .Returns((byte[] b) => b);
            protectionProviderMock.Setup(p => p.CreateProtector(It.IsAny<string>()))
                .Returns(protectorMock.Object);
            return new ParticipantHeaderService(_fileService, protectionProviderMock.Object, new DocumentDeltaHelper());
        }

        [Fact]
        public void ShouldAddNewEntryToParticipantHeader()
        {
            var service = CreateInstance();
            service.AddToParticipantHeader("usr", "par", "val");
            var target = _fileService.Join($"data/usr/{IParticipantHeaderService.FileName}");
            var codedContent = WebEncoders.Base64UrlDecode(System.IO.File.ReadAllText(target));
            var decodedContent = Encoding.UTF8.GetString(codedContent);
            var targetDefinition =
                JsonConvert.DeserializeObject<HeaderDefinitionFile>(decodedContent);
            Assert.Equal("val", targetDefinition?.ParticipantHeader["par"].First());
        }
        
        [Fact]
        public void ShouldAddNoNewEntryWithDuplicateToParticipantHeader()
        {
            var service = CreateInstance();
            service.AddToParticipantHeader("usr", "par", "val");
            service.AddToParticipantHeader("usr", "par", "val");
            var target = _fileService.Join($"data/usr/{IParticipantHeaderService.FileName}");
            var codedContent = WebEncoders.Base64UrlDecode(System.IO.File.ReadAllText(target));
            var decodedContent = Encoding.UTF8.GetString(codedContent);
            var targetDefinition =
                JsonConvert.DeserializeObject<HeaderDefinitionFile>(decodedContent);
            Assert.Single(targetDefinition?.ParticipantHeader["par"] ?? new List<string>());
        }
        
        [Fact]
        public void ShouldRemoveEntryFromParticipantHeader()
        {
            var service = CreateInstance();
            service.AddToParticipantHeader("usr", "par", "val");
            service.RemoveFromParticipantHeader("usr", "par", "val");
            var target = _fileService.Join($"data/usr/{IParticipantHeaderService.FileName}");
            var codedContent = WebEncoders.Base64UrlDecode(System.IO.File.ReadAllText(target));
            var decodedContent = Encoding.UTF8.GetString(codedContent);
            var targetDefinition =
                JsonConvert.DeserializeObject<HeaderDefinitionFile>(decodedContent);
            Assert.Empty(targetDefinition?.ParticipantHeader["par"] ?? new List<string>());
        }
        
        [Fact]
        public void ShouldNotCrashOnInvalidEntryRemovalFromParticipantHeader()
        {
            var service = CreateInstance();
            service.RemoveFromParticipantHeader("usr", "par", "val");
            var target = _fileService.Join($"data/usr/{IParticipantHeaderService.FileName}");
            var codedContent = WebEncoders.Base64UrlDecode(System.IO.File.ReadAllText(target));
            var decodedContent = Encoding.UTF8.GetString(codedContent);
            var targetDefinition =
                JsonConvert.DeserializeObject<HeaderDefinitionFile>(decodedContent);
            Assert.Empty(targetDefinition?.ParticipantHeader ?? new Dictionary<string, List<string>>());
        }
        
        [Fact]
        public void ShouldAddNewEntryToProjectHeader()
        {
            var service = CreateInstance();
            service.AddToProjectHeader("usr", "par", "val");
            var target = _fileService.Join($"data/usr/{IParticipantHeaderService.FileName}");
            var codedContent = WebEncoders.Base64UrlDecode(System.IO.File.ReadAllText(target));
            var decodedContent = Encoding.UTF8.GetString(codedContent);
            var targetDefinition =
                JsonConvert.DeserializeObject<HeaderDefinitionFile>(decodedContent);
            Assert.Equal("val", targetDefinition?.ProjectHeader["par"].First());
        }
        
        [Fact]
        public void ShouldAddNoNewEntryWithDuplicateToProjectHeader()
        {
            var service = CreateInstance();
            service.AddToProjectHeader("usr", "par", "val");
            service.AddToProjectHeader("usr", "par", "val");
            var target = _fileService.Join($"data/usr/{IParticipantHeaderService.FileName}");
            var codedContent = WebEncoders.Base64UrlDecode(System.IO.File.ReadAllText(target));
            var decodedContent = Encoding.UTF8.GetString(codedContent);
            var targetDefinition =
                JsonConvert.DeserializeObject<HeaderDefinitionFile>(decodedContent);
            Assert.Single(targetDefinition?.ProjectHeader["par"] ?? new List<string>());
        }

        [Fact]
        public void ShouldRemoveEntryFromProjectHeader()
        {
            var service = CreateInstance();
            service.AddToProjectHeader("usr", "par", "val");
            service.RemoveFromProjectHeader("usr", "par", "val");
            var target = _fileService.Join($"data/usr/{IParticipantHeaderService.FileName}");
            var codedContent = WebEncoders.Base64UrlDecode(System.IO.File.ReadAllText(target));
            var decodedContent = Encoding.UTF8.GetString(codedContent);
            var targetDefinition =
                JsonConvert.DeserializeObject<HeaderDefinitionFile>(decodedContent);
            Assert.Empty(targetDefinition?.ProjectHeader["par"] ?? new List<string>());
        }
        
        [Fact]
        public void ShouldNotCrashOnInvalidEntryRemovalFromProjectHeader()
        {
            var service = CreateInstance();
            service.RemoveFromProjectHeader("usr", "par", "val");
            var target = _fileService.Join($"data/usr/{IParticipantHeaderService.FileName}");
            var codedContent = WebEncoders.Base64UrlDecode(System.IO.File.ReadAllText(target));
            var decodedContent = Encoding.UTF8.GetString(codedContent);
            var targetDefinition =
                JsonConvert.DeserializeObject<HeaderDefinitionFile>(decodedContent);
            Assert.Empty(targetDefinition?.ProjectHeader ?? new Dictionary<string, List<string>>());
        }

        [Fact]
        public void ShouldCalculateHeader()
        {
            var service = CreateInstance();
            
            service.AddToParticipantHeader("usr", "par", "doc.val1");
            service.AddToProjectHeader("usr", "prj", "dc.val2");

            var model = new ParticipantModel
            {
                Id = "par",
                Additional = new Dictionary<string, string> {{IParticipantTransformationService.DocumentedBy, "prj"}},
                Data = new DataBody
                {
                    Documents = new Dictionary<string, Document>
                    {
                        {
                            "doc", new Document
                            {
                                Id = "doc",
                                DocumentId = "dc",
                                Values = new Dictionary<string, DocumentField>
                                {
                                    {
                                        "val1", new DocumentField
                                        {
                                            Id = "val1",
                                            Value = "value1"
                                        }
                                    },
                                    {
                                        "val2", new DocumentField
                                        {
                                            Id = "val2",
                                            Value = "value2"
                                        }
                                    }
                                }
                            }
                        },
                        {
                            "doc2", new Document
                            {
                                Id = "doc2",
                                DocumentId = "dc",
                                Values = new Dictionary<string, DocumentField>
                                {
                                    {
                                        "val1", new DocumentField
                                        {
                                            Id = "val1",
                                            Value = "value1"
                                        }
                                    },
                                    {
                                        "val2", new DocumentField
                                        {
                                            Id = "val2",
                                            Value = "value3"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var updatedModel = service.CalculateHeader("usr", model);
            Assert.Equal(2, updatedModel.Header.Count);
            Assert.Equal("value1", updatedModel.Header["val1"].First());
            Assert.Equal(2, updatedModel.Header["val2"].Count);
            Assert.Equal("value2", updatedModel.Header["val2"].First());
        }
        
        [Fact]
        public void ShouldNotAddDuplicateValue()
        {
            var service = CreateInstance();
            
            service.AddToParticipantHeader("usr", "par", "doc.val1");
            service.AddToParticipantHeader("usr", "par", "doc2.val1");
            service.AddToProjectHeader("usr", "prj", "dc.val1");

            var model = new ParticipantModel
            {
                Id = "par",
                Additional = new Dictionary<string, string> {{IParticipantTransformationService.DocumentedBy, "prj"}},
                Data = new DataBody
                {
                    Documents = new Dictionary<string, Document>
                    {
                        {
                            "doc", new Document
                            {
                                Id = "doc",
                                DocumentId = "dc",
                                Values = new Dictionary<string, DocumentField>
                                {
                                    {
                                        "val1", new DocumentField
                                        {
                                            Id = "val1",
                                            Value = "value1"
                                        }
                                    },
                                    {
                                        "val2", new DocumentField
                                        {
                                            Id = "val2",
                                            Value = "value2"
                                        }
                                    }
                                }
                            }
                        },
                        {"doc2", new Document
                        {
                            Id = "doc2",
                            Values = new Dictionary<string, DocumentField>
                            {
                                {
                                    "val1", new DocumentField
                                    {
                                        Id = "val1",
                                        Value = "value1"
                                    }
                                }
                            }
                        }}
                    }
                }
            };

            var updatedModel = service.CalculateHeader("usr", model);
            Assert.Single(updatedModel.Header);
            Assert.Equal("value1", updatedModel.Header["val1"].First());
        }

        
    }
}