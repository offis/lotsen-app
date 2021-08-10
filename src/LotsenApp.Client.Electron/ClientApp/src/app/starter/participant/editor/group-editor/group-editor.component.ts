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
} from '@angular/core';
import { GroupDetail } from '../../group-detail';
import { GroupValue } from '../../group-value';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { Cardinality } from '../../cardinality.enum';
import { ParticipantService } from '../../participant.service';
import { IParticipant } from '../../iparticipant';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { FieldDetailValue } from '../document-editor/field-detail-value';
import { GroupDetailValue } from '../document-editor/group-detail-value';
import { DocumentValue } from '../../document-value';

@Component({
  selector: 'la2-group-editor',
  templateUrl: './group-editor.component.html',
  styleUrls: ['./group-editor.component.scss'],
})
export class GroupEditorComponent implements OnInit {
  private internalGroupValues: GroupValue[] = [];

  Cardinality = Cardinality;

  @Input()
  detail!: GroupDetail;
  @Input()
  participantPath: string = '';
  @Input()
  projectPath: string = '';

  @Input()
  set values(values: GroupValue[]) {
    this.internalGroupValues = values;
    this.synchronizeGroupValues();
  }

  get values(): GroupValue[] {
    return this.internalGroupValues;
  }

  @Input()
  participant!: IParticipant;
  @Input()
  documentValues!: DocumentValue;
  @Input()
  documentId!: string;
  @Input()
  parentGroupId: string | undefined;

  @Output()
  valuesChange = new EventEmitter<GroupValue[]>();

  @Input()
  level = 0;

  mergedFields: FieldDetailValue[][] = [];
  mergedChildren: GroupDetailValue[][] = [];

  constructor(
    private translate: TranslateService,
    private participantService: ParticipantService
  ) {}

  async ngOnInit(): Promise<void> {
    this.synchronizeGroupValues();

    if (
      this.detail.cardinality === Cardinality.One &&
      this.values.length === 0
    ) {
      await this.addGroup();
    }
  }

  private synchronizeGroupValues() {
    this.mergedFields = [];
    this.mergedChildren = [];
    for (const value of this.values) {
      this.addGroupInternal(value);
    }
  }

  private addGroupInternal(value: GroupValue): void {
    this.mergedFields.push(
      this.detail.fields.map((f) => {
        return {
          detail: f,
          value: value.fields.find((fv) => fv.id === f.id),
        };
      })
    );
    this.mergedChildren.push(
      this.detail.children.map((c) => {
        return {
          detail: c,
          values: value.children.filter((gc) => gc.groupId === c.id),
        };
      })
    );
  }

  async addGroup(): Promise<void> {
    if (
      this.detail.cardinality === Cardinality.One &&
      this.values.length === 1
    ) {
      console.error('Creating a group would violate the data format');
      return;
    }
    const newId = await this.participantService.CreateGroup(
      this.participant.id,
      this.participant.documentedBy,
      this.documentId,
      this.detail.id,
      this.parentGroupId
    );
    const newValue = {
      id: newId.id,
      groupId: this.detail.id,
      isDelta: true,
      fields: [],
      children: [],
    };
    this.values.push(newValue);
    this.addGroupInternal(newValue);
    this.valuesChange.emit(this.values);
  }

  async removeGroup(value: GroupValue): Promise<void> {
    await this.participantService.RemoveGroup(
      this.participant.id,
      this.participant.documentedBy,
      this.documentId,
      value.id
    );
    this.values = this.values.filter((v) => v.id !== value.id);
  }

  async reorderGroups(
    event: CdkDragDrop<GroupValue[], GroupValue>
  ): Promise<void> {
    moveItemInArray(this.values, event.previousIndex, event.currentIndex);
    this.synchronizeGroupValues();
    this.valuesChange.emit(this.values);
    await this.participantService.ReorderGroup(
      this.participant.id,
      this.participant.documentedBy,
      this.documentId,
      this.values
    );
  }

  updateValue(index: number, field: FieldDetailValue): void {
    if (!field?.value) {
      return;
    }
    const existingField = this.values[index].fields.find(
      (f) => f.id === field.detail.id
    );
    if (!existingField) {
      this.values[index].fields.push(field.value);
    } else {
      existingField.value = field.value.value;
      existingField.isDelta = field.value.isDelta;
      existingField.useDisplay = field.value.useDisplay;
    }
    this.valuesChange.emit(this.values);
  }

  updateChildren(
    instanceIndex: number,
    groupIndex: number,
    group: GroupDetailValue
  ): void {
    if (!group.values) {
      return;
    }
    const value = group.values[groupIndex];
    const existingGroup = this.values[instanceIndex].children.find(
      (g) => g.id === value.id
    );
    // console.log(instanceIndex, groupIndex, existingGroup);
    // Special case: new group
    if (!existingGroup) {
      this.values[instanceIndex].children.push(group.values[groupIndex]);
    } else {
      // Regular case: existing groups have been updated. Discard the current model and use the new model instead
      this.values[instanceIndex].children = group.values;
      // existingGroup.fields = group.values[groupIndex].fields;
      // existingGroup.isDelta = group.values[groupIndex].isDelta;
      // existingGroup.children = group.values[groupIndex].children;
    }
    // console.log('Group editor', group, this.values);
    this.valuesChange.emit(this.values);
  }
}
