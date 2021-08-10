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

namespace LotsenApp.Client.Participant.Model
{
    public class ParticipantModel
    {
        public string Id { get; set; }
        public DateTime SaveFileTimestamp { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Synchronized { get; set; }
        public DateTime SynchronizedAt { get; set; }
        public string OnlineId { get; set; }
        public IDictionary<string, List<string>> Header { get; set; } = new Dictionary<string, List<string>>();
        public DataBody Data { get; set; } = new DataBody();
        public IDictionary<string, string> Additional { get; set; } = new Dictionary<string, string>();
        public bool IsDeleted { get; set; }
        public DateTime DeletedAt { get; set; }
        public DateTime PermanentDeletionTime { get; set; }
        
        public string EncryptedHeader { get; set; }
        public string EncryptedBody { get; set; }

        public ParticipantModel()
        {
            
        }

        public ParticipantModel(EncryptedParticipantModel encryptedParticipantModel, DataBody documents = null, IDictionary<string, List<string>> header = null)
        {
            Id = encryptedParticipantModel.Id;
            SaveFileTimestamp = encryptedParticipantModel.SaveFileTimestamp;
            CreatedAt = encryptedParticipantModel.CreatedAt;
            Synchronized = encryptedParticipantModel.Synchronized;
            SynchronizedAt = encryptedParticipantModel.SynchronizedAt;
            OnlineId = encryptedParticipantModel.OnlineId;
            Header = header;
            Data = documents;
            Additional = encryptedParticipantModel.Additional;
            IsDeleted = encryptedParticipantModel.IsDeleted;
            DeletedAt = encryptedParticipantModel.DeletedAt;
            PermanentDeletionTime = encryptedParticipantModel.PermanentDeletionTime;
            EncryptedHeader = encryptedParticipantModel.EncryptedHeader;
            EncryptedBody = encryptedParticipantModel.EncryptedData;
        }
    }
}