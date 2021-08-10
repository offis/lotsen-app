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
} from '@angular/core';
import { DocumentDto } from '../../document-dto';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ParticipantService } from '../../participant.service';
import { UserService } from '../../../core/user.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subscription } from 'rxjs';
import { IParticipant } from '../../iparticipant';
import { SpecificDocumentDto } from '../../specific-document-dto';
import { UserConfiguration } from '../../../core/user-configuration';
import { UserConfigurationService } from '../../../core/user-configuration.service';

@Component({
  selector: 'la2-document-overview',
  templateUrl: './document-overview.component.html',
  styleUrls: ['./document-overview.component.scss'],
})
export class DocumentOverviewComponent implements OnInit, OnDestroy {
  @Input()
  participant!: IParticipant;
  @Input()
  createdDocuments!: DocumentDto[];
  @Input()
  userConfiguration!: UserConfiguration;

  openedTab = 0;

  openState: SpecificDocumentDto[] = [];

  inAnimation = true;

  private subscriptions: Subscription[] = [];

  constructor(
    private translateService: TranslateService,
    private route: ActivatedRoute,
    private participantService: ParticipantService,
    private userService: UserService,
    private router: Router,
    private snackBar: MatSnackBar,
    private userConfigurationService: UserConfigurationService
  ) {}

  async ngOnInit(): Promise<void> {
    const participantKeys = Object.keys(
      this.userConfiguration.editorConfiguration.openedDocuments
    );
    if (participantKeys.every((k) => k !== this.participant.id)) {
      this.userConfiguration.editorConfiguration.openedDocuments[
        this.participant.id
      ] = [];
    }

    const ids =
      this.userConfiguration.editorConfiguration.openedDocuments[
        this.participant.id
      ];

    /*
    const ids = JSON.parse(
      localStorage.getItem(
        `participant.editor.${this.userService.User?.id}.${this.participant.id}.opened`
      ) ?? '[]'
    );*/
    for (const id of ids) {
      await this.openAction(id);
    }
    this.subscriptions.push(
      this.route.queryParams.subscribe(async (next) => {
        if (next.openDocument) {
          await this.openAction(next.openDocument);
          await this.router.navigate(['/www/participant/editor']);
        }
        if (next.closeDocument) {
          await this.closeAction(next.closeDocument);
          await this.router.navigate(['/www/participant/editor']);
        }
      }),
      this.participantService.DocumentsSaved.subscribe(async (next) => {
        if (this.participant.id !== next) {
          return;
        }
        await this.reloadAction();
      }),
      this.participantService.DocumentRenamed.subscribe((next) => {
        if (this.participant.id !== next.participantId) {
          return;
        }
        const document = this.openState.find((o) => o.id === next.documentId);
        if (!document) {
          return;
        }
        document.name = next.newName;
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach((s) => s.unsubscribe());
  }

  @HostListener('window:keyup', ['$event'])
  async handleKeyUp(event: KeyboardEvent) {
    if (this.inAnimation) {
      return;
    }
    if (event.ctrlKey && event.shiftKey && event.key === 'ArrowLeft') {
      this.openedTab = (this.openedTab - 1) % this.openState.length;
      this.openedTab =
        this.openedTab < 0 ? this.openState.length - 1 : this.openedTab;
    }

    if (event.ctrlKey && event.shiftKey && event.key === 'ArrowRight') {
      this.openedTab = (this.openedTab + 1) % this.openState.length;
    }

    if (event.ctrlKey && event.shiftKey && event.key === 'Q') {
      await this.closeAction(this.openedTab);
    }
  }

  async reloadAction(): Promise<void> {
    for (const openDocument of this.openState) {
      const newObject = await this.participantService.GetParticipantDocument(
        this.participant.id,
        openDocument.id
      );
      openDocument.isDelta = newObject.isDelta;
      openDocument.name = newObject.name;
      openDocument.documents = newObject.documents;
    }
  }

  async openAction(documentId: string | null): Promise<void> {
    if (!documentId) {
      return;
    }
    const tab = this.openState.find((value) => value.id === documentId);
    if (tab) {
      this.openedTab = this.openState.indexOf(tab);
      return;
    }
    try {
      const document = await this.participantService.GetParticipantDocument(
        this.participant.id,
        documentId
      );
      document.parentDocumentName = document.parentDocumentName
        ? document.parentDocumentName + ' / '
        : '';
      const newLength = this.openState.push(document);
      setTimeout(() => (this.openedTab = newLength - 1), 100);

      await this.setOpenState(this.openState);
    } catch (error) {
      const message = this.translateService.instant(
        'Application.DocumentOverview.UnknownDocument'
      );
      const dismiss = this.translateService.instant(
        'Application.Errors.Dismiss'
      );
      this.snackBar.open(message, dismiss, {
        duration: 5000,
      });
      console.warn(`A document with the id ${documentId} could not be found`);
    }
  }

  async closeAction(documentIdOrTabIndex: number | string | null) {
    if (!documentIdOrTabIndex && documentIdOrTabIndex !== 0) {
      return;
    }
    // Remove with index
    const tabIndex = +documentIdOrTabIndex;
    if (tabIndex || tabIndex === 0) {
      this.openState.splice(tabIndex, 1);
      await this.setOpenState(this.openState);
      return;
    }
    // Remove with document id
    const documentId = '' + documentIdOrTabIndex;
    const documentObjects = this.openState.filter((p) => p.id === documentId);
    if (documentObjects.length !== 1) {
      return;
    }
    const participantIndex = this.openState.indexOf(documentObjects[0]);
    this.openState.splice(participantIndex, 1);
    await this.setOpenState(this.openState);
  }

  tabSwitching(): void {
    this.inAnimation = true;
  }

  tabSwitched(): void {
    this.inAnimation = false;
  }

  async closeTab(index: number) {
    this.openState.splice(index, 1);
    await this.setOpenState(this.openState);
  }

  async closeAll() {
    this.openState = [];
    await this.setOpenState(this.openState);
  }

  async closeOther(index: number) {
    this.openState = this.openState.splice(index, 1);
    await this.setOpenState(this.openState);
  }

  async closeRight(index: number) {
    this.openState = this.openState.filter((v, i) => i <= index);
    await this.setOpenState(this.openState);
  }

  async closeLeft(index: number) {
    this.openState = this.openState.filter((v, i) => i >= index);
    await this.setOpenState(this.openState);
  }

  async setOpenState(openState: DocumentDto[]) {
    this.openState = openState;
    this.userConfiguration.editorConfiguration.openedDocuments[
      this.participant.id
    ] = this.openState.map((p) => p.id);
    await this.userConfigurationService.SetUserConfiguration(
      this.userConfiguration
    );
    /*
    this.openState = openState;
    const savedIds = JSON.stringify(this.openState.map((p) => p.id));
    localStorage.setItem(
      `participant.editor.${this.userService.User?.id}.${this.participant.id}.opened`,
      savedIds
    );*/
  }
}
