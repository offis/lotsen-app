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

using System.Collections.Generic;
using System.Threading.Tasks;
using LotsenApp.Client.Participant.Delta;
using LotsenApp.Client.Participant.Dto;
using LotsenApp.Client.Participant.Header;
using Microsoft.Extensions.Logging;

namespace LotsenApp.Client.Participant
{
    public class ParticipantServiceImpl : IParticipantService
    {
        private readonly IParticipantStorage _storageService;
        private readonly IParticipantTransformationService _transformationService;
        private readonly IDeltaService _deltaService;
        private readonly IMergeService _mergeService;
        private readonly ILogger<ParticipantServiceImpl> _logger;

        public ParticipantServiceImpl(IParticipantStorage storageService,
            IParticipantTransformationService transformationService, IDeltaService deltaService,
            IMergeService mergeService, ILogger<ParticipantServiceImpl> logger)
        {
            _storageService = storageService;
            _transformationService = transformationService;
            _deltaService = deltaService;
            _mergeService = mergeService;
            _logger = logger;
        }

        public async Task<List<ParticipantHeaderDto>> GetParticipantOverview(string userId)
        {
            var participants = _storageService.GetParticipants(userId);
            var headerDtos = new List<ParticipantHeaderDto>();
            foreach (var participant in participants)
            {
                headerDtos.Add(await _transformationService.CreateHeaderDto(userId, participant));
            }

            return headerDtos;
        }

        public async Task<ParticipantHeaderDto> GetSpecificParticipantInfo(string userId, string participantId)
        {
            var saveState = _storageService.GetLatestSaveState(userId, participantId);
            return await _transformationService.CreateHeaderDto(userId, saveState);
        }

        public async Task SaveChanges(string userId, string participantId)
        {
            var saveState = _storageService.GetLatestSaveState(userId, participantId);
            try
            {
                var delta = _storageService.GetDelta(userId, participantId, AccessMode.Read);
                delta = await _deltaService.SeedDeltaFile(userId, saveState, delta);
                var newSaveState = await _mergeService.MergeModelWithDelta(userId, saveState, delta);
                if (saveState != newSaveState)
                {
                    _storageService.SaveData(userId, newSaveState);
                    _storageService.RemoveDelta(userId, participantId);
                }
            }
            finally
            {
                _storageService.ReleaseLock(userId, participantId, AccessMode.Read);
            }
        }

        public async Task<ParticipantDocumentDto> GetDocumentsForUser(string userId, string participantId)
        {
            var saveState = _storageService.GetLatestSaveState(userId, participantId);
            try
            {
                var delta = _storageService.GetDelta(userId, participantId, AccessMode.Read);
                delta = await _deltaService.SeedDeltaFile(userId, saveState, delta);
                return await _transformationService.GetDocuments(userId, saveState, delta);
            }
            finally
            {
                _storageService.ReleaseLock(userId, participantId, AccessMode.Read);
            }
        }

        public async Task<DocumentDto> GetDocumentForUser(string userId, string participantId, string documentId)
        {
            var saveState = _storageService.GetLatestSaveState(userId, participantId);
            try
            {
                var delta = _storageService.GetDelta(userId, participantId, AccessMode.Read);
                delta = await _deltaService.SeedDeltaFile(userId, saveState, delta);
                return await _transformationService.GetDocument(userId, documentId, saveState, delta);
            }
            finally
            {
                _storageService.ReleaseLock(userId, participantId, AccessMode.Read);
            }
        }

        public async Task CopyValues(string userId, string participantId, string documentId, string documentId2, bool preserve)
        {
            var document1 = await GetDocumentValues(userId, participantId, documentId);
            var document2 = await GetDocumentValues(userId, participantId, documentId2);
            var updatedDocument = await _transformationService.CopyValues(document1, document2, preserve);
            await UpdateDocument(userId, participantId, updatedDocument);
        }

        public async Task<IdentifierResponse> CreateDocument(string userId, string participantId,
            CreateDocumentDto createDocumentDto)
        {
            try
            {
                var saveState = _storageService.GetLatestSaveState(userId, participantId);
                var delta = _storageService.GetDelta(userId, participantId);
                delta = await _deltaService.SeedDeltaFile(userId, saveState, delta);
                var (newSaveState, documentId) =
                    await _deltaService.AddDocument(userId, delta, createDocumentDto);
                _storageService.SaveDelta(userId, newSaveState);
                return new IdentifierResponse
                {
                    Id = documentId
                };
            }
            finally
            {
                _storageService.ReleaseLock(userId, participantId);
            }
        }

        public async Task UpdateDocument(string userId, string participantId, IDocumentChange documentValue)
        {
            try
            {
                var saveState = _storageService.GetLatestSaveState(userId, participantId);
                if (saveState == null)
                {
                    _logger.LogWarning(
                        $"The participant {participantId} for user {userId} does not exist or is deleted. The document will not be updated");
                    return;
                }

                var delta = _storageService.GetDelta(userId, participantId);
                delta = await _deltaService.SeedDeltaFile(userId, saveState, delta);

                var newSaveState =
                    await _deltaService.UpdateDocument(userId, saveState, delta, documentValue);
                _storageService.SaveDelta(userId, newSaveState);
            }
            finally
            {
                _storageService.ReleaseLock(userId, participantId);
            }
        }

        public async Task ReorderDocuments(string userId, string participantId, ReOrderDto[] reOrderDtos)
        {
            try
            {
                var delta = _storageService.GetDelta(userId, participantId);
                var saveState = _storageService.GetLatestSaveState(userId, participantId);
                delta = await _deltaService.SeedDeltaFile(userId, saveState, delta);
                var newDelta = await _deltaService.ReorderDocuments(userId, delta, reOrderDtos);
                _storageService.SaveDelta(userId, newDelta);
            }
            finally
            {
                _storageService.ReleaseLock(userId, participantId);
            }
        }

        public async Task<DocumentValueDto> GetDocumentValues(string userId, string participantId, string documentId)
        {
            try
            {
                var model = _storageService.GetLatestSaveState(userId, participantId);
                var delta = _storageService.GetDelta(userId, participantId, AccessMode.Read);
                var saveState = _storageService.GetLatestSaveState(userId, participantId);
                delta = await _deltaService.SeedDeltaFile(userId, saveState, delta);
                var dto = await _transformationService.CreateValueDto(userId, model, delta, documentId);
                return dto;
            }
            finally
            {
                _storageService.ReleaseLock(userId, participantId, AccessMode.Read);
            }
        }

        public async Task<IdentifierResponse> DeleteDocument(string userId, string participantId, string documentId)
        {
            try
            {
                var delta = _storageService.GetDelta(userId, participantId);
                var saveState = _storageService.GetLatestSaveState(userId, participantId);
                delta = await _deltaService.SeedDeltaFile(userId, saveState, delta);
                var newSaveState = await _deltaService.DeleteDocument(userId, delta, documentId);
                _storageService.SaveDelta(userId, newSaveState);
                return new IdentifierResponse
                {
                    Id = documentId
                };
            }
            finally
            {
                _storageService.ReleaseLock(userId, participantId);
            }
        }

        public async Task<IdentifierResponse> CreateGroup(string userId, string participantId, CreateGroupDto createGroupDto)
        {
            try
            {
                var delta = _storageService.GetDelta(userId, participantId);
                var saveState = _storageService.GetLatestSaveState(userId, participantId);
                delta = await _deltaService.SeedDeltaFile(userId, saveState, delta);
                var (newSaveState, groupId) =
                    await _deltaService.AddGroup(userId, delta, createGroupDto);
                _storageService.SaveDelta(userId, newSaveState);
                return new IdentifierResponse
                {
                    Id = groupId
                };
            }
            finally
            {
                _storageService.ReleaseLock(userId, participantId);
            }
        }

        public async Task<IdentifierResponse> DeleteGroup(string userId, string participantId, string documentId,
            string groupId)
        {
            try
            {

                var delta = _storageService.GetDelta(userId, participantId);
                var saveState = _storageService.GetLatestSaveState(userId, participantId);
                delta = await _deltaService.SeedDeltaFile(userId, saveState, delta);
                var newSaveState =
                    await _deltaService.RemoveGroup(userId, delta, documentId, groupId);
                _storageService.SaveDelta(userId, newSaveState);
                return new IdentifierResponse
                {
                    Id = groupId
                };
            }
            finally
            {
                _storageService.ReleaseLock(userId, participantId);
            }
        }

        public async Task ReorderGroup(string userId, string participantId, string documentId, ReOrderDto[] reOrderDto)
        {
            try
            {
                var delta = _storageService.GetDelta(userId, participantId);
                var saveState = _storageService.GetLatestSaveState(userId, participantId);
                delta = await _deltaService.SeedDeltaFile(userId, saveState, delta);
                var newSaveState =
                    await _deltaService.ReorderGroups(userId, delta, documentId, reOrderDto);
                _storageService.SaveDelta(userId, newSaveState);
            }
            finally
            {
                _storageService.ReleaseLock(userId, participantId);
            }
        }

        public async Task<NewParticipantResponse> CreateParticipant(string userId, CreateParticipantDto request)
        {
            var encryptedModel = await _transformationService.CreateEncryptedModel(request, userId);
            _storageService.SaveData(userId, encryptedModel);
            return new NewParticipantResponse
            {
                Id = encryptedModel.Id
            };
        }

        public Task<IdentifierResponse> DeleteParticipant(string userId, string participantId)
        {
            _storageService.DeleteParticipant(userId, participantId);
            return Task.FromResult(new IdentifierResponse
            {
                Id = participantId
            });
        }

        public Task<HeaderEntryDto[]> CreateHeaderEntryDtos(string userId)
        {
            return _transformationService.CreateHeaderEntryDtos(userId, _storageService);
        }

        public async Task UpdateParticipantHeader(string userId, HeaderEditDto dto)
        {
            var saveState = _storageService.GetLatestSaveState(userId, dto.ParticipantId);
            var newSaveState = await _transformationService.UpdateParticipantHeader(userId, dto, saveState);
            _storageService.SaveData(userId, newSaveState);
        }
    }
}