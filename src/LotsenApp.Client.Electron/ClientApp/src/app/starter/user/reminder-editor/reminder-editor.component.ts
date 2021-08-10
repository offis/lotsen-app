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

import {
  AfterViewInit,
  Component,
  ElementRef,
  Input,
  OnDestroy,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import Editor from '@toast-ui/editor';
import {ReminderService} from '../../core/reminder.service';
import {AbstractControl, FormControl, Validators} from '@angular/forms';
import {Reminder} from '../../core/reminder';
import {EventEmitter} from '@angular/core';
import {Subscription} from 'rxjs';

@Component({
  selector: 'la2-reminder-editor',
  templateUrl: './reminder-editor.component.html',
  styleUrls: ['./reminder-editor.component.scss'],
})
export class ReminderEditorComponent
  implements OnInit, AfterViewInit, OnDestroy {
  constructor(private reminderService: ReminderService) {
  }

  titleControl = new FormControl(null, Validators.required);
  timeControl = new FormControl(null, Validators.required);
  timeEndControl = new FormControl(null, [
    Validators.required,
    (control: AbstractControl) => {
      const endTime = control.value as string;
      const endDate = this.dateEndControl?.value as Date;
      const startTime = this.timeControl?.value as string;
      const startDate = this.dateControl?.value as Date;

      const startTimestamp = ReminderEditorComponent.formatDateString(
        startDate,
        startTime
      );
      const endTimestamp = ReminderEditorComponent.formatDateString(
        endDate,
        endTime
      );

      if (endTimestamp < startTimestamp) {
        return {
          timestampInPast: true,
        };
      }
      return null;
    },
  ]);
  dateControl = new FormControl(null, Validators.required);
  dateEndControl = new FormControl(null, [
    Validators.required,
    (control: AbstractControl) => {
      const end = new Date((control?.value as Date)?.getTime() ?? 0);
      const start = new Date((this.dateControl?.value as Date)?.getTime() ?? 0);
      end?.setHours(0, 0, 0, 0);
      start?.setHours(0, 0, 0, 0);
      if (end < start) {
        console.log('End is smaller than start: ', end, start);
        return {
          endDateInPast: true,
        };
      }
      return null;
    },
  ]);
  createButtonDisabled = false;

  @ViewChild('textEditor')
  textEditor!: ElementRef;

  @Input()
  actionUrl?: string | null = null;
  @Input()
  reminderId?: string | null = null;
  @Input()
  reminderDate?: string | null = null;
  @Input()
  reminderDateEnd?: string | null = null;
  @Input()
  title?: string | null = null;
  @Input()
  description?: string | null = null;
  @Output()
  reminderCreated = new EventEmitter<Reminder>();

  private editor!: Editor;
  private subscriptions: Subscription[] = [];
  private items = [
    ['heading',
      'bold',
      'italic',
      'strike',
    ],
    ['hr',
      'quote',
    ],
    [
      'ul',
      'ol',
      // 'task',
      'indent',
      'outdent'
    ],
    [
      'table',
      // 'image',
      // 'link',
    ],
    [
      'code',
      'codeblock',
    ]
  ];

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

  private static formatDateString(
    date: Date | null,
    time: string | null
  ): string {
    if (!date) {
      return '';
    }
    const utcDateString = `${date.getUTCFullYear()}-${(
      date.getUTCMonth() +
      1 +
      ''
    ).padStart(2, '0')}-${(date.getUTCDate() + '').padStart(2, '0')}`;
    const regex = /(\d+):(\d+)/;
    const match = time?.match(regex);

    if (!match || (match?.length ?? 0) < 3) {
      return utcDateString;
    }
    const hours = +match[1];
    const minutes = +match[2];

    date.setHours(hours, minutes);

    const timeString = `${(date.getUTCHours() + '').padStart(2, '0')}:${(
      date.getUTCMinutes() + ''
    ).padStart(2, '0')}`;

    return `${utcDateString}T${timeString}:00.0Z`;
  }

  ngOnInit(): void {
    this.titleControl.setValue(this.title);
    this.subscriptions.push(
      this.dateControl.valueChanges.subscribe((next) => {
        this.dateEndControl.setValue(next);
      }),
      this.timeControl.valueChanges.subscribe((next) => {
        if (this.timeEndControl.value) {
          return;
        }
        const timeString = next as string;
        const serializedStartDate = ReminderEditorComponent.formatDateString(
          this.dateControl.value,
          timeString
        );
        const startDate =
          ReminderEditorComponent.formatStringDate(serializedStartDate);
        startDate.setMinutes(startDate.getMinutes() + 15);
        this.timeEndControl.setValue(
          `${startDate.getHours()}`.padStart(2, '0') +
          ':' +
          `${startDate.getMinutes()}`.padStart(2, '0')
        );
        if (
          this.dateEndControl.value === this.dateControl.value ||
          !this.dateEndControl.value
        ) {
          this.dateEndControl.setValue(startDate);
        }
      })
    );

    if (this.reminderDate) {
      const date = ReminderEditorComponent.formatStringDate(this.reminderDate);
      this.dateControl.setValue(date);
      this.timeControl.setValue(`${date.getHours()}:${date.getMinutes()}`);
    }
    if (this.reminderDateEnd) {
      const date = ReminderEditorComponent.formatStringDate(
        this.reminderDateEnd
      );
      this.dateEndControl.setValue(date);
      this.timeEndControl.setValue(`${date.getHours()}:${date.getMinutes()}`);
    }
  }

  ngAfterViewInit(): void {
    this.editor = new Editor({
      el: this.textEditor.nativeElement,
      height: 'auto',
      initialValue: this.description ?? '',
      initialEditType: 'wysiwyg',
      previewStyle: 'tab',
      usageStatistics: false,
      toolbarItems: this.items,
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach((s) => s.unsubscribe());
  }

  async saveReminder(): Promise<void> {
    this.timeEndControl.updateValueAndValidity();
    this.dateEndControl.updateValueAndValidity();
    if (
      this.titleControl.errors !== null ||
      this.dateControl.errors !== null ||
      this.timeControl.errors !== null ||
      this.dateEndControl.errors !== null ||
      this.timeEndControl.errors !== null
    ) {
      return;
    }
    this.createButtonDisabled = true;
    try {
      const reminder = {
        header: this.titleControl.value,
        reminderDate: ReminderEditorComponent.formatDateString(
          this.dateControl.value,
          this.timeControl.value
        ),
        reminderDateEnd: ReminderEditorComponent.formatDateString(
          this.dateEndControl.value,
          this.timeEndControl.value
        ),
        text: this.editor.getMarkdown(),
        reminderId: this.reminderId ?? null,
        actionUrl: this.actionUrl ?? null,
      };


      if (this.reminderId) {
        await this.reminderService.updateReminder(reminder);
        this.reminderCreated.emit(reminder);
        return;
      }
      reminder.reminderId = await this.reminderService.createReminder(reminder);
      this.reminderCreated.emit(reminder);
    } finally {
      this.createButtonDisabled = false;
    }
  }
}
