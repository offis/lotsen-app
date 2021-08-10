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
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace LotsenApp.Client.Reminder.Test
{
    [ExcludeFromCodeCoverage]
    public class CalendarOutputFormatterTest
    {
        private Calendar CreateTestObject()
        {
            var calendar = new Calendar();
            var random = new Random();
            for (var i = 0; i < 10; ++i)
            {
                calendar.Events.Add(new CalendarEvent
                {
                    Class = "PUBLIC",
                    Summary = Guid.NewGuid().ToString(),
                    Created = new CalDateTime(DateTime.Now),
                    Description = Guid.NewGuid().ToString(),
                    Start = new CalDateTime(DateTime.UtcNow + TimeSpan.FromMinutes(random.Next(0, 60))),
                    End = new CalDateTime(DateTime.UtcNow + TimeSpan.FromHours(random.Next(1, 24))),
                    Sequence = 0,
                    Uid = Guid.NewGuid().ToString(),
                    Location = Guid.NewGuid().ToString(),
                });
            }
            return calendar;
        }
        
        [Fact]
        public async Task ShouldCreateSerializedString()
        {
            var loggerMock = new Mock<ILogger<CalendarOutputFormatter>>();
            var formatter = new CalendarOutputFormatter(loggerMock.Object);
            var httpContext = new DefaultHttpContext();
            var context = new OutputFormatterWriteContext(httpContext, 
                (stream, encoding) => new StreamWriter(stream, encoding), 
                typeof(Calendar),
                CreateTestObject());
            await formatter.WriteResponseBodyAsync(context, Encoding.UTF8);
            // TODO Find a way to read the response and check to value
        }
        
        [Fact]
        public async Task ShouldReturnEmptyStringOnTypeMismatch()
        {
            var loggerMock = new Mock<ILogger<CalendarOutputFormatter>>();
            var formatter = new CalendarOutputFormatter(loggerMock.Object);
            var httpContext = new DefaultHttpContext();
            var context = new OutputFormatterWriteContext(httpContext, 
                (stream, encoding) => new StreamWriter(stream, encoding), 
                typeof(string),
                "");
            await formatter.WriteResponseBodyAsync(context, Encoding.UTF8);
            // TODO Find a way to read the response and check to value
        }
    }
}