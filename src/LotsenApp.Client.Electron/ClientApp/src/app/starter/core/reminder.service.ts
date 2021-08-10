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
import { UserService } from './user.service';
import { HttpClient } from '@angular/common/http';
import { Reminder } from './reminder';
import { IdResponse } from '../participant/id-response';
import { Observable, Subject } from 'rxjs';
import { Router } from '@angular/router';
import { UserConfigurationService } from './user-configuration.service';

@Injectable({
  providedIn: 'root',
})
export class ReminderService {
  private _reminderCreated = new Subject<Reminder>();
  private _reminderDeleted = new Subject<string>();
  private _timeouts: number[] = [];

  public get ReminderCreated(): Observable<Reminder> {
    return this._reminderCreated;
  }

  public get ReminderDeleted(): Observable<string> {
    return this._reminderDeleted;
  }

  constructor(
    private userService: UserService,
    private http: HttpClient,
    private router: Router,
    private configurationService: UserConfigurationService
  ) {}

  public getReminder(
    startDate: Date | null = null,
    endDate: Date | null = null
  ): Promise<Reminder[]> {
    let queryString = startDate != null || endDate != null ? '?' : '';
    queryString +=
      startDate != null
        ? 'startDate=' + ReminderService.formatDate(startDate)
        : '';
    queryString += queryString.length > 1 ? '&' : '';
    queryString +=
      endDate != null ? 'endDate=' + ReminderService.formatDate(endDate) : '';

    return this.http.get<Reminder[]>('/api/reminder' + queryString).toPromise();
  }

  public async createReminder(reminder: Reminder): Promise<string> {
    const id = await this.http
      .post<IdResponse>('/api/reminder', reminder)
      .toPromise();
    this._reminderCreated.next(reminder);
    return id.id;
  }

  public async deleteReminder(id: string): Promise<string> {
    const response = await this.http
      .delete<IdResponse>('/api/reminder/' + id)
      .toPromise();
    this._reminderDeleted.next(id);
    return response.id;
  }

  public async updateReminder(reminder: Reminder): Promise<string> {
    const response = await this.http
      .put<IdResponse>('/api/reminder/', reminder)
      .toPromise();
    return response.id;
  }

  public clearReminderSchedule() {
    for (const timeout of this._timeouts) {
      clearTimeout(timeout);
    }
    this._timeouts = [];
  }

  public async scheduleNotifications(till: Date) {
    this.clearReminderSchedule();
    const now = new Date();
    const reminder = await this.getReminder(now, till);
    for (const r of reminder) {
      const startDate = ReminderService.formatStringDate(r.reminderDate);
      const millisecondsTillReminder = +startDate - +now;
      const body = r.text.length < 30 ? r.text : r.text.substr(0, 30) + '...';
      this._timeouts.push(
        // @ts-ignore
        setTimeout(async () => {
          const configuration =
            await this.configurationService.GetUserConfiguration();
          if (
            !(
              configuration.notificationConfiguration.displayNotifications &&
              configuration.notificationConfiguration.reminderNotifications
            )
          ) {
            return;
          }
          // create notification
          const notification = new Notification(r.header, {
            body: body,
          });
          if (r.actionUrl) {
            notification.onclick = (ev) => {
              this.router.navigate([r.actionUrl]);
            };
          }
        }, millisecondsTillReminder)
      );
    }
  }

  private static formatDate(date: Date | null): string {
    if (!date) {
      return '';
    }
    return `${date.getUTCFullYear()}-${(date.getUTCMonth() + 1 + '').padStart(
      2,
      '0'
    )}-${(date.getUTCDate() + '').padStart(2, '0')}T${(
      date.getUTCHours() + ''
    ).padStart(2, '0')}:${(date.getUTCMinutes() + '').padStart(2, '0')}:${(
      date.getUTCSeconds() + ''
    ).padStart(2, '0')}.0Z`;
  }

  private static formatStringDate(date: string): Date {
    const parsedDate = new Date();
    parsedDate.setUTCFullYear(
      +date.substr(0, 4),
      +date.substr(5, 2) - 1,
      +date.substr(8, 2)
    );

    parsedDate.setUTCHours(+date.substr(11, 2), +date.substr(14, 2), 0, 0);
    return parsedDate;
  }
}
