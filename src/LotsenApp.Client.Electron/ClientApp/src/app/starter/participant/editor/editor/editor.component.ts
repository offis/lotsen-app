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
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ParticipantService } from '../../participant.service';
import { IParticipant } from '../../iparticipant';
import { UserService } from '../../../core/user.service';
import { MatTabGroup } from '@angular/material/tabs';
import { Subscription } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { UserConfigurationService } from '../../../core/user-configuration.service';
import { UserConfiguration } from '../../../core/user-configuration';

@Component({
  selector: 'la2-editor',
  templateUrl: './editor.component.html',
  styleUrls: ['./editor.component.scss'],
})
export class EditorComponent implements OnInit, OnDestroy {
  openState: IParticipant[] = [];
  openedTab = 0;
  userConfiguration!: UserConfiguration;

  inAnimation = false;

  @ViewChild(MatTabGroup)
  tabGroup!: MatTabGroup;

  private subscriptions: Subscription[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private participantService: ParticipantService,
    private translateService: TranslateService,
    private snackBar: MatSnackBar,
    private userService: UserService,
    private configuration: UserConfigurationService
  ) {}

  async ngOnInit(): Promise<void> {
    this.userConfiguration = await this.configuration.GetUserConfiguration();
    const ids = this.userConfiguration.editorConfiguration.openedParticipants;
    for (const id of ids) {
      await this.openAction(id);
    }
    this.subscriptions.push(
      this.route.queryParams.subscribe(async (next) => {
        if (next.open) {
          await this.openAction(next.open);
          if (!next.openDocument) {
            await this.router.navigate(['/www/participant/editor']);
          }
          this.tabGroup?.realignInkBar();
        }
        if (next.close) {
          await this.closeAction(next.close);
          await this.router.navigate(['/www/participant/editor']);
          this.tabGroup?.realignInkBar();
        }
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
    if (event.ctrlKey && event.key === 'ArrowLeft') {
      this.openedTab = (this.openedTab - 1) % this.openState.length;
      this.openedTab =
        this.openedTab < 0 ? this.openState.length - 1 : this.openedTab;
    }

    if (event.ctrlKey && event.key === 'ArrowRight') {
      this.openedTab = (this.openedTab + 1) % this.openState.length;
    }

    if (event.ctrlKey && event.key === 'q') {
      await this.closeAction(this.openedTab);
    }
  }

  async openAction(participantId: string | null): Promise<void> {
    if (!participantId) {
      return;
    }
    if (this.openState.some((value) => value.id === participantId)) {
      const openedParticipant = this.openState.filter(
        (o) => o.id === participantId
      )[0];
      this.openedTab = this.openState.indexOf(openedParticipant);
      return;
    }
    try {
      const participant = await this.participantService.GetParticipant(
        participantId
      );
      this.openState.push(participant);
      this.openedTab = this.openState.indexOf(participant);
      await this.setOpenState(this.openState);
    } catch (error) {
      const message = this.translateService.instant(
        'Application.Editor.Error.UnknownParticipant'
      );
      const dismiss = this.translateService.instant(
        'Application.Errors.Dismiss'
      );
      this.snackBar.open(message, dismiss, {
        duration: 5000,
      });
      await this.closeAction(participantId);
    }
  }

  async closeAction(participantIdOrTabIndex: number | string | null) {
    if (!participantIdOrTabIndex && participantIdOrTabIndex !== 0) {
      return;
    }
    // Remove with index
    const tabIndex = +participantIdOrTabIndex;
    if (tabIndex || tabIndex === 0) {
      this.openState.splice(tabIndex, 1);
      await this.setOpenState(this.openState);
      return;
    }
    // Remove with participant id
    const participantId = '' + participantIdOrTabIndex;
    const participantObjects = this.openState.filter(
      (p) => p.id === participantId
    );
    if (participantObjects.length !== 1) {
      return;
    }
    const participantIndex = this.openState.indexOf(participantObjects[0]);
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

  async setOpenState(openState: IParticipant[]): Promise<void> {
    this.openState = openState;
    this.userConfiguration.editorConfiguration.openedParticipants =
      this.openState.map((p) => p.id);

    await this.configuration.SetUserConfiguration(this.userConfiguration);
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
}
