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
  OnInit,
  ViewChild,
} from '@angular/core';
import { ParticipantService } from '../../participant.service';
import { TranslateService } from '@ngx-translate/core';
import { Router } from '@angular/router';
import { MatButton } from '@angular/material/button';
import { IParticipant } from '../../iparticipant';
import { Displayable } from '../../displayable';
import { ProjectService } from '../../project.service';

@Component({
  selector: 'la2-document-create-menu',
  templateUrl: './document-create-menu.component.html',
  styleUrls: ['./document-create-menu.component.scss'],
})
export class DocumentCreateMenuComponent implements OnInit {
  @Input()
  participant!: IParticipant;

  @ViewChild('createDocumentButton')
  firstButton!: MatButton;

  creatableDocuments: Displayable[] = [];

  constructor(
    private participantService: ParticipantService,
    private projectService: ProjectService,
    private translateService: TranslateService,
    private router: Router
  ) {}

  async ngOnInit(): Promise<void> {
    this.creatableDocuments = await this.projectService.GetCreatableDocuments(
      this.participant.documentedBy
    );
  }

  @HostListener('window:keyup', ['$event'])
  handleKeyUp(event: KeyboardEvent): void {
    if (event.ctrlKey && event.key === 'n') {
      this.firstButton._elementRef.nativeElement.click();
    }
  }

  async createDocument(displayable: Displayable): Promise<void> {
    let name = displayable.name;
    if (displayable.i18NKey) {
      name = await this.translateService.get(displayable.i18NKey).toPromise();
    }
    const idResponse = await this.participantService.CreateDocument(
      displayable,
      this.participant.documentedBy,
      this.participant.id,
      name
    );
    await this.router.navigate(['/www/participant/editor'], {
      queryParams: {
        openDocument: idResponse.id,
      },
    });
  }
}
