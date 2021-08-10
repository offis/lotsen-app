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
using LotsenApp.Client.DataFormat.Access;
using Xunit;

namespace LotsenApp.Client.DataFormat.Test.Access
{
    [ExcludeFromCodeCoverage]
    public class DataDefinitionDtoTest
    {
        [Fact]
        public void ShouldSetProjectId()
        {
            var dto = new DataDefinitionDto
            {
                ProjectId = "prj-id"
            };
            
            Assert.Equal("prj-id", dto.ProjectId);
        }
        
        [Fact]
        public void ShouldSetName()
        {
            var dto = new DataDefinitionDto
            {
                Name = "Project"
            };
            
            Assert.Equal("Project", dto.Name);
        }
        
        [Fact]
        public void ShouldSetI18NKey()
        {
            var dto = new DataDefinitionDto
            {
                I18NKey = "Test"
            };
            
            Assert.Equal("Test", dto.I18NKey);
        }
        
        [Fact]
        public void ShouldSetVersion()
        {
            var dto = new DataDefinitionDto
            {
                Version = 2
            };
            
            Assert.Equal(2, dto.Version);
        }
        
        [Fact]
        public void ShouldSetLocales()
        {
            var dto = new DataDefinitionDto
            {
                Locales = new []{"de"}
            };
            
            Assert.Equal(new[] {"de"}, dto.Locales);
        }
        
        [Fact]
        public void ShouldSetIsParticipant()
        {
            var dto = new DataDefinitionDto
            {
                IsParticipant = true
            };
            
            Assert.True(dto.IsParticipant);
        }
    }
}