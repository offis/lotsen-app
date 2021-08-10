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

import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ReminderService } from '../../core/reminder.service';
import { Reminder } from '../../core/reminder';
import { Subscription } from 'rxjs';
import { ReminderGroup } from './reminder-group';
import { MatDialog } from '@angular/material/dialog';
import { EditableReminder } from './editable-reminder';
import { CreateReminderDialogComponent } from '../create-reminder-dialog/create-reminder-dialog.component';
import { Router } from '@angular/router';
import {
  CalendarOptions,
  EventApi,
  FullCalendarComponent,
  ViewApi,
} from '@fullcalendar/angular';
import deLocale from '@fullcalendar/core/locales/de';
import enGbLocale from '@fullcalendar/core/locales/en-gb';
import { TranslateService } from '@ngx-translate/core';
import { ViewReminderDialogComponent } from '../view-reminder-dialog/view-reminder-dialog.component';
import { DashboardService } from '../dashboard.service';
import { UserConfigurationService } from '../../core/user-configuration.service';
import { ReminderConfiguration } from '../../core/reminder-configuration';

@Component({
  selector: 'la2-reminder-list',
  templateUrl: './reminder-list.component.html',
  styleUrls: ['./reminder-list.component.scss'],
})
export class ReminderListComponent implements OnInit, OnDestroy {
  reminder: ReminderGroup[] = [];
  showCalendar = false;

  calendarOptions: CalendarOptions = {
    initialView: 'dayGridMonth',
    weekNumbers: true,
    nowIndicator: true,
    editable: true,
    headerToolbar: {
      left: 'today prev,next',
      center: 'title',
      right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek',
    },
    locales: [deLocale, enGbLocale],
    locale: 'de',
    events: {
      url: '/api/reminder/calendar.ics',
      format: 'ics',
    },
    // @ts-ignore
    dateClick: this.handleDateClick.bind(this),
    eventClick: this.handleEventClick.bind(this),
    viewClassNames: this.handleViewClassNames.bind(this),
    eventDrop: this.handleEventDrop.bind(this),
    eventResize: this.handleEventResize.bind(this),
  };

  @ViewChild('calendar')
  calendarComponent!: FullCalendarComponent;

  private subscriptions: Subscription[] = [];
  private reminderConfiguration!: ReminderConfiguration;

  constructor(
    private reminderService: ReminderService,
    private translate: TranslateService,
    public dialog: MatDialog,
    private dashboardService: DashboardService,
    private userConfigurationService: UserConfigurationService,
    private router: Router
  ) {}

  async ngOnInit(): Promise<void> {
    this.subscriptions.push(
      this.reminderService.ReminderCreated.subscribe(async (next) => {
        this.calendarComponent.getApi().refetchEvents();
      })
    );
    this.subscriptions.push(
      this.reminderService.ReminderDeleted.subscribe(async (next) => {
        this.calendarComponent.getApi().refetchEvents();
      }),
      this.translate.onLangChange.subscribe((next) => {
        this.calendarOptions.locale = next.lang;
      }),
      this.dashboardService.DashboardUpdated.subscribe((next) => {
        this.calendarComponent.getApi().updateSize();
      })
    );

    this.reminderConfiguration =
      await this.userConfigurationService.GetReminderConfiguration();
    this.calendarOptions.initialView =
      this.reminderConfiguration.displayType ?? 'dayGridMonth';
    this.calendarOptions.locale = this.translate.currentLang;
    setTimeout(() => (this.showCalendar = true), 300);
  }

  async ngOnDestroy(): Promise<void> {
    this.subscriptions.forEach((s) => s.unsubscribe());
    this.showCalendar = false;
  }

  async createReminder(): Promise<void> {
    const dialogRef = this.dialog.open(CreateReminderDialogComponent);
    const result = await dialogRef.afterClosed().toPromise();
    if (result) {
      this.calendarComponent.getApi().refetchEvents();
    }
  }

  handleDateClick(args: any): void {
    const mouseEvent = args.jsEvent as Event;
    // const date = args.date as Date;
    const dateString = ReminderListComponent.formatDateString(args.date);

    mouseEvent.preventDefault();
    mouseEvent.stopPropagation();
    this.dialog.open(CreateReminderDialogComponent, {
      data: {
        reminderDate: dateString,
      },
    });
  }

  async handleEventClick(args: any): Promise<void> {
    const mouseEvent = args.jsEvent as MouseEvent;
    mouseEvent.preventDefault();
    mouseEvent.stopPropagation();

    const event = args.event as EventApi;
    const reminder = JSON.parse(
      event._def.extendedProps.description
    ) as Reminder;
    if (mouseEvent.button === 2) {
      // TODO open menu?
    }

    if (mouseEvent.button === 0) {
      const subscription = this.dialog.open(ViewReminderDialogComponent, {
        data: reminder,
      });
      await subscription.afterClosed().toPromise();
      this.calendarComponent.getApi().refetchEvents();
    }
  }

  handleViewClassNames(args: any): any {
    const view = args.view as ViewApi;
    this.reminderConfiguration.displayType = view.type;
    this.userConfigurationService.UpdateReminderConfiguration(
      this.reminderConfiguration
    );
    return undefined;
  }

  async handleEventDrop(args: any): Promise<void> {
    const newEvent = args.event as EventApi;
    const serializedReminder = newEvent.extendedProps.description;
    const reminder = EditableReminder.FromReminder(
      JSON.parse(serializedReminder) as Reminder
    );
    reminder.reminderDate = ReminderListComponent.formatDateString(
      newEvent.start
    );
    reminder.reminderDateEnd = ReminderListComponent.formatDateString(
      newEvent.end
    );
    await this.reminderService.updateReminder(reminder);
  }

  async handleEventResize(args: any): Promise<void> {
    const newEvent = args.event as EventApi;
    const serializedReminder = newEvent.extendedProps.description;
    const reminder = EditableReminder.FromReminder(
      JSON.parse(serializedReminder) as Reminder
    );
    reminder.reminderDate = ReminderListComponent.formatDateString(
      newEvent.start
    );
    reminder.reminderDateEnd = ReminderListComponent.formatDateString(
      newEvent.end
    );
    await this.reminderService.updateReminder(reminder);
  }

  private static formatDateString(date: Date | null): string {
    if (!date) {
      return '';
    }
    const utcDateString = `${date.getUTCFullYear()}-${(
      date.getUTCMonth() +
      1 +
      ''
    ).padStart(2, '0')}-${(date.getUTCDate() + '').padStart(2, '0')}`;

    const timeString = `${(date.getUTCHours() + '').padStart(2, '0')}:${(
      date.getUTCMinutes() + ''
    ).padStart(2, '0')}`;

    return `${utcDateString}T${timeString}:00.0Z`;
  }

  stopPropagation(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
  }
}
