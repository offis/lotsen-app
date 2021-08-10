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
using LotsenApp.Client.Participant.Model;
using Xunit;

namespace LotsenApp.Client.Participant.Test.Dto
{
    [ExcludeFromCodeCoverage]
    public class ParticipantModelTest
    {
        [Fact]
        public void ShouldAssignSetter()
        {
            var synchronizedAt = DateTime.Now;
            var header = new Dictionary<string, List<string>>();
            var encryptedHeader = Guid.NewGuid().ToString();
            var encryptedBody = Guid.NewGuid().ToString();

            var model = new ParticipantModel
            {
                SynchronizedAt = synchronizedAt,
                Header = header,
                EncryptedHeader = encryptedHeader,
                EncryptedBody = encryptedBody
            };
            
            Assert.Equal(synchronizedAt, model.SynchronizedAt);
            Assert.Equal(header, model.Header);
            Assert.Equal(encryptedHeader, model.EncryptedHeader);
            Assert.Equal(encryptedBody, model.EncryptedBody);
        }

        [Fact]
        public void ShouldAssignValuesInConstructor()
        {
            var saveTime = DateTime.Now;
            var encryptedHeader = Guid.NewGuid().ToString();
            var encryptedData = Guid.NewGuid().ToString();
            var isDeleted = new Random().NextDouble() <= 0.5;
            var deletedAt = saveTime + TimeSpan.FromMinutes(5);
            var permanentDeletionTime = deletedAt + TimeSpan.FromDays(14);

            var encryptedModel = new EncryptedParticipantModel
            {
                SaveFileTimestamp = saveTime,
                EncryptedHeader = encryptedHeader,
                EncryptedData = encryptedData,
                IsDeleted = isDeleted,
                DeletedAt = deletedAt,
                PermanentDeletionTime = permanentDeletionTime
            };

            var model = new ParticipantModel(encryptedModel, new DataBody(), new Dictionary<string, List<string>>());
            
            Assert.Equal(saveTime, model.SaveFileTimestamp);
            Assert.Equal(encryptedHeader, model.EncryptedHeader);
            Assert.Equal(encryptedData, model.EncryptedBody);
            Assert.Equal(isDeleted, model.IsDeleted);
            Assert.Equal(deletedAt, model.DeletedAt);
            Assert.Equal(permanentDeletionTime, model.PermanentDeletionTime);
        }
    }
}