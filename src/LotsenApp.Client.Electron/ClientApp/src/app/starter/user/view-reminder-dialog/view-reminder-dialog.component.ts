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

import { Component, Inject } from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogRef,
} from '@angular/material/dialog';
import { Reminder } from '../../core/reminder';
import { DeleteReminderDialogComponent } from '../delete-reminder-dialog/delete-reminder-dialog.component';
import { EditableReminder } from '../reminder-list/editable-reminder';
import { Router } from '@angular/router';
import { ReminderService } from '../../core/reminder.service';

@Component({
  selector: 'la2-view-reminder-dialog',
  templateUrl: './view-reminder-dialog.component.html',
  styleUrls: ['./view-reminder-dialog.component.scss'],
})
export class ViewReminderDialogComponent {
  edit = false;
  editCopy: EditableReminder;

  constructor(
    public dialogRef: MatDialogRef<ViewReminderDialogComponent>,
    public dialog: MatDialog,
    @Inject(MAT_DIALOG_DATA) public data: Reminder,
    private reminderService: ReminderService,
    private router: Router
  ) {
    this.editCopy = EditableReminder.FromReminder(data);
  }

  async deleteReminder(reminder: Reminder): Promise<void> {
    const dialogRef = this.dialog.open(DeleteReminderDialogComponent, {
      data: reminder,
    });

    const result = await dialogRef.afterClosed().toPromise();
    if (result) {
      await this.reminderService.deleteReminder(reminder.reminderId ?? '');
    }
    this.dialogRef.close();
  }

  editReminder(reminder: EditableReminder): void {
    reminder.edit = true;
    this.edit = true;
  }

  stopEditing(reminder: EditableReminder, newValue: Reminder): void {
    reminder.header = newValue.header;
    reminder.text = newValue.text;
    reminder.actionUrl = newValue.actionUrl;
    reminder.reminderDate = newValue.reminderDate;
    reminder.reminderId = newValue.reminderId;
    reminder.edit = false;
    this.edit = false;
  }

  async navigateToAction(url: string): Promise<void> {
    const parts = url.split('?');
    if (parts.length === 1) {
      await this.router.navigate(parts);
    }
    const path = parts[0];
    const parameters = parts[1].split('&').map((p) => p.split('='));
    const queryParameter: { [key: string]: string } = {};
    for (const parameter of parameters) {
      queryParameter[parameter[0]] = parameter[1];
    }
    await this.router.navigate([path], {
      queryParams: queryParameter,
    });
  }
}
