/**
 * @license
 * Copyright (c) 2021 OFFIS e.V.. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 *
 * 3. Neither the name of the copyright holder nor the names of its contributors
 *    may be used to endorse or promote products derived from this software without
 *    specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
 * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { GridTile } from './grid-tile';
import { ProgrammeDefinition } from './programme-definition';
import { ReminderConfiguration } from './reminder-configuration';
import { UserConfiguration } from './user-configuration';

@Injectable({
  providedIn: 'root',
})
export class UserConfigurationService {
  constructor(private httpClient: HttpClient) {}

  public async GetUserConfiguration(): Promise<UserConfiguration> {
    return await this.httpClient
      .get<UserConfiguration>('/api/configuration/user')
      .toPromise();
  }

  public async SetUserConfiguration(
    configuration: UserConfiguration
  ): Promise<void> {
    await this.httpClient
      .post('/api/configuration/user', configuration)
      .toPromise();
  }

  public async GetDashboardConfiguration(): Promise<GridTile[]> {
    return (await this.GetUserConfiguration()).dashboardConfiguration;
  }

  public async UpdateDashboardConfiguration(
    configuration: GridTile[]
  ): Promise<void> {
    await this.httpClient
      .post('/api/configuration/user/dashboard', configuration)
      .toPromise();
  }

  public async GetProgrammeConfiguration(): Promise<ProgrammeDefinition[]> {
    return (await this.GetUserConfiguration()).programmeConfiguration;
  }

  public async UpdateProgrammeConfiguration(
    configuration: ProgrammeDefinition[]
  ): Promise<void> {
    await this.httpClient
      .post('/api/configuration/user/programme', configuration)
      .toPromise();
  }

  public async GetReminderConfiguration(): Promise<ReminderConfiguration> {
    return (await this.GetUserConfiguration()).reminderConfiguration;
  }

  public async UpdateReminderConfiguration(
    configuration: ReminderConfiguration
  ): Promise<void> {
    await this.httpClient
      .post('/api/configuration/user/reminder', configuration)
      .toPromise();
  }
}
