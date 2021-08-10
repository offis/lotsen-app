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
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace LotsenApp.Client.DataFormat.Database.Test
{
    [ExcludeFromCodeCoverage]
    public abstract class DatabaseDataFormatStorageTest : DataFormatContextTest
    {
        private readonly Mock<IDataProtectionProvider> _protectionMock;
        protected DatabaseDataFormatStorageTest(DbContextOptions<DataFormatContext> options) : 
            base(options)
        {
            _protectionMock = new Mock<IDataProtectionProvider>();
            SetupProtectionMock();
        }

        private void SetupProtectionMock()
        {
            var protectorMock = new Mock<IDataProtector>();
            SetupProtectorMock(protectorMock);
            _protectionMock.Setup(p =>
                    p.CreateProtector(It.IsAny<string>()))
                .Returns((IDataProtector)null);
        }

        private void SetupProtectorMock(Mock<IDataProtector> mock)
        {
            mock.Setup(p => p.Unprotect(It.IsAny<byte[]>()))
                .Returns((byte[] i) => i);
        }

        [Fact]
        public async Task ShouldCreateProject()
        {
            await using var context = new DataFormatContext(ContextOptions);
            var storage = new DatabaseDataFormatStorage(context, _protectionMock.Object);
            var project = new Project
            {
                Id = "proj2"
            };
            var result = await storage.CreateProject(project);
            Assert.True(result);
            Assert.Equal(2, storage.Projects.Length);
        }
        
        [Fact]
        public async Task ShouldUpdateProject()
        {
            await using var context = new DataFormatContext(ContextOptions);
            var storage = new DatabaseDataFormatStorage(context, _protectionMock.Object);
            var project = new Project
            {
                Id = "proj-id",
                Version = 2
            };
            var result = await storage.CreateProject(project);
            Assert.True(result);
            Assert.Single(storage.Projects);
        }
        
        [Fact]
        public async Task ShouldNotUpdateProject()
        {
            await using var context = new DataFormatContext(ContextOptions);
            var storage = new DatabaseDataFormatStorage(context, _protectionMock.Object);
            var project = new Project
            {
                Id = "proj-id",
                Version = 0
            };
            var result = await storage.CreateProject(project);
            Assert.False(result);
            Assert.Single(storage.Projects);
        }
        
        [Fact]
        public async Task ShouldAddI18N()
        {
            await using var context = new DataFormatContext(ContextOptions);
            var storage = new DatabaseDataFormatStorage(context, _protectionMock.Object);
            var project = new Project
            {
                Id = "proj-id",
                Version = 0
            };
            await storage.AddI18N(project, "fr", "French locale", false);
            var locales = await storage.GetLocalesForProject("proj-id");
            Assert.Equal(2, locales.Length);
        }
        
        [Fact]
        public async Task ShouldKeepI18N()
        {
            await using var context = new DataFormatContext(ContextOptions);
            var storage = new DatabaseDataFormatStorage(context, _protectionMock.Object);
            var project = new Project
            {
                Id = "proj-id",
                Version = 0
            };
            await storage.AddI18N(project, "de", "Another locale", false);
            var localeValues = await storage.GetProjectI18N("de");
            Assert.Single(localeValues);
            Assert.NotEqual("Another locale", localeValues[0]);
        }
        
        [Fact]
        public async Task ShouldReplaceI18N()
        {
            await using var context = new DataFormatContext(ContextOptions);
            var storage = new DatabaseDataFormatStorage(context, _protectionMock.Object);
            var project = new Project
            {
                Id = "proj-id",
                Version = 0
            };
            await storage.AddI18N(project, "de", "Another locale", true);
            var localeValues = await storage.GetProjectI18N("de");
            Assert.Single(localeValues);
            Assert.Equal("Another locale", localeValues[0]);
        }
        
        [Fact]
        public async Task ShouldGetAssociations()
        {
            await using var context = new DataFormatContext(ContextOptions);
            var storage = new DatabaseDataFormatStorage(context, _protectionMock.Object);
            Assert.Single(storage.Associations);
        }
    }
}