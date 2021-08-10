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
using LotsenApp.Client.Participant.Dto;
using LotsenApp.Client.Participant.Header;

namespace LotsenApp.Client.Participant
{
    public interface IParticipantService
    {
        Task<List<ParticipantHeaderDto>> GetParticipantOverview(string userId);

        Task<ParticipantHeaderDto> GetSpecificParticipantInfo(string userId, string participantId);

        Task SaveChanges(string userId, string participantId);

        Task<ParticipantDocumentDto> GetDocumentsForUser(string userId, string participantId);
        Task<DocumentDto> GetDocumentForUser(string userId, string participantId, string documentId);

        Task<IdentifierResponse> CreateDocument(string userId, string participantId, CreateDocumentDto createDocumentDto);

        Task UpdateDocument(string userId, string participantId, UpdateDocumentDto documentValue);

        Task ReorderDocuments(string userId, string participantId, ReOrderDto[] reOrderDtos);

        Task<DocumentValueDto> GetDocumentValues(string userId, string participantId, string documentId);

        Task<IdentifierResponse> DeleteDocument(string userId, string participantId, string documentId);

        Task<IdentifierResponse> CreateGroup(string userId, string participantId, CreateGroupDto createGroupDto);

        Task<IdentifierResponse> DeleteGroup(string userId, string participantId, string documentId, string groupId);

        Task ReorderGroup(string userId, string participantId, string documentId, ReOrderDto[] reOrderDto);

        Task<NewParticipantResponse> CreateParticipant(string userId, CreateParticipantDto request);

        Task<IdentifierResponse> DeleteParticipant(string userId, string participantId);

        Task<HeaderEntryDto[]> CreateHeaderEntryDtos(string userId);

        Task UpdateParticipantHeader(string userId, HeaderEditDto dto);
    }
}