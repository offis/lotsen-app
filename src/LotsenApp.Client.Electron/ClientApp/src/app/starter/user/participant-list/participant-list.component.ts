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
import { ParticipantService } from '../../participant/participant.service';
import { IParticipant } from '../../participant/iparticipant';
import { IProject } from '../../participant/iproject';
import { HttpClient } from '@angular/common/http';
import { ProjectService } from '../../participant/project.service';

@Component({
  selector: 'la2-participant-list',
  templateUrl: './participant-list.component.html',
  styleUrls: ['./participant-list.component.scss'],
})
export class ParticipantListComponent implements OnInit {
  participants: IParticipant[] = [];
  projects: IProject[] = [];
  alreadyVerified?: boolean;
  working = true;
  constructor(
    private participantService: ParticipantService,
    private projectService: ProjectService,
    private http: HttpClient
  ) {}

  async ngOnInit(): Promise<void> {
    await this.updateVerification(
      await this.http
        .get<boolean>('/api/authentication/data-password')
        .toPromise()
    );
  }

  async updateVerification(event: boolean): Promise<void> {
    this.alreadyVerified = event;
    if (event) {
      this.working = true;
      this.participants = await this.participantService.GetParticipants();
      this.projects = await this.projectService.GetProjects();
    }
    this.working = false;
  }
}
