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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;

namespace LotsenApp.Client.DataFormat.Database.Test
{
    [ExcludeFromCodeCoverage]
    public abstract class DataFormatContextTest
    {
        protected DbContextOptions<DataFormatContext> ContextOptions { get; }

        protected DataFormatContextTest(DbContextOptions<DataFormatContext> contextOptions)
        {
            ContextOptions = contextOptions;
            
            Seed();
        }
        [Fact]
        public void ShouldReturnProjects()
        {
            using var context = new DataFormatContext(ContextOptions);

            Assert.Single(context.Projects);
        }
        
        [Fact]
        public void ShouldReturnI18N()
        {
            using var context = new DataFormatContext(ContextOptions);

            Assert.Single(context.I18N);
        }
        
        [Fact]
        public void ShouldReturnAssociation()
        {
            using var context = new DataFormatContext(ContextOptions);

            Assert.Single(context.Association);
        }

        private void Seed()
        {
            using var context = new DataFormatContext(ContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var project = new Project
            {
                Id = "proj-id",
                Version = 1
            };

            var associations = new Dictionary<string, string[]>
            {
                {"proj-id", new[] {"usr-id"}}
            };

            var projectEntry = new ProjectEntry
            {
                ProjectId = "proj-id",
                EncryptedProjectDefinition = JsonConvert.SerializeObject(project)
            };
            var i18N = new I18NEntry
            {
                ProjectId = "proj-id",
                Locale = "de",
                I18NValues = "I18NValues"
            };
            var userProject = new UserProject
            {
                AssociationId = "1",
                Association = JsonConvert.SerializeObject(associations)
            };
            
            context.AddRange(projectEntry, i18N, userProject);

            context.SaveChanges();
        }
    }
}