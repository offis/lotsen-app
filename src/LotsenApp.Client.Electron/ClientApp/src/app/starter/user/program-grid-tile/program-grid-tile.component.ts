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

import { Component, OnInit } from '@angular/core';
import { UserConfigurationService } from '../../core/user-configuration.service';
import { ProgrammeDefinition } from '../../core/programme-definition';
import { HttpClient } from '@angular/common/http';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { MatButton } from '@angular/material/button';
import { MatFormField } from '@angular/material/form-field';

@Component({
  selector: 'la2-program-grid-tile',
  templateUrl: './program-grid-tile.component.html',
  styleUrls: ['./program-grid-tile.component.scss'],
})
export class ProgramGridTileComponent implements OnInit {
  programmes: ProgrammeDefinition[] = [];

  constructor(
    private userConfigurationService: UserConfigurationService,
    private httpClient: HttpClient
  ) {}

  async ngOnInit(): Promise<void> {
    this.programmes =
      await this.userConfigurationService.GetProgrammeConfiguration();
  }

  async addItem(programmeDefinition: ProgrammeDefinition): Promise<void> {
    this.programmes.push(programmeDefinition);
    await this.userConfigurationService.UpdateProgrammeConfiguration(
      this.programmes
    );
  }

  async removeItem(index: number): Promise<void> {
    this.programmes.splice(index, 1);
    await this.userConfigurationService.UpdateProgrammeConfiguration(
      this.programmes
    );
  }

  async UpdateStorage(): Promise<void> {
    await this.userConfigurationService.UpdateProgrammeConfiguration(
      this.programmes
    );
  }

  editItemLabel(definition: ProgrammeDefinition): void {
    // startButton._elementRef.nativeElement.classList.add('hide');
    // editLabel._elementRef.nativeElement.classList.remove('hide');
    definition.edit = true;
    // editField.focus();
  }

  stopEditingLabel(
    event: KeyboardEvent | FocusEvent,
    definition: ProgrammeDefinition
  ): void {
    if (
      event instanceof FocusEvent ||
      (event instanceof KeyboardEvent &&
        (event.key === 'Tab' || event.key === 'Enter'))
    ) {
      // startButton._elementRef.nativeElement.classList.remove('hide');
      // editField._elementRef.nativeElement.classList.add('hide');
      definition.edit = false;
    }
  }

  async editItemPath(index: number, path: string): Promise<void> {
    this.programmes[index].path = path;
    await this.UpdateStorage();
  }

  async updateItem(): Promise<void> {}

  async readFile(event: Event, index?: number): Promise<void> {
    const fileUpload = event.target as any;
    if (fileUpload && fileUpload.files && fileUpload.files.length > 0) {
      const file = fileUpload.files[0];
      if (index || index === 0) {
        this.editItemPath(index, file.path);
        return;
      }
      const fileNameWithExtension = file.name;
      const extensionIndex = fileNameWithExtension.lastIndexOf('.');
      const fileName = fileNameWithExtension.substring(0, extensionIndex);
      await this.addItem({
        label: fileName,
        path: file.path,
      });
    }
  }

  async startProgramme(path: string): Promise<void> {
    await this.httpClient.post('/api/programme', { path }).toPromise();
  }

  async drop(event: CdkDragDrop<ProgrammeDefinition[]>): Promise<void> {
    moveItemInArray(this.programmes, event.previousIndex, event.currentIndex);
    await this.UpdateStorage();
  }

  stopPropagation(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
  }
}
