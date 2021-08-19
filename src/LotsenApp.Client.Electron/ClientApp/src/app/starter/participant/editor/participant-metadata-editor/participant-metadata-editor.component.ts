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

import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ParticipantService } from '../../participant.service';
import { IParticipant } from '../../iparticipant';
import { ActivatedRoute } from '@angular/router';
import { ParticipantNameComponent } from '../../../shared/participant-name/participant-name.component';
import { ParticipantProjectComponent } from '../../../shared/participant-project/participant-project.component';
import { ParticipantTintComponent } from '../../../shared/participant-tint/participant-tint.component';
import { ProjectService } from '../../project.service';
import { IProject } from '../../iproject';
import { FormControl } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';

@Component({
  selector: 'la2-participant-metadata-editor',
  templateUrl: './participant-metadata-editor.component.html',
  styleUrls: ['./participant-metadata-editor.component.scss'],
})
export class ParticipantMetadataEditorComponent implements OnInit, OnDestroy {
  participant!: IParticipant;
  projects: IProject[] = [];
  documents: IProject[] = [];
  nameControl = new FormControl('');
  projectControl = new FormControl();
  documentControl = new FormControl();
  @ViewChild('nameComponent')
  nameComponent!: ParticipantNameComponent;
  @ViewChild('projectComponent')
  projectComponent!: ParticipantProjectComponent;
  @ViewChild('tintComponent')
  tintComponent!: ParticipantTintComponent;
  predefinedColors: string[] = [];

  private subscriptions: Subscription[] = [];

  constructor(
    private participantService: ParticipantService,
    private projectService: ProjectService,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private translateService: TranslateService
  ) {}

  async ngOnInit() {
    this.subscriptions.push(
      this.documentControl.valueChanges.subscribe((next) => {
        if (!next) {
          return;
        }
        const project = this.documents.find((d) => d.id === next);
        if (!project) {
          this.predefinedColors = [];
          return;
        }
        this.predefinedColors = project.colors ?? [];
      })
    );

    const id = this.route.snapshot.params['id'];
    this.participant = await this.participantService.GetParticipant(id);
    this.projects = await this.projectService.GetProjects();
    this.documents = await this.projectService.GetDocumentationProjects();
    this.nameControl.setValue(this.participant.header.name[0]);
    this.projectControl.setValue(this.participant.projectId);
    this.documentControl.setValue(this.participant.documentedBy);
  }

  ngOnDestroy() {
    this.subscriptions.forEach((s) => s.unsubscribe());
  }

  back() {
    history.back();
  }

  async save() {
    const dismiss = this.translateService.instant('Application.Errors.Dismiss');
    try {
      await this.participantService.UpdateHeader({
        participantId: this.participant.id,
        name: this.nameControl.value,
        icon: this.participant.header.icon[0],
        tint: this.participant.header.tint[0],
      });
      // Show save confirmation
      const message = this.translateService.instant(
        'Application.ParticipantMetadataEditor.SaveSuccess'
      );
      this.snackBar.open(message, dismiss, {
        duration: 5000,
      });
    } catch (error) {
      console.error(error);
      const message = this.translateService.instant(
        'Application.ParticipantMetadataEditor.SaveFailure'
      );
      this.snackBar.open(message, dismiss, {
        duration: 5000,
      });
    }
  }
}
