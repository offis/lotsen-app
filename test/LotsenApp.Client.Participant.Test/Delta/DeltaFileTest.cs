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
using LotsenApp.Client.Participant.Delta;
using LotsenApp.Client.Participant.Model;
using Xunit;

namespace LotsenApp.Client.Participant.Test.Delta
{
    [ExcludeFromCodeCoverage]
    public class DeltaFileTest
    {
        [Fact]
        public void ShouldAssignSetter()
        {
            var participantId = Guid.NewGuid().ToString();
            var saveFile = Guid.NewGuid().ToString();
            var dateTime = DateTime.Now;
            var saveTime = dateTime - TimeSpan.FromMinutes(5);
            var deltaFile = new DeltaFile
            {
                DeltaTimestamp = dateTime,
                SaveFileTimestamp = saveTime,
                ParticipantId = participantId,
                SaveFileName = saveFile,    
            };
            
            Assert.Equal(participantId, deltaFile.ParticipantId);
            Assert.Equal(saveTime, deltaFile.SaveFileTimestamp);
            Assert.Equal(dateTime, deltaFile.DeltaTimestamp);
            Assert.Equal(saveFile, deltaFile.SaveFileName);
        }
        
        [Fact]
        public void ShouldAssignValuesInConstructor()
        {
            var participantId = Guid.NewGuid().ToString();
            var saveFile = Guid.NewGuid().ToString();
            var dateTime = DateTime.Now;
            var saveTime = dateTime - TimeSpan.FromMinutes(5);
            var encryptedFile = new EncryptedDeltaFile
            {
                ParticipantId = participantId,
                SaveFileTimestamp = saveTime,
                SaveFileName = saveFile,
                DeltaTimestamp = dateTime,
            };
            var deltaFile = new DeltaFile(encryptedFile, new Dictionary<string, DocumentDelta>(),
                new Dictionary<string, TreeItem>());
            
            Assert.Equal(participantId, deltaFile.ParticipantId);
            Assert.Equal(saveTime, deltaFile.SaveFileTimestamp);
            Assert.Equal(dateTime, deltaFile.DeltaTimestamp);
            Assert.Equal(saveFile, deltaFile.SaveFileName);
        }
    }
}