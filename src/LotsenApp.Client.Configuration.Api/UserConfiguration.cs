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

namespace LotsenApp.Client.Configuration.Api
{
    public class UserConfiguration
    {
        public string UserId { get; set; }
        public bool FirstSignIn { get; set; } = true;
        public string EncryptedPrivateKeyByDataPassword { get; set; }
        public string EncryptedPrivateKeyByRecoveryKey { get; set; }
        public string EncryptedDataKey { get; set; }
        public string HashedDataPassword { get; set; }

        public DashboardConfiguration[] DashboardConfigurations { get; set; } = 
        {
            new()
            {
                X = 0,
                Y = 0,
                Cols = 1,
                Rows = 1,
                Component = DashboardComponentType.Programmes
            },
            new()
            {
                X = 0,
                Y = 1,
                Cols = 1,
                Rows = 3,
                Component = DashboardComponentType.Reminder
            },
            new()
            {
                X = 1,
                Y = 0,
                Cols = 3,
                Rows = 4,
                Component = DashboardComponentType.ParticipantList
            }
        };

        public ProgrammeDefinition[] ProgrammeConfiguration { get; set; } = Array.Empty<ProgrammeDefinition>();

        public ReminderConfiguration ReminderConfiguration { get; set; } = new ReminderConfiguration();

        public DisplayConfiguration DisplayConfiguration { get; set; } = new DisplayConfiguration();
        
        public EditorConfiguration EditorConfiguration { get; set; }= new EditorConfiguration();

        public LocalisationConfiguration LocalisationConfiguration { get; set; } = new LocalisationConfiguration();
        
        public NotificationConfiguration NotificationConfiguration { get; set; } = new NotificationConfiguration();

        public SaveConfiguration SaveConfiguration { get; set; } = new SaveConfiguration();
        public SynchronisationConfiguration SynchronisationConfiguration { get; set; } =
            new SynchronisationConfiguration();

        public UpdateConfiguration UpdateConfiguration { get; set; } = new UpdateConfiguration();
    }
}