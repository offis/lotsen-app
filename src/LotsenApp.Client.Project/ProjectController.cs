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
using System.Threading.Tasks;
using LotsenApp.Client.Authentication.Api;
using LotsenApp.Client.DataFormat;
using LotsenApp.Client.DataFormat.Access;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LotsenApp.Client.Project
{
    [Route("api/project")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly UserManager<LocalLotsenAppUser> _userManager;
        private readonly DataFormatService _dataFormatService;

        public ProjectController(UserManager<LocalLotsenAppUser> userManager, DataFormatService dataFormatService)
        {
            _userManager = userManager;
            _dataFormatService = dataFormatService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<Displayable>> GetProjects()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            return await _dataFormatService.GetUserProjects(user.Id);
        }

        [HttpGet("detail")]
        [Authorize]
        public async Task<IEnumerable<DataDefinitionDto>> GetProjectDetails()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return await _dataFormatService.GetDataDefinitionsForUser(user.Id);
        }

        [HttpGet("{projectId}/document")]
        [Authorize]
        public async Task<IActionResult> GetDocumentsForProject([FromRoute] string projectId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (!await _dataFormatService.UserHasAccessToProject(user.Id, projectId))
            {
                return Forbid();
            }

            return Ok(await _dataFormatService.GetDisplayablesInProject(projectId));
        }

        [HttpGet("{projectId}/documentation-event/{documentationEventId}")]
        [Authorize]
        public async Task<IActionResult> GetDocumentsForDocumentationEvent([FromRoute] string projectId,
            [FromRoute] string documentationEventId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (!await _dataFormatService.UserHasAccessToProject(user.Id, projectId))
            {
                return Forbid();
            }

            var displayables =
                await _dataFormatService.GetDisplayablesInDocumentationEvent(projectId, documentationEventId);
            return Ok(displayables);
        }

        [HttpGet("document")]
        [Authorize]
        public async Task<IEnumerable<Displayable>> GetDocumentationProjects()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var projects = await _dataFormatService.GetDocumentationProjects(user.Id);

            return projects;
        }

        [HttpGet("{projectId}/document/{documentId}")]
        [Authorize]
        public async Task<Displayable> GetDocuments([FromRoute] string projectId, [FromRoute] string documentId)
        {
            return await _dataFormatService.GetDocumentHeader(projectId, documentId);
        }

        [HttpGet("{projectId}/document/{documentId}/detail")]
        [Authorize]
        public IActionResult GetDocumentDetail([FromRoute] string projectId, [FromRoute] string documentId)
        {
            try
            {
                return Ok(_dataFormatService.GetDocumentFormat(projectId, documentId));
            }
            catch (NullReferenceException)
            {
                return StatusCode(500, "The document id that was used for this request is invalid");
            }
            catch (DocumentationEventNotFoundException)
            {
                return StatusCode(500, $"The documentation event with the ID {documentId} was not found");
            }
            catch (DocumentNotFoundException)
            {
                return StatusCode(500, $"A document relating to the ID {documentId} was not found");
            }

        }
    }
}