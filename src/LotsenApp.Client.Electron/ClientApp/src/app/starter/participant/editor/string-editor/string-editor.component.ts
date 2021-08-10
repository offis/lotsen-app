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
  ElementRef,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { DataType } from '../../data-type.enum';
import { EditorConfiguration } from './editor-configuration';
import { FormControl } from '@angular/forms';
import { Subscription } from 'rxjs';
import { FieldDetail } from '../../field-detail';
import { FieldValue } from '../../field-value';
import { TranslateService } from '@ngx-translate/core';
import { MatInput } from '@angular/material/input';

@Component({
  selector: 'la2-string-editor',
  templateUrl: './string-editor.component.html',
  styleUrls: ['./string-editor.component.scss'],
})
export class StringEditorComponent implements OnInit, OnDestroy {
  @Input()
  fieldDetail!: FieldDetail;

  private internalFieldValue?: FieldValue;
  @Input()
  set fieldValue(value: FieldValue | undefined) {
    if (this.stringFormControl.value !== value?.value) {
      this.stringFormControl.setValue(value?.value);
    }
    this.internalFieldValue = value;
    this.fieldValueChange.emit(value);
  }

  get fieldValue(): FieldValue | undefined {
    return this.internalFieldValue;
  }
  @Input()
  set editorMode(value: boolean) {
    this.internalEditorMode = value;
    this.editorModeChange.emit(value);
  }

  get editorMode(): boolean {
    return this.internalEditorMode;
  }

  @Output()
  fieldValueChange = new EventEmitter<FieldValue>();
  @Output()
  editorModeChange = new EventEmitter<boolean>();

  @Output()
  valueCommit = new EventEmitter<{ field: FieldDetail; direction: number }>();

  @ViewChild('smallText')
  smallText!: MatInput;
  @ViewChild('bigText')
  bigText!: MatInput;

  stringFormControl = new FormControl();
  isBigText = false;
  applicable = true;
  private internalEditorMode = false;

  private changeSubscription!: Subscription;
  constructor(private translate: TranslateService) {}

  ngOnInit(): void {
    if (this.fieldDetail.type.type !== DataType.STRING) {
      console.error(
        `The type ${this.fieldDetail.type.type} is not applicable for the string control`
      );
      this.applicable = false;
      return;
    }
    const configuration = this.fieldDetail.type.values;
    // Parse the configuration string
    const configurationElements: EditorConfiguration[] | undefined =
      configuration
        ?.split(';')
        .map((e) => e.split('='))
        .map((e) => {
          return {
            key: e[0],
            value: e.length === 2 ? e[1] : '',
          };
        });
    this.isBigText =
      configurationElements?.find((c) => c.key === 'size')?.value === 'large';
    // Display values are not used by the string type
    this.stringFormControl.setValue(this.fieldValue?.value ?? '');
    // Emit value on change
    this.changeSubscription = this.stringFormControl.valueChanges.subscribe(
      (next) => {
        this.fieldValue = {
          id: this.fieldDetail.id,
          value: this.stringFormControl.value,
          isDelta: this.fieldValue?.isDelta ?? true,
        };
        this.fieldValueChange.emit(this.fieldValue);
      }
    );
  }

  ngOnDestroy(): void {
    this.changeSubscription?.unsubscribe();
  }

  updateValue(event: KeyboardEvent): void {
    if (
      (event.key === 'Enter' && (event.shiftKey || !this.isBigText)) ||
      event.key === 'Tab'
    ) {
      event.preventDefault();
      this.editorMode = false;
      this.valueCommit.emit({
        field: this.fieldDetail,
        direction: event.shiftKey ? -1 : 1,
      });
    }
  }

  startEditing(): void {
    this.editorMode = true;
    setTimeout(() => {
      this.bigText?.focus();
      this.smallText?.focus();
    }, 100);
  }

  stopEditing(): void {
    if (this.bigText?.focused || this.smallText?.focused) {
      return;
    }
    this.editorMode = false;
  }

  focus(): void {
    this.startEditing();
  }
}
