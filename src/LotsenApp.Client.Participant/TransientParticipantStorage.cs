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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LotsenApp.Client.File;
using LotsenApp.Client.Participant.Delta;
using LotsenApp.Client.Participant.Model;
using LotsenApp.Client.Plugin;

namespace LotsenApp.Client.Participant
{
    public class TransientParticipantStorage: IParticipantStorage
    {

        private readonly IDictionary<string, IDictionary<string, EncryptedDeltaFile>> _deltas =
            new ConcurrentDictionary<string, IDictionary<string, EncryptedDeltaFile>>();
        private readonly IDictionary<string, List<EncryptedDeltaFile>> _deltaHistory =
            new ConcurrentDictionary<string, List<EncryptedDeltaFile>>();

        private readonly IDictionary<string, List<EncryptedParticipantModel>> _participants =
            new ConcurrentDictionary<string, List<EncryptedParticipantModel>>();

        public EncryptedParticipantModel[] GetParticipants(string userId, bool includeDeleted = false)
        {
            var participantDictionary = new Dictionary<string, EncryptedParticipantModel>();
            var exists = _participants.TryGetValue(userId, out var participantList);
            if (!exists)
            {
                return new EncryptedParticipantModel[0];
            }
            foreach (var model in participantList.OrderByDescending(p => p.SaveFileTimestamp))
            {
                // Only include deleted if desired and filter older save states
                if (model.IsDeleted && !includeDeleted ||
                    participantDictionary.ContainsKey(model.Id))
                {
                    continue;
                }

                participantDictionary.Add(model.Id, model);
            }
            
            return participantDictionary.Values.ToArray();
        }

        public void DeleteParticipant(string userId, string participantId)
        {
            var exists = _participants.TryGetValue(userId, out var saveStates);
            if (!exists)
            {
                return;
            }
            foreach (var encryptedParticipantModel in saveStates.Where(s => s.Id == participantId))
            {
                encryptedParticipantModel.IsDeleted = true;
                encryptedParticipantModel.PermanentDeletionTime = DateTime.Now + TimeSpan.FromDays(14);
            }
        }

        public IEnumerable<EncryptedParticipantModel> GetAllSaveStatesForParticipant(string userId, string participantId)
        {
            var exists = _participants.TryGetValue(userId, out var saveStates);
            if (!exists)
            {
                return Enumerable.Empty<EncryptedParticipantModel>();
            }

            return saveStates.Where(s => s.Id == participantId);
        }

        public EncryptedParticipantModel GetLatestSaveState(string userId, string participantId)
        {
            return GetAllSaveStatesForParticipant(userId, participantId)
                .OrderByDescending(p => p.SaveFileTimestamp)
                .FirstOrDefault();
        }

        public void SaveData(string userId, EncryptedParticipantModel model)
        {
            var creationTime = DateTime.UtcNow;
            model.SaveFileTimestamp = creationTime;
            if (!_participants.ContainsKey(userId))
            {
                _participants.Add(userId, new List<EncryptedParticipantModel>());
            }
            _participants[userId].Add(model);
        }

        public EncryptedDeltaFile GetDelta(string userId, string participantId, AccessMode mode = AccessMode.Write)
        {
            var lockSlim = ConcurrentFileAccessHelper.GetAccessor($"{userId}-{participantId}");
            if (mode == AccessMode.Read)
            {
                lockSlim.EnterReadLock();
            }
            else
            {
                lockSlim.EnterWriteLock();
            }
            var exists = _deltas.TryGetValue(userId, out var deltaFiles);
            if (!exists || !deltaFiles.ContainsKey(participantId))
            {
                var saveState = GetLatestSaveState(userId, participantId);
                var newDelta = new EncryptedDeltaFile
                {
                    DeltaTimestamp = DateTime.UtcNow,
                    ParticipantId = participantId,
                    SaveFileName =
                        $"{saveState.Id}-{saveState.SaveFileTimestamp:yyyy-MM-dd}T{saveState.SaveFileTimestamp:HH-mm-ss}.save",
                    SaveFileTimestamp = saveState.SaveFileTimestamp,
                };
                return newDelta;
            }
            
            var delta = deltaFiles[participantId];

            return delta;
        }

        public void ReleaseLock(string userId, string participantId, AccessMode mode = AccessMode.Write)
        {
            var lockSlim = ConcurrentFileAccessHelper.GetAccessor($"{userId}-{participantId}");
            if (mode == AccessMode.Read)
            {
                lockSlim.ExitReadLock();
            }
            else
            {
                lockSlim.ExitWriteLock();
            }
        }

        public void RemoveDelta(string userId, string participantId)
        {
            var exists = _deltas.TryGetValue(userId, out var deltas);
            if (!exists || !deltas.ContainsKey(participantId))
            {
                return;
            }

            var participantDelta = deltas[participantId];

            if (!_deltaHistory.ContainsKey(participantId))
            {
                _deltaHistory.Add(participantId, new List<EncryptedDeltaFile>());
            }
            _deltaHistory[participantId].Add(participantDelta);
            deltas.Remove(participantId);
        }

        public void SaveDelta(string userId, EncryptedDeltaFile deltaFile)
        {
            var creationTime = DateTime.UtcNow;
            deltaFile.DeltaTimestamp = creationTime;
            if (!_deltas.ContainsKey(userId))
            {
                _deltas.Add(userId, new ConcurrentDictionary<string, EncryptedDeltaFile>());
            }

            _deltas[userId][deltaFile.ParticipantId] = deltaFile;
        }
    }
}