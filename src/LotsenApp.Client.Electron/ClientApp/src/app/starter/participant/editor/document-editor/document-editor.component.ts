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
  HostListener,
  Input,
  OnDestroy,
  OnInit,
  QueryList,
  ViewChild,
  ViewChildren,
} from '@angular/core';
import { DocumentDto } from '../../document-dto';
import { ParticipantService } from '../../participant.service';
import { DocumentDetail } from '../../document-detail';
import { DocumentValue } from '../../document-value';
import { FieldDetailValue } from './field-detail-value';
import { IParticipant } from '../../iparticipant';
import { FieldDetail } from '../../field-detail';
import { FieldEditorComponent } from '../field-editor/field-editor.component';
import { Subscription } from 'rxjs';
import { FormControl } from '@angular/forms';
import { MatInput } from '@angular/material/input';
import { GroupDetailValue } from './group-detail-value';
import { MatAccordion } from '@angular/material/expansion';
import { ProjectService } from '../../project.service';
import { GroupValue } from '../../group-value';

@Component({
  selector: 'la2-document-editor',
  templateUrl: './document-editor.component.html',
  styleUrls: ['./document-editor.component.scss'],
})
export class DocumentEditorComponent implements OnInit, OnDestroy {
  @Input()
  participant!: IParticipant;
  @Input()
  metadata!: DocumentDto;

  @ViewChildren(FieldEditorComponent)
  fieldEditors!: QueryList<FieldEditorComponent>;
  @ViewChild('name')
  nameField!: MatInput;
  @ViewChild('accordion')
  accordion!: MatAccordion;

  metadataDetail?: DocumentDetail;
  values?: DocumentValue;
  editMode = false;
  nameControl = new FormControl('');

  mergedValues: FieldDetailValue[] = [];
  mergedGroups: GroupDetailValue[] = [];

  private subscriptions: Subscription[] = [];

  constructor(
    private participantService: ParticipantService,
    private projectService: ProjectService
  ) {}

  async ngOnInit(): Promise<void> {
    await this.setup();
    this.subscriptions.push(
      this.participantService.DocumentsSaved.subscribe(async (next) => {
        if (this.participant.id !== next) {
          return;
        }
        await this.setup();
      }),
      this.nameControl.valueChanges.subscribe((next) => {
        if (this.values) {
          this.values.name = next;
        }
        // Propagate name changes
        this.participantService.RenameDocument(
          this.participant.id,
          this.metadata.id,
          next
        );
      }),
      this.participantService.BeforeSaving.subscribe(async (next) => {
        if (this.participant.id !== next) {
          return;
        }
        await this.autoSave();
      }),
      this.participantService.DocumentUpdated.subscribe(async (next) => {
        if (next === this.metadata.id) {
          await this.setup();
        }
      })
    );
  }

  async ngOnDestroy(): Promise<void> {
    this.subscriptions.forEach((s) => s.unsubscribe());
    await this.autoSave();
  }

  @HostListener('window:beforeunload')
  async autoSave(): Promise<void> {
    if (!this.values) {
      return;
    }
    // @ts-ignore undefined is filtered
    this.values.fields = this.mergedValues
      .map((mv) => mv.value)
      .filter((v) => v !== undefined);
    this.values.groups = this.mergedGroups
      .map((gv) => gv.values ?? [])
      .reduce((prev, cur) => prev.concat(cur), []);
    await this.participantService.SaveChanges(this.participant.id, this.values);
    console.debug(
      'Saved changes to document ',
      this.metadata.id,
      ' for participant ',
      this.participant.id
    );
  }

  private async setup(): Promise<void> {
    const detail = await this.projectService.GetDocumentMetaData(
      this.participant.documentedBy,
      this.metadata.documentId
    );
    if (!detail.id) {
      // A documentation event was opened
      console.error('Opening this document is not supported as of right now');
      return;
    }
    const values = await this.participantService.GetDocumentValues(
      this.participant.id,
      this.participant.documentedBy,
      this.metadata.id
    );

    this.values = values;
    this.metadataDetail = detail;

    this.mergedValues = detail.fields.map((f) => {
      return {
        detail: f,
        value: this.values?.fields.find((v) => v.id === f.id),
      };
    });
    this.mergedGroups = detail.groups.map((g) => {
      return {
        detail: g,
        values: this.values?.groups.filter((gv) => gv.groupId === g.id) ?? [],
      };
    });

    this.nameControl.setValue(values.name);

    setTimeout(() => {
      this.accordion?.openAll();
    }, 100);
  }

  focusNextField(event: { field: FieldDetail; direction: number }): void {
    const currentIndex = this.metadataDetail?.fields.indexOf(event.field) ?? -1;
    if (currentIndex === -1) {
      return;
    }
    let nextIndex = (currentIndex + event.direction) % this.fieldEditors.length;
    if (nextIndex === -1) {
      nextIndex = this.fieldEditors.length - 1;
    }
    this.fieldEditors.filter((item, index) => index === nextIndex)[0].focus();
  }

  editName(): void {
    this.editMode = true;
    setTimeout(() => this.nameField.focus(), 100);
  }

  updateValue(event: KeyboardEvent): void {
    if (event.key === 'Enter' || event.key === 'Tab') {
      event.preventDefault();
      this.stopEditingName();
    }
  }

  stopEditingName(): void {
    this.editMode = false;
  }
}
