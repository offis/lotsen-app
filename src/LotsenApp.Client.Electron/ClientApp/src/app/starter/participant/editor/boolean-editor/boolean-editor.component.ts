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
import { MatSliderChange } from '@angular/material/slider';
import { TranslateService } from '@ngx-translate/core';
import {
  MatSlideToggle,
  MatSlideToggleChange,
} from '@angular/material/slide-toggle';

@Component({
  selector: 'la2-boolean-editor',
  templateUrl: './boolean-editor.component.html',
  styleUrls: ['./boolean-editor.component.scss'],
})
export class BooleanEditorComponent {
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

  @Output()
  fieldValueChange = new EventEmitter<FieldValue>();

  @Output()
  valueCommit = new EventEmitter<{ field: FieldDetail; direction: number }>();

  @ViewChild(MatSlideToggle)
  slider!: MatSlideToggle;

  editorMode = false;

  updateValue(event: MatSlideToggleChange): void {
    this.fieldValueChange.emit({
      id: this.fieldDetail.id,
      value: event.checked ? 'true' : 'false',
      isDelta: this.fieldValue?.isDelta ?? true,
    });
  }

  startEditing(): void {
    this.editorMode = true;
    setTimeout(() => this.slider?.focus(), 100);
  }

  stopEditing(): void {
    this.editorMode = false;
  }

  focus(): void {
    this.startEditing();
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
