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
using System.Threading;
using LotsenApp.Client.File;
using LotsenApp.Client.Participant.Model;
using LotsenApp.Client.Plugin;
using Newtonsoft.Json;
using Xunit;

namespace LotsenApp.Client.Participant.Test
{
    [ExcludeFromCodeCoverage]
    public class PersistentParticipantStorageTest
    {
        private IFileService _fileService;
        public PersistentParticipantStorageTest()
        {
            ConcurrentFileAccessHelper.ReleaseAllLocks();
            _fileService = new ScopedFileService {Root = Guid.NewGuid() + "/"};
            _fileService.EnsureCreated();
        }
        
        [Fact]
        public void ShouldCreateNewParticipant()
        {
            var storage = new PersistentParticipantStorage(_fileService);

            var encryptedModel = new EncryptedParticipantModel
            {
                Id = "part-id"
            };
            storage.SaveData("id", encryptedModel);
            var result = storage.GetLatestSaveState("id", "part-id");
            var expected = JsonConvert.SerializeObject(encryptedModel);
            var actual = JsonConvert.SerializeObject(result);
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void ShouldReturnEmptyEnumerableWithNoSaveStates()
        {
            var storage = new PersistentParticipantStorage(_fileService);
            
            var result = storage.GetAllSaveStatesForParticipant("id", "part-id");
            
            Assert.Empty(result);
        }
        
        [Fact]
        public void ShouldReturnMultipleSaveStates()
        {
            var storage = new PersistentParticipantStorage(_fileService);
            
            var encryptedModel = new EncryptedParticipantModel
            {
                Id = "part-id"
            };
            storage.SaveData("id", encryptedModel);
            Thread.Sleep(1500);
            var encryptedModel2 = new EncryptedParticipantModel
            {
                Id = "part-id"
            };
            storage.SaveData("id", encryptedModel2);
            var result = storage.GetAllSaveStatesForParticipant("id", "part-id");
            
            Assert.Equal(2, result.Count());
        }
        
        [Fact]
        public void ShouldReturnEmptyArrayWithNoSaveStates()
        {
            var storage = new PersistentParticipantStorage(_fileService);
            
            var result = storage.GetParticipants("id");
            
            Assert.Empty(result);
        }
        
        [Fact]
        public void ShouldDeleteParticipant()
        {
            var storage = new PersistentParticipantStorage(_fileService);
            
            var encryptedModel = new EncryptedParticipantModel
            {
                Id = "part-id"
            };
            storage.SaveData("id", encryptedModel);
            storage.DeleteParticipant("id", "part-id");
            var result = storage.GetParticipants("id");
            Assert.Empty(result);
        }
        
        [Fact]
        public void ShouldIncludeDeletedParticipants()
        {
            var storage = new PersistentParticipantStorage(_fileService);
            
            var encryptedModel = new EncryptedParticipantModel
            {
                Id = "part-id"
            };
            storage.SaveData("id", encryptedModel);
            storage.DeleteParticipant("id", "part-id");
            var result = storage.GetParticipants("id", true);
            Assert.Single(result);
        }
        
        [Fact]
        public void ShouldDoNothingOnDeletedParticipants()
        {
            var storage = new PersistentParticipantStorage(_fileService);
            
            storage.DeleteParticipant("id", "part-id");
            var result = storage.GetParticipants("id", true);
            Assert.Empty(result);
        }
        
        [Theory]
        [InlineData(AccessMode.Read)]
        [InlineData(AccessMode.Write)]
        public void ShouldGetInitialDelta(AccessMode mode)
        {
            var storage = new PersistentParticipantStorage(_fileService);
            
            var encryptedModel = new EncryptedParticipantModel
            {
                Id = "part-id"
            };

            storage.SaveData("id", encryptedModel);

            var saveFileName =
                $"{encryptedModel.Id}-{encryptedModel.SaveFileTimestamp:yyyy-MM-dd}T{encryptedModel.SaveFileTimestamp:HH-mm-ss}.save";
            
            var result = storage.GetDelta("id", "part-id", mode);
            Assert.Equal("part-id", result.ParticipantId);
            Assert.Equal(encryptedModel.SaveFileTimestamp, result.SaveFileTimestamp);
            Assert.Equal(saveFileName, result.SaveFileName);
            Assert.Null(result.Documents);
            Assert.Null(result.DocumentTree);
        }
        
        [Theory]
        [InlineData(AccessMode.Read)]
        [InlineData(AccessMode.Write)]
        public void ShouldReleaseCorrectLock(AccessMode mode)
        {
            var storage = new PersistentParticipantStorage(_fileService);
            
            var encryptedModel = new EncryptedParticipantModel
            {
                Id = "part-id"
            };

            storage.SaveData("id", encryptedModel);
            
            var result = storage.GetDelta("id", "part-id", mode);
            
            storage.ReleaseLock("id", "part-id", mode);
        }
        
        [Fact]
        public void ShouldSaveAndReturnLock()
        {
            var storage = new PersistentParticipantStorage(_fileService);
            
            var encryptedModel = new EncryptedParticipantModel
            {
                Id = "part-id"
            };

            storage.SaveData("id", encryptedModel);
            
            var expectedResult = storage.GetDelta("id", "part-id");
            storage.SaveDelta("id", expectedResult);
            storage.ReleaseLock("id", "part-id");
            var test = storage.GetDelta("id", "part-id");
         
            var expected = JsonConvert.SerializeObject(expectedResult);
            var actual = JsonConvert.SerializeObject(test);
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void ShouldRemoveDelta()
        {
            var storage = new PersistentParticipantStorage(_fileService);
            
            var encryptedModel = new EncryptedParticipantModel
            {
                Id = "part-id"
            };

            storage.SaveData("id", encryptedModel);
            
            var expectedResult = storage.GetDelta("id", "part-id");
            storage.SaveDelta("id", expectedResult);
            storage.ReleaseLock("id", "part-id");
            
            storage.RemoveDelta("id", "part-id");

            var test = storage.GetDelta("id", "part-id");
            Assert.NotSame(expectedResult, test);
        }
        
        [Fact]
        public void ShouldDoNothingWithNoDeltaToRemove()
        {
            var storage = new PersistentParticipantStorage(_fileService);
            
            storage.RemoveDelta("id", "part-id");
        }
        
        
    }
}