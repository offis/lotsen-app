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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LotsenApp.Client.Reminder;
using Newtonsoft.Json;
using Xunit;

namespace LotsenApp.Client.Electron.Test
{
    [ExcludeFromCodeCoverage]
    [Trait("Type", "Integration")]
    public class ReminderControllerTest: IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public ReminderControllerTest(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ShouldGetCalendarInformation()
        {
            using var client = _factory.CreateClient();

            var response = await client.GetAsync("api/authentication/offline");
            Assert.True(response.IsSuccessStatusCode);
            var calendarResponse = await client.GetAsync("api/reminder/calendar.ics");
            Assert.True(calendarResponse.IsSuccessStatusCode);
            var content = await calendarResponse.Content.ReadAsStringAsync();
            
            Assert.StartsWith("BEGIN:VCALENDAR", content);
        }
        
        [Fact]
        public async Task ShouldAddNewReminderToCalendar()
        {
            using var client = _factory.CreateClient();

            var newReminder = new ReminderModel
            {
                ReminderId = Guid.NewGuid().ToString(),
                Header = "AddNewReminderToCalendar",
                Text = "Description",
                ReminderDate = DateTime.UtcNow,
                ReminderDateEnd = DateTime.UtcNow + TimeSpan.FromHours(1)
            };
            
            var response = await client.GetAsync("api/authentication/offline");
            Assert.True(response.IsSuccessStatusCode);
            
            var postResponse = await client.PostAsJsonAsync("api/reminder", newReminder);
            Assert.True(postResponse.IsSuccessStatusCode);
            var postContent = await postResponse.Content.ReadAsStringAsync();
            var postDto = JsonConvert.DeserializeObject<IdResponse>(postContent);
            Assert.Equal(newReminder.ReminderId, postDto?.Id);
            var calendarResponse = await client.GetAsync("api/reminder/calendar.ics");
            Assert.True(calendarResponse.IsSuccessStatusCode);
            var content = await calendarResponse.Content.ReadAsStringAsync();
            
            Assert.StartsWith("BEGIN:VCALENDAR", content);
        }
        
        [Fact]
        public async Task ShouldDeleteReminder()
        {
            using var client = _factory.CreateClient();

            var newReminder = new ReminderModel
            {
                ReminderId = Guid.NewGuid().ToString(),
                Header = "HeaderDelete",
                Text = "Description",
                ReminderDate = DateTime.UtcNow,
                ReminderDateEnd = DateTime.UtcNow + TimeSpan.FromHours(1)
            };
            
            var response = await client.GetAsync("api/authentication/offline");
            Assert.True(response.IsSuccessStatusCode);
            
            var initialResponse = await client.GetAsync("api/reminder");
            var initialContent = await initialResponse.Content.ReadAsStringAsync();
            var initialDto = JsonConvert.DeserializeObject<ReminderModel[]>(initialContent);
            Assert.NotNull(initialDto);
            var initialIds = initialDto.Select(r => r.ReminderId).ToList();
            
            var postResponse = await client.PostAsJsonAsync("api/reminder", newReminder);
            Assert.True(postResponse.IsSuccessStatusCode);
            var postContent = await postResponse.Content.ReadAsStringAsync();
            var postDto = JsonConvert.DeserializeObject<IdResponse>(postContent);
            Assert.Equal(newReminder.ReminderId, postDto?.Id);
            var deleteResponse = await client.DeleteAsync($"api/reminder/{newReminder.ReminderId}");
            Assert.True(deleteResponse.IsSuccessStatusCode);
            var getResponse = await client.GetAsync("api/reminder");
            var content = await getResponse.Content.ReadAsStringAsync();
            var dto = JsonConvert.DeserializeObject<ReminderModel[]>(content);
            Assert.NotNull(dto);
            Assert.True(dto.All(model => initialIds.Contains(model.ReminderId)));
        }
        
        [Fact]
        public async Task ShouldUpdateReminder()
        {
            using var client = _factory.CreateClient();

            var newReminder = new ReminderModel
            {
                ReminderId = Guid.NewGuid().ToString(),
                Header = "Header",
                Text = "Description",
                ReminderDate = DateTime.UtcNow,
                ReminderDateEnd = DateTime.UtcNow + TimeSpan.FromHours(1)
            };
            
            var response = await client.GetAsync("api/authentication/offline");
            Assert.True(response.IsSuccessStatusCode);
            
            var postResponse = await client.PostAsJsonAsync("api/reminder", newReminder);
            Assert.True(postResponse.IsSuccessStatusCode);
            var postContent = await postResponse.Content.ReadAsStringAsync();
            var postDto = JsonConvert.DeserializeObject<IdResponse>(postContent);
            Assert.Equal(newReminder.ReminderId, postDto?.Id);
            newReminder.Header = "HeaderUpdate";
            var updateResponse = await client.PutAsJsonAsync("api/reminder", newReminder);
            Assert.True(updateResponse.IsSuccessStatusCode);
            var getResponse = await client.GetAsync("api/reminder");
            var content = await getResponse.Content.ReadAsStringAsync();
            var dto = JsonConvert.DeserializeObject<ReminderModel[]>(content);
            Assert.NotNull(dto);
            var updatedReminder = dto.First(d => d.ReminderId == newReminder.ReminderId);
            Assert.Equal(newReminder.Header, updatedReminder.Header);
        }
        
        [Fact]
        public async Task ShouldCreateReminderOnWrongUpdate()
        {
            using var client = _factory.CreateClient();

            var newReminder = new ReminderModel
            {
                ReminderId = Guid.NewGuid().ToString(),
                Header = "HeaderWrongUpdate",
                Text = "Description",
                ReminderDate = DateTime.UtcNow,
                ReminderDateEnd = DateTime.UtcNow + TimeSpan.FromHours(1)
            };
            
            var response = await client.GetAsync("api/authentication/offline");
            Assert.True(response.IsSuccessStatusCode);
            
            var updateResponse = await client.PutAsJsonAsync("api/reminder", newReminder);
            Assert.True(updateResponse.IsSuccessStatusCode);
            var getResponse = await client.GetAsync("api/reminder");
            var content = await getResponse.Content.ReadAsStringAsync();
            var dto = JsonConvert.DeserializeObject<ReminderModel[]>(content);
            Assert.NotNull(dto);
            
            var updatedReminder = dto.First(d => d.ReminderId == newReminder.ReminderId);
            Assert.Equal(newReminder.Header, updatedReminder.Header);
        }
    }
}