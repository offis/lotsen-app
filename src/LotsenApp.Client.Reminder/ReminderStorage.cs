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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LotsenApp.Client.File;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;

namespace LotsenApp.Client.Reminder
{
    public class ReminderStorage
    {
        private const string Purpose = "LotsenApp.Client.Reminder.";
        private readonly IDictionary<string, Semaphore> _semaphores = new ConcurrentDictionary<string, Semaphore>();
        private readonly IDataProtectionProvider _provider;
        private readonly IFileService _fileService;

        public ReminderStorage(IDataProtectionProvider provider, IFileService fileService)
        {
            _provider = provider;
            _fileService = fileService;
        }

        public List<ReminderModel> GetReminder(string userId, bool write)
        {
            var encryptedReminder = GetReminderFileContent(userId, write);
            if (encryptedReminder == null)
            {
                return new List<ReminderModel>();
            }

            var protector = _provider.CreateProtector($"{Purpose}{userId}");
            var serializedReminder = protector.Unprotect(encryptedReminder);
            return JsonConvert.DeserializeObject<List<ReminderModel>>(serializedReminder);
        }

        public void SaveReminder(string userId, List<ReminderModel> reminder)
        {
            var serializedReminder = JsonConvert.SerializeObject(reminder);

            var protector = _provider.CreateProtector($"{Purpose}{userId}");
            var encryptedReminder = protector.Protect(serializedReminder);
            SaveReminderFile(userId, encryptedReminder);
        }
        
        private string GetReminderFileContent(string userId, bool write)
        {
            var reminderFile = GetReminderFile(userId);
            var semaphore = GetSemaphore(userId);
            semaphore.WaitOne();
            string content = null;
            if (System.IO.File.Exists(reminderFile))
            {
                content = System.IO.File.ReadAllText(reminderFile);
            }

            if (!write)
            {
                semaphore.Release(1);
            }

            return content;
        }

        private void SaveReminderFile(string userId, string serializedFile)
        {
            var reminderFile = GetReminderFile(userId);
            System.IO.File.WriteAllText(reminderFile, serializedFile);
            var semaphore = GetSemaphore(userId);
            semaphore.Release(1);
        }

        private string GetReminderFile(string userId)
        {
            const string fileName = "reminder.crypt";
            var relativePath = _fileService.Join($"data/{userId}");
            Directory.CreateDirectory(relativePath);
            return Path.Combine(relativePath, fileName);
        }

        private Semaphore GetSemaphore(string userId)
        {
            if (_semaphores.ContainsKey(userId))
            {
                return _semaphores[userId];
            }

            var semaphore = new Semaphore(0, 1);
            semaphore.Release(1);
            _semaphores.Add(userId, semaphore);
            return semaphore;
        }
    }
}