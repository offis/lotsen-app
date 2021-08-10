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

import { Reminder } from '../../core/reminder';

export class EditableReminder implements Reminder {
  actionUrl: string | null = null;
  edit: boolean = false;
  header: string = '';
  reminderDate: string = '';
  reminderDateEnd: string = '';
  reminderId: string | null = null;
  text: string = '';

  private constructor(reminder: Reminder) {
    this.actionUrl = (reminder.actionUrl || reminder.ActionUrl) ?? null;
    this.header = (reminder.header || reminder.Header) ?? '';
    this.reminderDate = (reminder.reminderDate || reminder.ReminderDate) ?? '';
    this.reminderDateEnd =
      (reminder.reminderDateEnd || reminder.ReminderDateEnd) ?? '';
    this.reminderId = (reminder.reminderId || reminder.ReminderId) ?? null;
    this.text = (reminder.text || reminder.Text) ?? '';
  }

  static FromReminder(reminder: Reminder): EditableReminder {
    return new EditableReminder(reminder);
  }
}
