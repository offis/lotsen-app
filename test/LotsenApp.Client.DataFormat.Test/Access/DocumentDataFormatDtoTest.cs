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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LotsenApp.Client.DataFormat.Access;
using LotsenApp.Client.DataFormat.Definition;
using LotsenApp.Client.DataFormat.Display;
using Xunit;

namespace LotsenApp.Client.DataFormat.Test.Access
{
    [ExcludeFromCodeCoverage]
    public class DocumentDataFormatDtoTest
    {
        [Fact]
        public void ShouldSetValues()
        {
            var document = new Document
            {
                Id = "doc-id",
                Name = "Document",
                DocumentType = DocumentType.Custom,
                DataFields = new List<string>
                {
                    "dfd-id"
                },
                Groups = new List<string>
                {
                    "grp-id"
                }
            };
            var documentDisplay = new DocumentDisplay
            {
                DataFields = new List<DocumentDataFieldDisplay>
                {
                    new DocumentDataFieldDisplay
                    {
                        Id = "dfd-id",
                        Ordinal = 9
                    }
                },
                Groups = new List<DocumentGroupDisplay>
                {
                    new DocumentGroupDisplay
                    {
                        Id = "grp-id",
                        Ordinal = 9
                    }
                }
            };
            var project = new Project
            {
                DataDefinition = new DataDefinition
                {
                    DataFields = new List<DataField>
                    {
                        new DataField
                        {
                            Id = "dfd-id",
                            DataType = "dtp-id",
                            Expression = "exp",
                            Name = "Data Field"
                        }
                    },
                    Groups = new List<Group>
                    {
                        new Group
                        {
                            Id = "grp-id",
                            Cardinality = Cardinality.One,
                            Name = "Group",
                        }
                    },
                    DataTypes = new List<DataType>
                    {
                        new DataType
                        {
                            Id = "dtp-id",
                            Name = "Data Type",
                            Type = Types.CUSTOM,
                            Values = "values"
                        }
                    }
                },
                DataDisplay = new DataDisplay
                {
                    DataFields = new List<DataFieldDisplay>
                    {
                        new DataFieldDisplay
                        {
                            Id = "dfd-id",
                            Expression = "exp",
                            I18NKey = "Test"
                        }
                    },
                    Groups = new List<GroupDisplay>
                    {
                        new GroupDisplay
                        {
                            Id = "grp-id",
                            I18NKey = "Test",
                            Ordinal = 99,
                        }
                    },
                    DataTypes = new List<DataTypeDisplay>
                    {
                        new DataTypeDisplay
                        {
                            Id = "dtp-id",
                            Expression = "exp"
                        }
                    }
                }
            };
            var dto = new DocumentDataFormatDto(document, documentDisplay, project);
            
            Assert.Equal(document.Id, dto.Id);
            Assert.Equal(document.Name, dto.Name);
            Assert.Equal(document.DocumentType, dto.Type);
            Assert.Single(dto.Fields);
            Assert.Single(dto.Groups);
        }
    }
}