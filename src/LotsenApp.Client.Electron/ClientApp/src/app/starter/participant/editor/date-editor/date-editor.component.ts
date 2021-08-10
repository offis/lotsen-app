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
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { FieldDetail } from '../../field-detail';
import { FieldValue } from '../../field-value';
import { TranslateService } from '@ngx-translate/core';
import {
  MatDatepicker,
  MatDatepickerInputEvent,
  MatDatepickerToggle,
} from '@angular/material/datepicker';
import { MatInput } from '@angular/material/input';
import { UserConfigurationService } from '../../../core/user-configuration.service';

@Component({
  selector: 'la2-date-editor',
  templateUrl: './date-editor.component.html',
  styleUrls: ['./date-editor.component.scss'],
})
export class DateEditorComponent implements OnInit {
  private internalFieldValue?: FieldValue;

  @Input()
  fieldDetail!: FieldDetail;
  @Input()
  set fieldValue(value: FieldValue | undefined) {
    this.internalFieldValue = value;
    this.fieldValueChange.emit(value);
  }

  get fieldValue(): FieldValue | undefined {
    return this.internalFieldValue;
  }

  dateFormat = 'short';

  @Output()
  fieldValueChange = new EventEmitter<FieldValue>();

  @Output()
  valueCommit = new EventEmitter<{ field: FieldDetail; direction: number }>();

  @ViewChild(MatDatepicker)
  picker!: MatDatepicker<Date>;
  @ViewChild(MatInput)
  input!: MatInput;
  @ViewChild(MatDatepickerToggle)
  toggle!: MatDatepickerToggle<any>;

  editorMode = false;
  get displayValue(): string | undefined {
    return this.currentDateValue?.toLocaleDateString();
  }
  get currentDateValue(): Date | undefined {
    return this.formatStringDate(this.fieldValue?.value);
  }

  constructor(private configurationService: UserConfigurationService) {}

  async ngOnInit() {
    const configuration =
      await this.configurationService.GetUserConfiguration();
    this.dateFormat = configuration.localisationConfiguration.dateFormat;
  }

  updateValue(event: MatDatepickerInputEvent<unknown, unknown>): void {
    const date = event.value as Date;
    this.fieldValueChange.emit({
      id: this.fieldDetail.id,
      value: this.formatDate(date),
      isDelta: this.fieldValue?.isDelta ?? true,
    });
  }

  formatDate(date: Date): string {
    return `${date.getFullYear()}-${(date.getMonth() + 1 + '').padStart(
      2,
      '0'
    )}-${(date.getDate() + '').padStart(2, '0')}T00:00:00`;
  }

  formatStringDate(date: string | undefined): Date | undefined {
    if (!date) {
      return undefined;
    }
    const parsedDate = new Date();
    parsedDate.setFullYear(
      +date.substr(0, 4),
      +date.substr(5, 2) - 1,
      +date.substr(8, 2)
    );
    parsedDate.setHours(0, 0, 0, 0);

    return parsedDate;
  }

  startEditing(): void {
    this.editorMode = true;
    setTimeout(() => this.input?.focus(), 100);
  }

  stopEditing(): void {
    if (this.picker?.opened || this.input?.focused) {
      return;
    }
    this.editorMode = false;
  }

  focus(): void {
    this.startEditing();
    setTimeout(() => this.picker.open(), 100);
  }

  changeValue(event: KeyboardEvent): void {
    if (event.key === 'Enter' || event.key === 'Tab') {
      this.stopEditing();
      this.valueCommit.emit({
        field: this.fieldDetail,
        direction: event.shiftKey ? -1 : 1,
      });
    }
  }
}
