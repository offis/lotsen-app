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
import { TranslateService } from '@ngx-translate/core';
import { DisplayValue } from './display-value';
import { FormControl } from '@angular/forms';
import { MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatInput } from '@angular/material/input';
import { BehaviorSubject, Observable, Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

@Component({
  selector: 'la2-enum-editor',
  templateUrl: './enum-editor.component.html',
  styleUrls: ['./enum-editor.component.scss'],
})
export class EnumEditorComponent implements OnInit, OnDestroy {
  Math = Math;
  @Input()
  fieldDetail!: FieldDetail;
  private internalFieldValue?: FieldValue;
  @Input()
  set fieldValue(value: FieldValue | undefined) {
    this.internalFieldValue = value;
    this.fieldValueChange.emit(value);
    // noinspection JSIgnoredPromiseFromCall
    this.setControlValue();
  }

  get fieldValue(): FieldValue | undefined {
    return this.internalFieldValue;
  }

  @Output()
  fieldValueChange = new EventEmitter<FieldValue>();

  @Output()
  valueCommit = new EventEmitter<{ field: FieldDetail; direction: number }>();

  @ViewChild(MatInput)
  freeSelect!: MatInput;

  labelText = '';
  editorMode = false;

  options: string[] = [];
  get displayValue(): number {
    return this.fieldValue?.useDisplay ?? 0;
  }
  customControl = new FormControl(null);

  displayValues: DisplayValue[] = [];

  private internalDisplayValues: { [key: string]: string } = {};
  private internalResolvedDisplayValues: DisplayValue[] = [];
  private filter!: BehaviorSubject<DisplayValue[]>;
  private subscriptions: Subscription[] = [];
  get filteredDisplayValues(): Observable<DisplayValue[]> {
    return this.filter;
  }
  get selectedValue(): string {
    if (!this.fieldValue?.value) {
      return '';
    }
    const displayValue = this.displayValues.find(
      (d) => d.key === this.fieldValue?.value
    );
    if (displayValue) {
      return displayValue.values[this.displayValue];
    }
    return this.fieldValue?.value;
  }

  constructor(private translate: TranslateService) {}

  async ngOnInit() {
    this.options =
      this.fieldDetail.type.values?.split(',').map((v) => v.trim()) ?? [];
    this.options = this.options.filter((o) => o !== 'custom');
    const displayValuesDetail = this.fieldDetail.type.displayValues ?? {};
    if(Object.keys(displayValuesDetail).length > 0) {
      this.displayValues = this.options.map((o) => {
        return {
          key: o,
          values: displayValuesDetail[o],
        };
      });
    } else {
      this.displayValues = this.options.map((o) => {
        return {
          key: o,
          values: [o],
        };
      });
    }
    this.filter = new BehaviorSubject<DisplayValue[]>(this.displayValues);
    await this.setControlValue();
    this.updateInternalDisplayValues();
    this.subscriptions.push(
      this.translate.onLangChange.subscribe((_) => {
        this.updateInternalDisplayValues();
      }),
      this.customControl.valueChanges
        .pipe(debounceTime(300))
        .subscribe((value) => {
          if (
            !value ||
            this.displayValues.some((v) =>
              v.values.some((dv) => this.translate.instant(dv) === value)
            )
          ) {
            this.filter.next(this.displayValues);
            return;
          }
          const newFilter = this.displayValues.filter((v) => {
            return this.internalDisplayValues[v.key].includes(value);
          });
          this.filter.next(newFilter);
        })
    );
  }

  ngOnDestroy() {
    this.subscriptions.forEach((s) => s.unsubscribe());
  }

  private updateInternalDisplayValues() {
    this.internalResolvedDisplayValues = this.displayValues.map((d) => {
      return {
        key: d.key,
        values: d.values.map((v) => this.translate.instant(v)),
      };
    });
    for (const displayValue of this.internalResolvedDisplayValues) {
      this.internalDisplayValues[displayValue.key] =
        displayValue.values.join(' ');
    }
  }

  async setControlValue() {
    if (this.selectedValue) {
      this.customControl.setValue(
        await this.translate.get(this.selectedValue).toPromise()
      );
    }
  }

  getTranslationOrDefault(key: string, defaultValue: string): string {
    if (!key) {
      return defaultValue;
    }
    let translation = key;
    translation = this.translate.instant(translation);
    return translation === key ? defaultValue : translation;
  }

  updateValue() {
    const displayValue = this.internalResolvedDisplayValues.find((d) =>
      d.values.some((v) => v === this.customControl.value)
    );
    this.fieldValueChange.emit({
      id: this.fieldDetail.id,
      value: displayValue?.key ?? this.customControl.value,
      useDisplay: this.fieldValue?.useDisplay,
      isDelta: this.fieldValue?.isDelta ?? true,
    });
  }

  startEditing(): void {
    this.editorMode = true;
    setTimeout(() => {
      this.freeSelect?.focus();
    }, 100);
  }

  stopEditing(): void {
    if (this.freeSelect?.focused) {
      return;
    }
    this.editorMode = false;
  }

  focus(): void {
    this.startEditing();
  }

  changeValue(event: KeyboardEvent) {
    if (event.key === 'Enter' || event.key === 'Tab') {
      // event.preventDefault();
      this.updateValue();
      this.stopEditing();
      this.valueCommit.emit({
        field: this.fieldDetail,
        direction: event.shiftKey ? -1 : 1,
      });
    } else {
      this.updateValue();
    }
  }

  async selectValue(_: MatAutocompleteSelectedEvent) {
    await this.updateValue();
    this.stopEditing();
  }
}
