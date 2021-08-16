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

import { Component, Input, OnInit } from '@angular/core';
import { DocumentDto } from '../../document-dto';
import { ParticipantService } from '../../participant.service';
import { TranslateService } from '@ngx-translate/core';
import { IParticipant } from '../../iparticipant';
import { Router } from '@angular/router';
import { Displayable } from '../../displayable';
import { MatDialog } from '@angular/material/dialog';
import { DocumentDeleteConfirmComponent } from '../document-delete-confirm/document-delete-confirm.component';
import { ProjectService } from '../../project.service';

@Component({
  selector: 'la2-document-overview-display',
  templateUrl: './document-overview-display.component.html',
  styleUrls: ['./document-overview-display.component.scss'],
})
export class DocumentOverviewDisplayComponent implements OnInit {
  @Input()
  documentDto!: DocumentDto;

  @Input()
  participant!: IParticipant;

  @Input()
  similarDocuments: DocumentDto[] = [];

  get otherDocuments() {
    return this.similarDocuments.filter((d) => d.id !== this.documentDto.id);
  }

  subDocuments: Displayable[] = [];

  constructor(
    private participantService: ParticipantService,
    private projectService: ProjectService,
    private translateService: TranslateService,
    private router: Router,
    private dialog: MatDialog
  ) {}

  async ngOnInit(): Promise<void> {
    this.subDocuments = await this.projectService.GetSubDocuments(
      this.participant.documentedBy,
      this.documentDto.documentId
    );
  }

  getDocumentName(displayable: Displayable): string {
    let translation = displayable.i18NKey;
    if (displayable.i18NKey) {
      translation = this.translateService.instant(displayable.i18NKey);
    }
    return (
      (translation === displayable.i18NKey ? displayable.name : translation) ??
      ''
    );
  }

  async deleteDocument(dto: DocumentDto): Promise<void> {
    const ref = this.dialog.open(DocumentDeleteConfirmComponent, {
      data: {
        name: dto.name,
      },
    });
    const result = await ref.afterClosed().toPromise();
    if (result) {
      await this.participantService.DeleteDocument(dto, this.participant.id);
      await this.router.navigate(['/www/participant/editor'], {
        queryParams: {
          closeDocument: dto.id,
        },
      });
    }
  }

  async createDocument(displayable: Displayable): Promise<void> {
    const idResponse = await this.participantService.CreateDocument(
      displayable,
      this.participant.documentedBy,
      this.participant.id,
      this.getDocumentName(displayable),
      this.documentDto.id
    );
    await this.router.navigate(['/www/participant/editor'], {
      queryParams: {
        openDocument: idResponse.id,
      },
    });
  }

  async performAction(documentDto: DocumentDto) {
    console.log(documentDto);
    await this.preserveCopy(documentDto);
  }

  async preserveCopy(documentDto: DocumentDto) {
    await this.participantService.CopyDocumentValues(
      this.participant.id,
      this.documentDto.id,
      documentDto.id,
      true
    );
    console.log('PreserveCopy success');
  }

  async overwriteCopy(documentDto: DocumentDto) {
    await this.participantService.CopyDocumentValues(
      this.participant.id,
      this.documentDto.id,
      documentDto.id,
      false
    );
    console.log('OverwriteCopy success');
  }
}
