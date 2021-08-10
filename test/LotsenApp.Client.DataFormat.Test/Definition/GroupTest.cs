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
using LotsenApp.Client.DataFormat.Definition;
using Xunit;

namespace LotsenApp.Client.DataFormat.Test.Definition
{
    [ExcludeFromCodeCoverage]
    public class GroupTest
    {
        [Fact]
        public void ShouldCopyInitialGroup()
        {
            var initialGroup = new Group
            {
                Id = "grp-id",
                Name = "Group",
                Cardinality = Cardinality.Many,
                Children = new(),
                Fields = new()
            };
            var copyGroup = new Group(initialGroup);

            Assert.Equal(initialGroup.Id, copyGroup.Id);
            Assert.Equal(initialGroup.Name, copyGroup.Name);
            Assert.Equal(initialGroup.Cardinality, copyGroup.Cardinality);
            Assert.Same(initialGroup.Children, copyGroup.Children);
            Assert.Same(initialGroup.Fields, copyGroup.Fields);
        }

        [Fact]
        public void ShouldCopyInitialDataField()
        {
            var initialField = new DataField()
            {
                Id = "fld-id",
                Name = "Field",
                Expression = "exp",
                DataType = "data-type"
            };
            var copyField = new DataField(initialField);

            Assert.Equal(initialField.Id, copyField.Id);
            Assert.Equal(initialField.Name, copyField.Name);
            Assert.Equal(initialField.Expression, copyField.Expression);
            Assert.Equal(initialField.DataType, copyField.DataType);
        }

        [Fact]
        public void ShouldCopyInitialType()
        {
            var initialType = new DataType
            {
                Id = "typ-id",
                Name = "Type",
                Type = Types.DATE,
                Values = "values"
            };
            var copyType = new DataType(initialType);

            Assert.Equal(initialType.Id, copyType.Id);
            Assert.Equal(initialType.Name, copyType.Name);
            Assert.Equal(initialType.Type, copyType.Type);
            Assert.Equal(initialType.Values, copyType.Values);
        }

        [Fact]
        public void ShouldCopyInitialDocument()
        {
            var initialDocument = new Document
            {
                Id = "dcm-id",
                Name = "Document",
                Groups = new(),
                DataFields = new(),
                DocumentType = DocumentType.Custom
            };
            var copyDocument = new Document(initialDocument);

            Assert.Equal(initialDocument.Id, copyDocument.Id);
            Assert.Equal(initialDocument.Name, copyDocument.Name);
            Assert.Same(initialDocument.Groups, copyDocument.Groups);
            Assert.Same(initialDocument.DataFields, copyDocument.DataFields);
            Assert.Equal(initialDocument.DocumentType, copyDocument.DocumentType);
        }

        [Fact]
        public void ShouldSetValuesForDataDefinition()
        {
            var definition = new DataDefinition
            {
                Groups = new(),
                DataFields = new(),
                DataTypes = new(),
            };

            Assert.Empty(definition.Groups);
            Assert.Empty(definition.DataFields);
            Assert.Empty(definition.DataTypes);
        }
    }
}