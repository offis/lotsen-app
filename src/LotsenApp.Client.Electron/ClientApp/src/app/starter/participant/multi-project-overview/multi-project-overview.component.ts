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
import { IParticipant } from '../iparticipant';
import { IProject } from '../iproject';
import { TranslateService } from '@ngx-translate/core';
import { IParticipantProjectAssociation } from './iparticipant-project-association';

@Component({
  selector: 'la2-multi-project-overview',
  templateUrl: './multi-project-overview.component.html',
  styleUrls: ['./multi-project-overview.component.scss'],
})
export class MultiProjectOverviewComponent implements OnInit {
  @Input()
  participants: IParticipant[] = [];
  @Input()
  projects: IProject[] = [];

  @Input()
  forceListView: boolean | undefined;
  @Input()
  calculateWidth = true;
  @Input()
  dateFormat = 'short';

  participantProjectAssociation: IParticipantProjectAssociation[] = [];
  withoutProjectAssociation!: IParticipantProjectAssociation;
  mapping = {
    '=0': 'Application.MultiProjectOverview.ParticipantCount.=0',
    '=1': 'Application.MultiProjectOverview.ParticipantCount.=1',
    other: 'Application.MultiProjectOverview.ParticipantCount.other',
  };

  constructor(private translateService: TranslateService) {}

  ngOnInit(): void {
    for (const project of this.projects) {
      this.participantProjectAssociation.push({
        id: project.id,
        name: project.name,
        I18NKey: project.I18NKey,
        participants: this.findParticipants(project.id),
      });
    }
    this.withoutProjectAssociation = {
      id: '',
      I18NKey: 'Project.Other.Name',
      name: 'Other',
      participants: this.findParticipantsWithoutProject(),
    };
  }

  getProjectName(project: IProject): string {
    const translation = this.translateService.instant(project.I18NKey);
    if (translation === project.I18NKey) {
      return project.name;
    }
    return translation;
  }

  findParticipants(projectId: string): IParticipant[] {
    return this.participants.filter((p) => p.projectId === projectId);
  }

  findParticipantsWithoutProject(): IParticipant[] {
    return this.participants.filter((p) =>
      this.projects.every((project) => project.id !== p.projectId)
    );
  }
}
