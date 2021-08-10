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
using System.Linq;
using System.Threading.Tasks;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using LotsenApp.Client.Authentication.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LotsenApp.Client.Reminder
{
    [Route("api/reminder")]
    [ApiController]
    public class ReminderController : ControllerBase
    {
        private readonly ReminderStorage _storage;
        private readonly UserManager<LocalLotsenAppUser> _userManager;
        private readonly ILogger<ReminderController> _logger;

        public ReminderController(ReminderStorage storage, UserManager<LocalLotsenAppUser> userManager,
            ILogger<ReminderController> logger)
        {
            _storage = storage;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("calendar.ics")]
        [Authorize]
        public async Task<string> GetICalInformation()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var reminder = _storage.GetReminder(user.Id, false);

            // New iCalendar object
            var calendar = new Calendar();

            foreach (var reminderModel in reminder)
            {
                calendar.Events.Add(new CalendarEvent
                {
                    Class = "PUBLIC",
                    Summary = reminderModel.Header,
                    Created = new CalDateTime(DateTime.Now),
                    Description = JsonConvert.SerializeObject(reminderModel, Formatting.None),
                    Start = new CalDateTime(reminderModel.ReminderDate),
                    End = new CalDateTime(reminderModel.ReminderDateEnd ??
                                          reminderModel.ReminderDate.Add(TimeSpan.FromMinutes(15))),
                    Sequence = 0,
                    Uid = reminderModel.ReminderId,
                    Location = reminderModel.ActionUrl
                });
            }

            var serializer = new CalendarSerializer(new SerializationContext());
            return serializer.SerializeToString(calendar);
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<ReminderModel>> GetReminder([FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            _logger.LogDebug($"Reminder for user {user.Id} between {startDate:s} and {endDate:s} requested");
            var reminder = _storage.GetReminder(user.Id, false);
            return reminder.Where(r =>
                (startDate == null || r.ReminderDate >= startDate) && (endDate == null || r.ReminderDate <= endDate));
        }

        [HttpPost]
        [Authorize]
        public async Task<IdResponse> CreateReminder([FromBody] ReminderModel model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var reminder = _storage.GetReminder(user.Id, true);
            model.ReminderId ??= Guid.NewGuid().ToString();
            reminder.Add(model);
            var sortedReminder = reminder.OrderBy(r => r.ReminderDate).ToList();
            _storage.SaveReminder(user.Id, sortedReminder);
            return new IdResponse
            {
                Id = model.ReminderId
            };
        }

        [HttpDelete("{reminderId}")]
        [Authorize]
        public async Task<IdResponse> DeleteReminder([FromRoute] string reminderId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var reminder = _storage.GetReminder(user.Id, true);
            var newReminder = reminder.Where(r => r.ReminderId != reminderId).ToList();
            _storage.SaveReminder(user.Id, newReminder);
            return new IdResponse
            {
                Id = reminderId
            };
        }

        [HttpPut]
        [Authorize]
        public async Task<IdResponse> UpdateReminder([FromBody] ReminderModel model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var reminder = _storage.GetReminder(user.Id, true);
            var targetReminder = reminder.FirstOrDefault(r => r.ReminderId == model.ReminderId);
            if (targetReminder == null)
            {
                _storage.SaveReminder(user.Id, reminder);
                return await CreateReminder(model);
            }

            targetReminder.Header = model.Header;
            targetReminder.Text = model.Text;
            targetReminder.ReminderDate = model.ReminderDate;
            targetReminder.ReminderDateEnd = model.ReminderDateEnd;
            targetReminder.ActionUrl = model.ActionUrl;
            _storage.SaveReminder(user.Id, reminder);


            return new IdResponse
            {
                Id = model.ReminderId
            };
        }
    }
}