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
using LotsenApp.Client.File;
using Microsoft.AspNetCore.DataProtection;
using Moq;
using Xunit;

namespace LotsenApp.Client.Reminder.Test
{
    [ExcludeFromCodeCoverage]
    public class ReminderStorageTest
    {
        private readonly ReminderStorage _storage;

        public ReminderStorageTest()
        {
            var protectionMock = new Mock<IDataProtectionProvider>();
            IFileService fileService = new ScopedFileService
            {
                Root = $"./{Guid.NewGuid().ToString()}"
            };
            Assert.NotEqual("./", fileService.Root);
            fileService.EnsureCreated();
            SetupProtectionMock(protectionMock);
            _storage = new ReminderStorage(protectionMock.Object, fileService);
        }

        private void SetupProtectionMock(Mock<IDataProtectionProvider> mock)
        {
            var protectorMock = new Mock<IDataProtector>();
            SetupDataProtector(protectorMock);
            mock.Setup(p => p.CreateProtector(It.IsAny<string>()))
                .Returns(protectorMock.Object);
        }

        private void SetupDataProtector(Mock<IDataProtector> mock)
        {
            mock.Setup(p => p.Protect(It.IsAny<byte[]>()))
                .Returns((byte[] input) => input);
            
            mock.Setup(p => p.Unprotect(It.IsAny<byte[]>()))
                .Returns((byte[] input) => input);
        }
        
        [Fact]
        public void ShouldGetEmptyListWithNoReminderFile()
        {
            Assert.Empty(_storage.GetReminder("usr-id", false));
        }
        
        [Fact]
        public void ShouldSaveAndGetReminder()
        {
            var list = new List<ReminderModel>
            {
                new ReminderModel
                {
                    ReminderId = Guid.NewGuid().ToString(),
                    Header = "Some header",
                    ReminderDate = DateTime.UtcNow,
                    ReminderDateEnd = DateTime.UtcNow + TimeSpan.FromMinutes(15)
                }
            };
            _storage.GetReminder("usr-id", true);
            _storage.SaveReminder("usr-id", list);
            Assert.Single(_storage.GetReminder("usr-id", false));
        }
    }
}