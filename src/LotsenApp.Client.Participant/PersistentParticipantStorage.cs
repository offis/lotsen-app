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
using System.IO;
using System.Linq;
using System.Threading;
using LotsenApp.Client.File;
using LotsenApp.Client.Participant.Delta;
using LotsenApp.Client.Participant.Model;
using LotsenApp.Client.Plugin;
using Newtonsoft.Json;

namespace LotsenApp.Client.Participant
{
    public class PersistentParticipantStorage: IParticipantStorage
    {
        private readonly IFileService _fileService;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };

        public PersistentParticipantStorage(IFileService fileService)
        {
            _fileService = fileService;
        }

        public EncryptedParticipantModel[] GetParticipants(string userId, bool includeDeleted = false)
        {
            var dataDirectory = GetDirectory(userId);
            var participantDictionary = new Dictionary<string, EncryptedParticipantModel>();
            foreach (var file in dataDirectory.GetFiles().OrderByDescending(f => f.CreationTimeUtc))
            {
                var model = DeserializeFile(file);
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
            var directory = GetDirectory(userId);
            var saveStates = directory.GetFiles().Select(f => (f, DeserializeFile(f)));
            foreach (var encryptedParticipantModel in saveStates.Where(s => s.Item2?.Id == participantId))
            {
                encryptedParticipantModel.Item2.IsDeleted = true;
                encryptedParticipantModel.Item2.DeletedAt = DateTime.UtcNow;
                encryptedParticipantModel.Item2.PermanentDeletionTime = DateTime.UtcNow + TimeSpan.FromDays(14);
                var serializedModel = SerializeFile(encryptedParticipantModel.Item2);
                System.IO.File.WriteAllText(encryptedParticipantModel.f.FullName, serializedModel);
            }
        }

        public IEnumerable<EncryptedParticipantModel> GetAllSaveStatesForParticipant(string userId, string participantId)
        {
            var directory = GetDirectory(userId);
            foreach (var file in directory.GetFiles().Where(f => f.Name.StartsWith(participantId)))
            {
                yield return DeserializeFile(file);
            }
        }

        public EncryptedParticipantModel GetLatestSaveState(string userId, string participantId)
        {
            var directory = GetDirectory(userId);
            return directory
                .GetFiles()
                .Where(f => f.Name.StartsWith(participantId))
                .OrderByDescending(f => f.CreationTimeUtc)
                .Select(DeserializeFile)
                .FirstOrDefault(p => !p.IsDeleted);
        }

        public void SaveData(string userId, EncryptedParticipantModel model)
        {
            var directory = GetDirectory(userId);
            var creationTime = DateTime.UtcNow;
            var saveFile = Path.Combine(directory.FullName,
                $"{model.Id}-{creationTime:yyyy-MM-dd}T{creationTime:HH-mm-ss}.save");
            model.SaveFileTimestamp = creationTime;
            var serializedModel = SerializeFile(model);
            System.IO.File.WriteAllText(saveFile, serializedModel);
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
            var directory = GetDeltaDirectory(userId);
            var savedDelta = directory.GetFiles()
                .Where(f => f.Name == $"{participantId}.delta")
                .Select(DeserializeDeltaFile)
                .FirstOrDefault();
            if (savedDelta != null)
            {
                return savedDelta;
            }

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
            // Not possible. Seeding has to happen somewhere else.
            //return await _deltaService.SeedDeltaFile(userId, saveState, newDelta);
        }

        public void ReleaseLock(string userId, string participantId, AccessMode mode = AccessMode.Write)
        {
            var lockSlim = ConcurrentFileAccessHelper.GetAccessor($"{userId}-{participantId}");
            if (mode == AccessMode.Read && lockSlim.IsReadLockHeld)
            {
                lockSlim.ExitReadLock();
            }
            else if(lockSlim.IsWriteLockHeld)
            {
                lockSlim.ExitWriteLock();
            }
        }

        public void RemoveDelta(string userId, string participantId)
        {
            var directory = GetDeltaDirectory(userId);
            var file = directory.GetFiles().FirstOrDefault(f => f.Name == $"{participantId}.delta");
            if (file == null)
            {
                return;
            }

            var historyDirectory = Directory.CreateDirectory(Path.Combine(directory.FullName, "history"));

            var deletionTime = DateTime.UtcNow;
            var newFileName = file.Name.Replace(".delta", $"-{deletionTime:yyyy-MM-dd}T{deletionTime:HH-mm-ss}.delta");
            file.MoveTo(Path.Combine(historyDirectory.FullName, newFileName));
        }

        public void SaveDelta(string userId, EncryptedDeltaFile deltaFile)
        {
            var directory = GetDeltaDirectory(userId);
            var creationTime = DateTime.UtcNow;
            deltaFile.DeltaTimestamp = creationTime;
            var saveFile = Path.Combine(directory.FullName, $"{deltaFile.ParticipantId}.delta");
            var serializedModel = SerializeFile(deltaFile);
            System.IO.File.WriteAllText(saveFile, serializedModel);
        }

        private DirectoryInfo GetDirectory(string userId)
        {
            return Directory.CreateDirectory(_fileService.Join($"data/{userId}/saveState"));
        }

        private DirectoryInfo GetDeltaDirectory(string userId)
        {
            return Directory.CreateDirectory(_fileService.Join($"data/{userId}/deltas"));
        }

        private EncryptedParticipantModel DeserializeFile(FileInfo file)
        {
            return JsonConvert.DeserializeObject<EncryptedParticipantModel>(System.IO.File.ReadAllText(file.FullName));
        }

        private string SerializeFile(EncryptedParticipantModel model)
        {
            return JsonConvert.SerializeObject(model, _serializerSettings);
        }

        private string SerializeFile(EncryptedDeltaFile model)
        {
            return JsonConvert.SerializeObject(model, _serializerSettings);
        }

        private EncryptedDeltaFile DeserializeDeltaFile(FileInfo file)
        {
            return JsonConvert.DeserializeObject<EncryptedDeltaFile>(System.IO.File.ReadAllText(file.FullName));
        }
    }
}