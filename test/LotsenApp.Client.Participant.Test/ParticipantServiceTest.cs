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
using System.Linq;
using System.Threading.Tasks;
using LotsenApp.Client.Authentication.Api;
using LotsenApp.Client.Authentication.DataPassword;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.Participant.Delta;
using LotsenApp.Client.Participant.Dto;
using LotsenApp.Client.Participant.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LotsenApp.Client.Participant.Test
{
    [ExcludeFromCodeCoverage]
    public class ParticipantServiceTest
    {
        #region MockDefinition

        private static IParticipantService CreateInstance()
        {
            var storageMock = new Mock<IParticipantStorage>();
            SetupStorageMock(storageMock);
            var transformationMock = new Mock<IParticipantTransformationService>();
            SetupTransformationMock(transformationMock);
            var deltaServiceMock = new Mock<IDeltaService>();
            SetupDeltaServiceMock(deltaServiceMock);
            var mergeServiceMock = new Mock<IMergeService>();
            SetupMergeServiceMock(mergeServiceMock);
            var loggerMock = new Mock<ILogger<ParticipantServiceImpl>>();

            return new ParticipantServiceImpl(
                storageMock.Object,
                transformationMock.Object,
                deltaServiceMock.Object,
                mergeServiceMock.Object,
                loggerMock.Object);
        }

        private static void SetupStorageMock(Mock<IParticipantStorage> mock)
        {
            mock.Setup(sto =>
                sto.GetParticipants("id", false)).Returns(new[]
            {
                Model
            });
            mock.Setup(sto =>
                sto.GetLatestSaveState("id", It.IsAny<string>())).Returns(Model);
            mock.Setup(sto =>
                sto.GetLatestSaveState("id2", It.IsAny<string>())).Returns((EncryptedParticipantModel) null);
            mock.Setup(sto =>
                sto.GetDelta("id", It.IsAny<string>(), AccessMode.Read)).Returns(Delta);
        }

        private static void SetupTransformationMock(Mock<IParticipantTransformationService> mock)
        {
            mock.Setup(tra => tra.CreateHeaderDto("id", It.IsAny<EncryptedParticipantModel>()).Result).Returns(
                new ParticipantHeaderDto
                {
                    Id = "part-id"
                });
            mock.Setup(tra =>
                    tra.GetDocuments("id", It.IsAny<EncryptedParticipantModel>(), It.IsAny<EncryptedDeltaFile>())
                        .Result)
                .Returns(new ParticipantDocumentDto
                {
                    ParticipantId = "part-id",
                });
            mock.Setup(tra =>
                    tra.GetDocument("id", It.IsAny<string>(), It.IsAny<EncryptedParticipantModel>(), It.IsAny<EncryptedDeltaFile>())
                        .Result)
                .Returns(new SpecificDocumentDto
                {
                    Id = "doc-id",
                    DocumentId = "document-id",
                    Name = "name"
                });
            mock.Setup(tra =>
                    tra.CreateValueDto("id", It.IsAny<EncryptedParticipantModel>(), It.IsAny<EncryptedDeltaFile>(),
                        It.IsAny<string>()).Result)
                .Returns(new DocumentValueDto
                {
                    Id = "doc-id",
                });
            mock.Setup(tra => tra.CreateEncryptedModel(It.IsAny<CreateParticipantDto>(), "id").Result)
                .Returns(new EncryptedParticipantModel {Id = "part-id"});
        }

        private static void SetupDeltaServiceMock(Mock<IDeltaService> mock)
        {
            mock.Setup(del =>
                    del.SeedDeltaFile("id", It.IsAny<EncryptedParticipantModel>(), It.IsAny<EncryptedDeltaFile>())
                        .Result)
                .Returns(Delta);
            mock.Setup(del =>
                    del.AddDocument("id", It.IsAny<EncryptedDeltaFile>(), It.IsAny<CreateDocumentDto>()).Result)
                .Returns((new EncryptedDeltaFile(), "doc-id"));
            mock.Setup(del =>
                    del.UpdateDocument("id", It.IsAny<EncryptedParticipantModel>(), It.IsAny<EncryptedDeltaFile>(),
                        It.IsAny<UpdateDocumentDto>()).Result)
                .Returns(new EncryptedDeltaFile());
            mock.Setup(del =>
                    del.AddGroup("id", It.IsAny<EncryptedDeltaFile>(), It.IsAny<CreateGroupDto>()).Result)
                .Returns((Delta, "grp-id"));
        }

        private static void SetupMergeServiceMock(Mock<IMergeService> mock)
        {
            mock.Setup(mer =>
                mer.MergeModelWithDelta("id", It.IsAny<EncryptedParticipantModel>(), It.IsAny<EncryptedDeltaFile>())
                    .Result).Returns(Model);
        }

        private static EncryptedParticipantModel Model => new EncryptedParticipantModel();
        private static EncryptedDeltaFile Delta => new EncryptedDeltaFile();

        #endregion

        [Fact]
        public async Task ShouldReturnListOfHeader()
        {
            var controller = CreateInstance();

            var result = await controller.GetParticipantOverview("id");

            Assert.Single(result);

            var dto = result.First();

            Assert.Equal("part-id", dto.Id);
        }

        [Fact]
        public async Task ShouldReturnSpecificHeader()
        {
            var controller = CreateInstance();

            var result = await controller.GetSpecificParticipantInfo("id", "part-id");

            Assert.Equal("part-id", result.Id);
        }

        [Fact]
        public async Task ShouldExecuteMerge()
        {
            var controller = CreateInstance();

            await controller.SaveChanges("id", "part-id");
        }

        [Fact]
        public async Task ShouldGetDocuments()
        {
            var controller = CreateInstance();

            var result = await controller.GetDocumentsForUser("id", "part-id");

            Assert.Equal("part-id", result.ParticipantId);
        }
        
        [Fact]
        public async Task ShouldGetDocument()
        {
            var controller = CreateInstance();

            var result = await controller.GetDocumentForUser("id", "part-id", "doc-id");

            Assert.Equal("doc-id", result.Id);
            Assert.Equal("document-id", result.DocumentId);
            Assert.Equal("name", result.Name);
            Assert.False(result.IsDelta);
        }

        [Fact]
        public async Task ShouldCreateDocument()
        {
            var controller = CreateInstance();

            var result = await controller.CreateDocument("id", "part-id", new CreateDocumentDto());

            Assert.Equal("doc-id", result.Id);
        }

        [Fact]
        public async Task ShouldUpdateDocument()
        {
            var controller = CreateInstance();

            await controller.UpdateDocument("id", "part-id", new UpdateDocumentDto());
        }

        [Fact]
        public async Task ShouldDoNothingOnNullSave()
        {
            var controller = CreateInstance();

            await controller.UpdateDocument("id2", "part-id", new UpdateDocumentDto());
        }

        [Fact]
        public async Task ShouldReorderDocuments()
        {
            var controller = CreateInstance();

            await controller.ReorderDocuments("id2", "part-id", Array.Empty<ReOrderDto>());
        }
        
        [Fact]
        public async Task ShouldGetDocumentValues()
        {
            var controller = CreateInstance();

            var dto = await controller.GetDocumentValues("id", "part-id", "doc-id");
            
            Assert.Equal("doc-id", dto.Id);
        }
        
        [Fact]
        public async Task ShouldDeleteDocument()
        {
            var controller = CreateInstance();

            var dto = await controller.DeleteDocument("id", "part-id", "doc-id");
            
            Assert.Equal("doc-id", dto.Id);
        }
        
        [Fact]
        public async Task ShouldCreateGroup()
        {
            var controller = CreateInstance();

            var dto = await controller.CreateGroup("id", "part-id", new CreateGroupDto());
            
            Assert.Equal("grp-id", dto.Id);
        }
        
        [Fact]
        public async Task ShouldDeleteGroup()
        {
            var controller = CreateInstance();

            var dto = await controller.DeleteGroup("id", "part-id", "doc-id", "grp-id");
            
            Assert.Equal("grp-id", dto.Id);
        }
        
        [Fact]
        public async Task ShouldReorderGroups()
        {
            var controller = CreateInstance();

            await controller.ReorderGroup("id", "part-id", "doc-id", Array.Empty<ReOrderDto>());
            
        }
        
        [Fact]
        public async Task ShouldCreateParticipant()
        {
            var controller = CreateInstance();

            var result = await controller.CreateParticipant("id", new CreateParticipantDto());
            
            Assert.Equal("part-id", result.Id);
        }
        
        [Fact]
        public async Task ShouldDeleteParticipant()
        {
            var controller = CreateInstance();

            var result = await controller.DeleteParticipant("id", "part-id");
            
            Assert.Equal("part-id", result.Id);
        }
    }
}