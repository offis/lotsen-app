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
using System.Text;
using System.Threading.Tasks;
using LotsenApp.Client.File;
using LotsenApp.Client.Plugin;
using Microsoft.AspNetCore.DataProtection;
using Moq;
using Xunit;

namespace LotsenApp.Client.DataFormat.File.Test
{
    [ExcludeFromCodeCoverage]
    public class FileDataFormatStorageTest
    {
        private readonly IFileService _fileService;
        private readonly Mock<IDataProtectionProvider> _protectionMock = new Mock<IDataProtectionProvider>();
        public FileDataFormatStorageTest()
        {
            ConcurrentFileAccessHelper.ReleaseAllLocks();
            _fileService = new ScopedFileService {Root = Guid.NewGuid() + "/"};
            _fileService.EnsureCreated();
        }
        
        [Fact]
        public async Task ShouldCreateNewProject()
        {
            var storage = new FileDataFormatStorage(_protectionMock.Object, _fileService);

            var testProject = new Project
            {
                Id = Guid.NewGuid().ToString(),
                Version = 1
            };

            var result = await storage.CreateProject(testProject);
            var targetFile = _fileService.Join($"config/data-definition/{testProject.Id}.json");
            
            Assert.True(result);
            Assert.True(System.IO.File.Exists(targetFile));
        }
        
        [Fact]
        public async Task ShouldCreateAndGetNewProject()
        {
            var storage = new FileDataFormatStorage(_protectionMock.Object, _fileService);

            var testProject = new Project
            {
                Id = Guid.NewGuid().ToString(),
                Version = 1
            };

            var result = await storage.CreateProject(testProject);
            var targetFile = _fileService.Join($"config/data-definition/{testProject.Id}.json");

            var projects = storage.Projects;
            
            Assert.True(result);
            Assert.True(System.IO.File.Exists(targetFile));
            Assert.Single(projects);
        }
        
        [Fact]
        public async Task ShouldOverwriteWithNewerProjectVersion()
        {
            var storage = new FileDataFormatStorage(_protectionMock.Object, _fileService);

            var testProject = new Project
            {
                Id = Guid.NewGuid().ToString(),
                Version = 1
            };

            await storage.CreateProject(testProject);
            testProject.Version = 2;
            var result = await storage.CreateProject(testProject);
            var targetFile = _fileService.Join($"config/data-definition/{testProject.Id}.json");
            
            Assert.True(result);
            Assert.True(System.IO.File.Exists(targetFile));
        }
        
        [Fact]
        public async Task ShouldDiscardProjectWithSameVersion()
        {
            var storage = new FileDataFormatStorage(_protectionMock.Object, _fileService);

            var testProject = new Project
            {
                Id = Guid.NewGuid().ToString(),
                Version = 1
            };

            await storage.CreateProject(testProject);
            var result = await storage.CreateProject(testProject);
            var targetFile = _fileService.Join($"config/data-definition/{testProject.Id}.json");
            
            Assert.False(result);
            Assert.True(System.IO.File.Exists(targetFile));
        }
        
        [Fact]
        public async Task ShouldAddNewI18N()
        {
            var storage = new FileDataFormatStorage(_protectionMock.Object, _fileService);

            var testProject = new Project
            {
                Id = Guid.NewGuid().ToString(),
                Version = 1
            };

            await storage.AddI18N(testProject, "de", "TestString", false);
            
            var targetFile = _fileService.Join($"config/data-definition/i18n/{testProject.Id}_de.json");
            
            Assert.True(System.IO.File.Exists(targetFile));
        }
        
        [Fact]
        public async Task ShouldGetAllI18N()
        {
            var storage = new FileDataFormatStorage(_protectionMock.Object, _fileService);

            var testProject = new Project
            {
                Id = Guid.NewGuid().ToString(),
                Version = 1
            };

            await storage.AddI18N(testProject, "de", "TestString", false);
            testProject.Id = Guid.NewGuid().ToString();
            await storage.AddI18N(testProject, "de", "TestString", false);
            testProject.Id = Guid.NewGuid().ToString();
            await storage.AddI18N(testProject, "de", "TestString", false);

            var i18N = await storage.GetProjectI18N("de");
            
            Assert.Equal(3, i18N.Length);
        }
        
        [Fact]
        public async Task ShouldGetAllLocalesForProject()
        {
            var storage = new FileDataFormatStorage(_protectionMock.Object, _fileService);

            var testProject = new Project
            {
                Id = Guid.NewGuid().ToString(),
                Version = 1
            };

            await storage.AddI18N(testProject, "de", "TestString", false);
            await storage.AddI18N(testProject, "en", "TestString", false);
            await storage.AddI18N(testProject, "fr", "TestString", false);

            var i18N = await storage.GetLocalesForProject(testProject.Id);
            
            Assert.Equal(3, i18N.Length);
        }
        
        [Fact]
        public void ShouldGetEmptyAssociationsWithNoFile()
        {
            var storage = new FileDataFormatStorage(_protectionMock.Object, _fileService);

            var associations = storage.Associations;
            
            Assert.Empty(associations);
        }
        
        [Fact]
        public void ShouldGetAssociationsWithFile()
        {
            var associationFile = _fileService.Join("config/data-definition/association.crypt");
            var protectorMock = new Mock<IDataProtector>();
            const string result = "{\"id\": [\"uid1\", \"uid2\"]}";
            var bytes = Encoding.UTF8.GetBytes(result);
            protectorMock.Setup(pro => pro.Unprotect(It.IsAny<byte[]>()))
                .Returns(bytes);
            _protectionMock.Setup(pro => pro.CreateProtector(It.IsAny<string>()))
                .Returns(protectorMock.Object);
            var storage = new FileDataFormatStorage(_protectionMock.Object, _fileService);
            System.IO.File.WriteAllText(associationFile, "SomeText");

            var associations = storage.Associations;

            Assert.Single(associations);
            Assert.Equal(2, associations["id"].Length);
        }
        
        
    }
}