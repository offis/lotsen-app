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
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { IParticipant } from '../iparticipant';
import { IProject } from '../iproject';
import { HttpClient } from '@angular/common/http';
import { Subscription } from 'rxjs';
import { MatButton } from '@angular/material/button';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { ParticipantService } from '../participant.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ParticipantNameComponent } from '../../shared/participant-name/participant-name.component';
import { ProjectService } from '../project.service';

@Component({
  selector: 'la2-create-participant',
  templateUrl: './create-participant.component.html',
  styleUrls: ['./create-participant.component.scss'],
})
export class CreateParticipantComponent
  implements OnInit, OnDestroy, AfterViewInit
{
  nameControl = new FormControl('', [Validators.required]);
  icon = 'face';
  previewParticipant!: IParticipant;
  color = '#0002';
  selectedProject = new FormControl('');
  documentedBy = new FormControl(null, [Validators.required]);

  projects: IProject[] = [];
  documents: IProject[] = [];

  @ViewChild('nameField')
  nameField!: ParticipantNameComponent;
  @ViewChild('createButton')
  createButton!: MatButton;

  private subscriptions: Subscription[] = [];
  private namePlaceholder = '';

  constructor(
    private http: HttpClient,
    private participantService: ParticipantService,
    private projectService: ProjectService,
    private router: Router,
    private translateService: TranslateService,
    private snackBar: MatSnackBar
  ) {}

  async ngOnInit(): Promise<void> {
    this.createParticipant();
    this.projects = await this.projectService.GetProjects();
    this.documents = await this.projectService.GetDocumentationProjects();

    this.subscriptions.push(
      this.selectedProject.valueChanges.subscribe((next) => {
        if (next) {
          this.documentedBy.disable();
          this.documentedBy.setValue(next);
        } else {
          this.documentedBy.enable();
        }
      })
    );

    this.subscriptions.push(
      this.translateService
        .get('Application.Project.Create.Id')
        .subscribe((v) => {
          this.namePlaceholder = v;
          this.createParticipant();
        })
    );

    if (this.documents.length === 1) {
      this.documentedBy.setValue(this.documents[0].id);
    }

    if (this.projects.length === 1) {
      this.selectedProject.setValue(this.projects[0].id);
    }
  }

  ngAfterViewInit(): void {
    setTimeout(() => this.nameField.focus(), 300);
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach((s) => s.unsubscribe());
  }

  onIconPickerSelect(newIcon: string): void {
    this.icon = newIcon;
    this.createParticipant();
  }

  createParticipant(): void {
    this.previewParticipant = {
      id: this.namePlaceholder,
      projectId: this.selectedProject.value,
      createdAt: new Date().toUTCString(),
      synchronized: false,
      synchronizedAt: '',
      header: {
        name: [this.nameControl.value],
        tint: [this.color],
        icon: [this.icon],
      },
      documentedBy: '',
    };
  }

  getTint(color: string): string {
    if (color.length === 7) {
      return `${color}22`;
    }
    if (color.length === 9) {
      return `${color.substr(0, 7)}22`;
    }
    return color;
  }

  async createNewParticipant(): Promise<void> {
    if (this.nameControl.errors !== null || this.documentedBy.errors !== null) {
      return;
    }
    this.createButton.disabled = true;
    try {
      const response = await this.participantService.CreateParticipant({
        name: this.nameControl.value,
        icon: this.icon,
        tint: this.color,
        projectId: this.selectedProject.value
          ? this.selectedProject.value
          : null,
        documentedBy: this.documentedBy.value,
      });

      await this.router.navigate(['/www/participant/editor'], {
        queryParams: {
          open: response.id,
        },
      });
    } catch (error) {
      console.error(error);
      const message = await this.translateService
        .get('Application.CreateParticipant.Error')
        .toPromise();
      const dismiss = await this.translateService
        .get('Application.Errors.Dismiss')
        .toPromise();
      this.snackBar.open(message, dismiss, {
        duration: 5000,
      });
    } finally {
      this.createButton.disabled = false;
    }
  }
}
