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
  OnDestroy,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { FieldDetail } from '../../field-detail';
import { FieldValue } from '../../field-value';
import { FormControl, ValidatorFn, Validators } from '@angular/forms';
import { Subscription } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { DataType } from '../../data-type.enum';
import { MatInput } from '@angular/material/input';

@Component({
  selector: 'la2-integer-editor',
  templateUrl: './integer-editor.component.html',
  styleUrls: ['./integer-editor.component.scss'],
})
export class IntegerEditorComponent implements OnInit, OnDestroy {
  @Input()
  fieldDetail!: FieldDetail;
  private internalFieldValue?: FieldValue;
  @Input()
  set fieldValue(value: FieldValue | undefined) {
    this.integerFormControl.setValue(value?.value);
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

  @ViewChild('integerText')
  integerText!: MatInput;

  integerFormControl = new FormControl();
  editorMode = false;
  applicable = true;
  labelText = '';

  private changeSubscription!: Subscription;

  constructor(private translate: TranslateService) {}

  ngOnInit(): void {
    if (this.fieldDetail.type.type !== DataType.INTEGER) {
      console.error(
        `The type ${this.fieldDetail.type.type} is not applicable for the integer control`
      );
      this.applicable = false;
      return;
    }
    const configuration = this.fieldDetail.type.values;
    // Parse the configuration string
    const configurationElements = configuration?.split(',');
    // TODO Parse mk3 additions
    const validators =
      configurationElements
        ?.map((e) => {
          const range = e.split('-');
          if (range.length > 1) {
            const min = Validators.min(+range[0]);
            const max = Validators.max(+range[1]);
            return [min, max];
          }
          return [] as ValidatorFn[];
        })
        .reduce((current, next) => current.concat(next)) ?? [];
    const integerValidator = Validators.pattern(/^-?\d+$/);
    validators?.push(integerValidator);
    this.integerFormControl.setValidators(validators);
    // Display values are not used by the string type
    this.integerFormControl.setValue(this.fieldValue?.value);
    // Emit value on change
    this.changeSubscription = this.integerFormControl.valueChanges.subscribe(
      (next) => {
        if (next === this.fieldValue?.value) {
          return;
        }
        this.internalFieldValue = {
          id: this.fieldDetail.id,
          value: `${next}`,
          isDelta: this.fieldValue?.isDelta ?? true,
        };
        this.fieldValueChange.emit(this.fieldValue);
      }
    );
    if (this.fieldDetail.i18NKey) {
      let translation = this.fieldDetail.i18NKey;
      translation = this.translate.instant(translation);
      this.labelText =
        translation === this.fieldDetail.i18NKey
          ? this.fieldDetail.name
          : translation;
    } else {
      this.labelText = this.fieldDetail.name;
    }
  }

  ngOnDestroy(): void {
    this.changeSubscription?.unsubscribe();
  }

  updateValue(event: KeyboardEvent): void {
    if (event.key === 'Enter' || event.key === 'Tab') {
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
      this.integerText.focus();
    }, 100);
  }

  stopEditing(): void {
    if (this.integerText?.focused) {
      return;
    }
    this.editorMode = false;
  }

  focus(): void {
    this.startEditing();
  }
}
