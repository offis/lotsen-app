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

using LotsenApp.Client.Configuration.Api;

namespace LotsenApp.Client.Configuration.Rest
{
    public sealed class UserConfigurationDto
    {
        public string UserId { get; set; }
        public DashboardConfiguration[] DashboardConfiguration { get; set; }
        public DisplayConfiguration DisplayConfiguration { get; set; }
        public EditorConfiguration EditorConfiguration { get; set; }
        public LocalisationConfiguration LocalisationConfiguration { get; set; } 
        public NotificationConfiguration NotificationConfiguration { get; set; }
        public ProgrammeDefinition[] ProgrammeConfiguration { get; set; }
        public ReminderConfiguration ReminderConfiguration { get; set; }
        public SaveConfiguration SaveConfiguration { get; set; }
        public SynchronisationConfiguration SynchronisationConfiguration { get; set; }
        public UpdateConfiguration UpdateConfiguration { get; set; }

        public UserConfigurationDto()
        {
            
        }

        public UserConfigurationDto(UserConfiguration userConfiguration)
        {
            DashboardConfiguration = userConfiguration.DashboardConfigurations;
            DisplayConfiguration = userConfiguration.DisplayConfiguration;
            EditorConfiguration = userConfiguration.EditorConfiguration;
            LocalisationConfiguration = userConfiguration.LocalisationConfiguration;
            NotificationConfiguration = userConfiguration.NotificationConfiguration;
            ProgrammeConfiguration = userConfiguration.ProgrammeConfiguration;
            ReminderConfiguration = userConfiguration.ReminderConfiguration;
            SaveConfiguration = userConfiguration.SaveConfiguration;
            SynchronisationConfiguration = userConfiguration.SynchronisationConfiguration;
            UpdateConfiguration = userConfiguration.UpdateConfiguration;
            UserId = userConfiguration.UserId;
        }

        public UserConfiguration Merge(UserConfiguration userConfiguration)
        {
            userConfiguration.DashboardConfigurations = DashboardConfiguration;
            userConfiguration.DisplayConfiguration = DisplayConfiguration;
            userConfiguration.EditorConfiguration = EditorConfiguration;
            userConfiguration.LocalisationConfiguration = LocalisationConfiguration;
            userConfiguration.NotificationConfiguration = NotificationConfiguration;
            userConfiguration.ProgrammeConfiguration = ProgrammeConfiguration;
            userConfiguration.ReminderConfiguration = ReminderConfiguration;
            userConfiguration.SaveConfiguration = SaveConfiguration;
            userConfiguration.SynchronisationConfiguration = SynchronisationConfiguration;
            userConfiguration.UpdateConfiguration = UpdateConfiguration;
            return userConfiguration;
        }
    }
}