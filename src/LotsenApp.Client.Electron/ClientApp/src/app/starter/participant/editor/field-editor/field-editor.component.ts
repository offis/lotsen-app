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
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { FieldDetail } from '../../field-detail';
import { FieldValue } from '../../field-value';
import { DataType } from '../../data-type.enum';
import { StringEditorComponent } from '../string-editor/string-editor.component';
import { BooleanEditorComponent } from '../boolean-editor/boolean-editor.component';
import { EnumEditorComponent } from '../enum-editor/enum-editor.component';
import { DateEditorComponent } from '../date-editor/date-editor.component';
import { IntegerEditorComponent } from '../integer-editor/integer-editor.component';
import { DoubleEditorComponent } from '../double-editor/double-editor.component';
import { TranslateService } from '@ngx-translate/core';
import { MatDialog } from '@angular/material/dialog';
import { CreateReminderDialogComponent } from '../../../user/create-reminder-dialog/create-reminder-dialog.component';
import { DocumentValue } from '../../document-value';
import { IParticipant } from '../../iparticipant';
import { ParticipantService } from '../../participant.service';

@Component({
  selector: 'la2-field-editor',
  templateUrl: './field-editor.component.html',
  styleUrls: ['./field-editor.component.scss'],
})
export class FieldEditorComponent {
  currentValue: FieldValue | undefined;

  FieldType = DataType;
  @Input()
  field!: FieldDetail;
  @Input()
  participantPath: string = '';
  @Input()
  projectPath: string = '';

  @Input()
  set value(newValue: FieldValue | undefined) {
    this.valueChange.emit(newValue);
    this.currentValue = newValue;
  }
  get value(): FieldValue | undefined {
    return this.currentValue;
  }

  get hasDisplayValues(): boolean {
    return (
      (this.value &&
        this.field.type.displayValues &&
        this.field.type?.displayValues[this.value.value]?.length > 1) ??
      false
    );
  }

  get fieldDisplayValues(): string[] {
    if (this.value && this.field.type.displayValues) {
      return this.field.type.displayValues[this.value.value];
    }
    return [];
  }

  @Input()
  documentValue!: DocumentValue;
  @Input()
  participant!: IParticipant;

  @Output()
  valueChange = new EventEmitter<FieldValue>();

  @Output()
  valueCommit = new EventEmitter<{ field: FieldDetail; direction: number }>();

  @ViewChild(StringEditorComponent)
  stringEditor!: StringEditorComponent;
  @ViewChild(BooleanEditorComponent)
  booleanEditor!: BooleanEditorComponent;
  @ViewChild(EnumEditorComponent)
  enumEditor!: EnumEditorComponent;
  @ViewChild(DateEditorComponent)
  dateEditor!: DateEditorComponent;
  @ViewChild(IntegerEditorComponent)
  integerEditor!: IntegerEditorComponent;
  @ViewChild(DoubleEditorComponent)
  doubleEditor!: DoubleEditorComponent;
  @ViewChild('wrapper')
  wrapper!: ElementRef;

  editorMode = false;

  private focusInside = false;

  constructor(
    private translate: TranslateService,
    private dialog: MatDialog,
    private participantService: ParticipantService
  ) {}

  focusLost(event?: FocusEvent) {
    const anyEvent = event as any;
    this.focusInside =
      anyEvent.path.some((p: any) => p == this.wrapper.nativeElement) &&
      anyEvent.path[0] != this.wrapper.nativeElement;
    if (
      (anyEvent.relatedTarget?.className === 'container' &&
        event?.relatedTarget !== this.wrapper.nativeElement) ||
      (anyEvent.path[0] == this.wrapper.nativeElement &&
        event?.target != this.wrapper.nativeElement)
    ) {
      this.focusInside = false;
    }
  }

  focus(event?: FocusEvent): void {
    const anyEvent = event as any;
    if (
      !anyEvent?.path.some((p: any) => p == this.wrapper.nativeElement) ||
      this.focusInside
    ) {
      return;
    }
    this.focusInside = true;
    // event?.preventDefault();
    switch (this.field.type.type) {
      case DataType.STRING:
        this.stringEditor.focus();
        break;
      case DataType.BOOLEAN:
        this.booleanEditor.focus();
        break;
      case DataType.ENUMERABLE:
        this.enumEditor.focus();
        break;
      case DataType.DATE:
        this.dateEditor.focus();
        break;
      case DataType.INTEGER:
        this.integerEditor.focus();
        break;
      case DataType.DOUBLE:
        this.doubleEditor.focus();
        break;
      default:
        this.valueCommit.emit({ field: this.field, direction: 1 });
    }
  }

  commitValue(event: { field: FieldDetail; direction: number }): void {
    this.valueCommit.emit(event);
  }

  startEditing(): void {
    switch (this.field.type.type) {
      case DataType.STRING:
        this.stringEditor.startEditing();
        break;
      case DataType.BOOLEAN:
        this.booleanEditor.startEditing();
        break;
      case DataType.ENUMERABLE:
        this.enumEditor.startEditing();
        break;
      case DataType.DATE:
        this.dateEditor.startEditing();
        break;
      case DataType.INTEGER:
        this.integerEditor.startEditing();
        break;
      case DataType.DOUBLE:
        this.doubleEditor.startEditing();
        break;
      default:
        console.error('This field is in an invalid state');
    }
  }

  stopEditing(): void {
    switch (this.field.type.type) {
      case DataType.STRING:
        this.stringEditor.stopEditing();
        break;
      case DataType.BOOLEAN:
        this.booleanEditor.stopEditing();
        break;
      case DataType.ENUMERABLE:
        this.enumEditor.stopEditing();
        break;
      case DataType.DATE:
        this.dateEditor.stopEditing();
        break;
      case DataType.INTEGER:
        this.integerEditor.stopEditing();
        break;
      case DataType.DOUBLE:
        this.doubleEditor.stopEditing();
        break;
      default:
        console.error('This field is in an invalid state');
    }
  }

  async changeDisplayValue(index: number) {
    if (!this.value) {
      return;
    }
    this.value.useDisplay = index;
    if (this.field.type.type === DataType.ENUMERABLE) {
      await this.enumEditor.setControlValue();
    }
  }

  createReminder() {
    let labelText = '';
    if (this.field.i18NKey) {
      let translation = this.field.i18NKey;
      translation = this.translate.instant(translation);
      labelText =
        translation === this.field.i18NKey ? this.field.name : translation;
    } else {
      labelText = this.field.name;
    }
    const reminderTemplate = {
      header: `${this.participant.header.name} - ${labelText}`,
      actionUrl: `/participant/editor?open=${this.participant.id}&openDocument=${this.documentValue.id}`,
    };
    this.dialog.open(CreateReminderDialogComponent, {
      data: reminderTemplate,
    });
  }

  resetValue() {
    if (this.value) {
      this.value.value = '';
    }
    switch (this.field.type.type) {
      case DataType.STRING:
        if (this.stringEditor.fieldValue) {
          this.stringEditor.fieldValue.value = '';
        }
        this.stringEditor.fieldValue = this.stringEditor.fieldValue;
        break;
      case DataType.BOOLEAN:
        if (this.booleanEditor.fieldValue) {
          this.booleanEditor.fieldValue.value = '';
        }
        this.booleanEditor.fieldValue = this.booleanEditor.fieldValue;
        break;
      case DataType.ENUMERABLE:
        if (this.enumEditor.fieldValue) {
          this.enumEditor.fieldValue.value = '';
          this.enumEditor.customControl.setValue('');
        }
        this.enumEditor.fieldValue = this.enumEditor.fieldValue;
        break;
      case DataType.DATE:
        if (this.dateEditor.fieldValue) {
          this.dateEditor.fieldValue.value = '';
        }
        this.dateEditor.fieldValue = this.dateEditor.fieldValue;
        break;
      case DataType.INTEGER:
        if (this.integerEditor.fieldValue) {
          this.integerEditor.fieldValue.value = '';
        }
        this.integerEditor.fieldValue = this.integerEditor.fieldValue;
        break;
      case DataType.DOUBLE:
        if (this.doubleEditor.fieldValue) {
          this.doubleEditor.fieldValue.value = '';
        }
        this.doubleEditor.fieldValue = this.doubleEditor.fieldValue;
        break;
      default:
        console.error('This field is in an invalid state');
    }
  }

  async addToParticipantHeader() {
    const participantSplit = this.participantPath.split('.');
    const participantId = participantSplit.shift();
    const path = participantSplit.join('.');
    await this.participantService.AddHeaderEntry({
      participantId,
      path,
    });
  }

  async addToProjectHeader() {
    const projectSplit = this.projectPath.split('.');
    const projectId = projectSplit.shift();
    const path = projectSplit.join('.');
    await this.participantService.AddHeaderEntry({
      projectId,
      path,
    });
  }

  async removeFromParticipantHeader() {
    const participantSplit = this.participantPath.split('.');
    const participantId = participantSplit.shift();
    const path = participantSplit.join('.');
    await this.participantService.RemoveHeaderEntry({
      participantId,
      path,
    });
  }

  async removeFromProjectHeader() {
    const projectSplit = this.projectPath.split('.');
    const projectId = projectSplit.shift();
    const path = projectSplit.join('.');
    await this.participantService.RemoveHeaderEntry({
      projectId,
      path,
    });
  }
}
