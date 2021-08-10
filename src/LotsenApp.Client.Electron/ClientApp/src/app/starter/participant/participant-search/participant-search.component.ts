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

import {Component, ElementRef, HostListener, Input, OnInit, ViewChild,} from '@angular/core';
import {FormControl} from '@angular/forms';
import {SearchChip} from '../participants-overview/search-chip';
import {COMMA, ENTER, SEMICOLON} from '@angular/cdk/keycodes';
import {IParticipant} from '../iparticipant';
import {MatChipInputEvent} from '@angular/material/chips';
import {ParticipantService} from "../participant.service";
import {HeaderEntryDto} from "../header-entry-dto";
import {HeaderValueDto} from "../header-value-dto";
import {DataType} from "../data-type.enum";
import {UserConfiguration} from "../../core/user-configuration";

@Component({
  selector: 'la2-participant-search',
  templateUrl: './participant-search.component.html',
  styleUrls: ['./participant-search.component.scss'],
})
export class ParticipantSearchComponent implements OnInit {
  private internalList: IParticipant[] = [];

  DataType = DataType;

  searchControl = new FormControl('', []);
  searchChips: SearchChip[] = [];
  searchSeparator: number[] = [COMMA, SEMICOLON, ENTER];
  filteredParticipants: IParticipant[] = [];
  headerEntries: HeaderEntryDto[] = [];

  @ViewChild('searchField')
  searchField!: ElementRef;

  @Input()
  set participants(value: IParticipant[]) {
    this.internalList = value;
    this.filterParticipants();
  }

  @Input()
  configuration?: UserConfiguration;

  constructor(private participantService: ParticipantService) {}

  async ngOnInit() {
    this.headerEntries = await this.participantService.GetHeaderEntryDtos();
  }

  @HostListener('window:keydown', ['$event'])
  handleKeyDown(event: KeyboardEvent): void {
    if (event.ctrlKey && event.key === 'f') {
      this.searchField.nativeElement.focus();
    }
  }

  resetSearch(): void {
    this.searchControl.setValue('');
    this.searchChips = [];
    this.filterParticipants();
  }

  removeSearchPhrase(phrase: SearchChip): void {
    const index = this.searchChips.indexOf(phrase);
    this.searchChips.splice(index, 1);
    this.filterParticipants();
  }

  addSearchPhrase(event: MatChipInputEvent): void {
    const rawValue = event.value;
    const keyValue = rawValue.split('=');
    if (keyValue.length === 1) {
      keyValue.unshift('common');
    }
    this.searchChips.push({
      key: keyValue[0].trim().toLowerCase(),
      value: keyValue[1].trim().toLowerCase(),
    });
    this.searchControl.setValue('');
    this.filterParticipants();
  }

  private filterParticipants(): void {
    let filteredParticipants = this.internalList;
    for (const chip of this.searchChips) {
      filteredParticipants = filteredParticipants.filter((p) => {
        switch (chip.key.toLowerCase()) {
          case 'common':
            const participantHeaderKeys = Object.keys(p.header);
            return (
              p.id.toLowerCase().startsWith(chip.value) ||
              p.onlineId?.toLowerCase().startsWith(chip.value) ||
              p.projectId?.toLowerCase().startsWith(chip.value) ||
              p.createdAt.toLowerCase().startsWith(chip.value) ||
              p.synchronizedAt.toLowerCase().startsWith(chip.value) ||
              (p.synchronized && chip.value === 'true') ||
              (!p.synchronized && chip.value === 'false') ||
              participantHeaderKeys.some((h) =>
                p.header[h]
                  .join(' ')
                  .split(' ')
                  .some((c) => c.toLowerCase().startsWith(chip.value))
              )
            );
          case 'id':
            return p.id.startsWith(chip.value);
          case 'onlineid':
            return p.onlineId?.startsWith(chip.value);
          case 'projectid':
            return p.projectId?.startsWith(chip.value);
          case 'synchronized':
            return p.synchronized === (chip.value === 'true');
          case 'synchronizedat':
            return p.synchronizedAt.startsWith(chip.value);
          case 'createdat':
            return p.createdAt.startsWith(chip.value);
          default:
            return p.header[chip.key]?.join(' ')
              .split(' ')
              .some((h) => h.startsWith(chip.value));
        }
      });
    }

    this.filteredParticipants = filteredParticipants;
  }

  addChip(field: HeaderEntryDto, value: HeaderValueDto) {
    let displayValue = value.i18NKey ?? value.value;
    if(field.dataType === DataType.BOOLEAN) {
      displayValue = value.value === 'true' ? 'Application.BooleanEditor.Yes' : 'Application.BooleanEditor.No';
    }
    this.searchChips.push({
      key: field.fieldId.trim().toLowerCase(),
      displayKey: field.i18NKey ?? field.name,
      value: value.value,
      displayValue: displayValue
    });
    this.filterParticipants();
  }
}
