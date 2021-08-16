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

using System.Threading.Tasks;
using LotsenApp.Client.Authentication.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LotsenApp.Client.Participant.Dto;
using LotsenApp.Client.Participant.Header;
using Microsoft.Extensions.Logging;
using HeaderDto = LotsenApp.Client.Participant.Header.HeaderDto;

namespace LotsenApp.Client.Participant
{
    [Route("api/participants")]
    [ApiController]
    public class ParticipantController : ControllerBase
    {
        private readonly UserManager<LocalLotsenAppUser> _userManager;
        private readonly IParticipantService _participantService;
        private readonly IParticipantHeaderService _headerService;
        private readonly ILogger<ParticipantController> _logger;

        public ParticipantController(UserManager<LocalLotsenAppUser> userManager, IParticipantService participantService,
            IParticipantHeaderService headerService,
            ILogger<ParticipantController> logger)
        {
            _userManager = userManager;
            _participantService = participantService;
            _headerService = headerService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetParticipantOverview()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Participant overview for user {user.Id} requested");

            var headerDtos = await _participantService.GetParticipantOverview(user.Id);

            return Ok(headerDtos);
        }

        [HttpGet("header")]
        [Authorize]
        public async Task<IActionResult> GetHeaderEntries()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Header entries for user {user.Id} requested");

            return Ok(await _participantService.CreateHeaderEntryDtos(user.Id));
        }
        
        [HttpPost("header")]
        [Authorize]
        public async Task<IActionResult> AddHeaderEntry([FromBody] HeaderDto dto)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"New header entry for user {user.Id}");

            if (dto.ParticipantId != null)
            {
                _headerService.AddToParticipantHeader(user.Id, dto.ParticipantId, dto.Path);
            } else if (dto.ProjectId != null)
            {
                _headerService.AddToProjectHeader(user.Id, dto.ProjectId, dto.Path);
            }

            return Ok();
        }
        
        [HttpPut("header")]
        [Authorize]
        public async Task<IActionResult> RemoveHeaderEntry([FromBody] HeaderDto dto)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Removal of header entry for user {user.Id}");

            if (dto.ParticipantId != null)
            {
                _headerService.RemoveFromParticipantHeader(user.Id, dto.ParticipantId, dto.Path);
            } else if (dto.ProjectId != null)
            {
                _headerService.RemoveFromProjectHeader(user.Id, dto.ProjectId, dto.Path);
            }

            return Ok();
        }

        [HttpGet("{participantId}")]
        [Authorize]
        public async Task<IActionResult> GetSpecificParticipantInfo([FromRoute] string participantId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Specific participant information for user {user.Id} and participant {participantId} requested");
            return Ok(await _participantService.GetSpecificParticipantInfo(user.Id, participantId));
        }

        [HttpGet("{participantId}/save")]
        [Authorize]
        public async Task<IActionResult> SaveChanges([FromRoute] string participantId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Saving data for user {user.Id} and participant {participantId} requested");

            await _participantService.SaveChanges(user.Id, participantId);

            return Ok();
        }

        [HttpGet("{participantId}/document")]
        [Authorize]
        public async Task<IActionResult> GetDocumentsForUser([FromRoute] string participantId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Document overview for user {user.Id} and participant {participantId} requested");

            return Ok(await _participantService.GetDocumentsForUser(user.Id, participantId));
        }

        [HttpGet("{participantId}/document/{documentId}")]
        [Authorize]
        public async Task<IActionResult> GetDocumentForUser([FromRoute] string participantId, [FromRoute] string documentId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Specific document {documentId} for user {user.Id} and participant {participantId} requested");
            return Ok(await _participantService.GetDocumentForUser(user.Id, participantId, documentId));

        }
        
        [HttpGet("{participantId}/document/{documentId}/copy/{documentId2}")]
        [Authorize]
        public async Task<IActionResult> CopyValuesForDocument([FromRoute] string participantId, [FromRoute] string documentId, [FromRoute] string documentId2, [FromQuery] bool preserve = true)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Specific document {documentId} for user {user.Id} and participant {participantId} requested");
            await _participantService.CopyValues(user.Id, participantId, documentId, documentId2, preserve);
            return Ok();
        }

        [HttpPost("{participantId}/document")]
        [Authorize]
        public async Task<IActionResult> CreateDocument([FromRoute] string participantId,
            [FromBody] CreateDocumentDto createDocumentDto)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Creating document {createDocumentDto.DocumentId} for user {user.Id} and participant {participantId} requested");
            return Ok(await _participantService.CreateDocument(user.Id, participantId, createDocumentDto));
        }

        [HttpPut("{participantId}/document")]
        [Authorize]
        public async Task<IActionResult> UpdateDocument([FromRoute] string participantId,
            [FromBody] UpdateDocumentDto documentValue)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Updating document {documentValue.Id} for user {user.Id} and participant {participantId} requested");
            await _participantService.UpdateDocument(user.Id, participantId, documentValue);
            return Ok();
        }

        [HttpPost("{participantId}/document/reorder")]
        [Authorize]
        public async Task<IActionResult> ReorderDocuments([FromRoute] string participantId,
            [FromBody] ReOrderDto[] reOrderDtos)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Reordering documents for user {user.Id} and participant {participantId} requested");

            await _participantService.ReorderDocuments(user.Id, participantId, reOrderDtos);

            return Ok();
        }

        [HttpGet("{participantId}/document/{documentId}/values")]
        [Authorize]
        public async Task<IActionResult> GetDocumentValues([FromRoute] string participantId,
            [FromRoute] string documentId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Values of document {documentId} for user {user.Id} and participant {participantId} requested");
            return Ok(await _participantService.GetDocumentValues(user.Id, participantId, documentId));
        }


        [HttpDelete("{participantId}/document/{documentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteDocument([FromRoute] string participantId,
            [FromRoute] string documentId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Deleting document {documentId} for user {user.Id} and participant {participantId} requested");

            return Ok(await _participantService.DeleteDocument(user.Id, participantId, documentId));
        }

        [HttpPost("{participantId}/group")]
        [Authorize]
        public async Task<IActionResult> CreateGroup([FromRoute] string participantId,
            [FromBody] CreateGroupDto createGroupDto)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Creating group in document {createGroupDto.DocumentId} for user {user.Id} and participant {participantId} requested");

            return Ok(await _participantService.CreateGroup(user.Id, participantId, createGroupDto));

        }

        [HttpDelete("{participantId}/document/{documentId}/group/{groupId}")]
        [Authorize]
        public async Task<IActionResult> DeleteGroup([FromRoute] string participantId,
            [FromRoute] string documentId, [FromRoute] string groupId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Deleting group {groupId} in document {documentId} for user {user.Id} and participant {participantId} requested");
            return Ok(await _participantService.DeleteGroup(user.Id, participantId, documentId, groupId));
        }
        
        [HttpPut("{participantId}/document/{documentId}/group")]
        [Authorize]
        public async Task<IActionResult> ReorderGroup([FromRoute] string participantId, [FromRoute] string documentId,
            [FromBody] ReOrderDto[] reOrderDto)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Reordering groups in document {documentId} for user {user.Id} and participant {participantId} requested");
            await _participantService.ReorderGroup(user.Id, participantId, documentId, reOrderDto);
            return Ok();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateParticipant([FromBody] CreateParticipantDto request)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Creating a participant for user {user.Id} requested");
            return Ok(await _participantService.CreateParticipant(user.Id, request));
        }

        [HttpDelete("{participantId}")]
        [Authorize]
        public async Task<IActionResult> DeleteParticipant([FromRoute] string participantId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Deletion of participant {participantId} for user {user.Id} requested");
            await _participantService.DeleteParticipant(user.Id, participantId);
            return Ok();
        }

        [HttpPost("{participantId}/header")]
        public async Task<IActionResult> UpdateHeader([FromRoute] string participantId, [FromBody] HeaderEditDto dto)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogInformation($"Update of header for participant {participantId} of user {user.Id} requested");
            await _participantService.UpdateParticipantHeader(user.Id, dto);
            return Ok();
        }
    }
}