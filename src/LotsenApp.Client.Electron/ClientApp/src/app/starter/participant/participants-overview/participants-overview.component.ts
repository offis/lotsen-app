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

import { Component, OnDestroy, OnInit } from '@angular/core';
import { ParticipantService } from '../participant.service';
import { IProject } from '../iproject';
import { IParticipant } from '../iparticipant';
import { EDisplayMode } from '../../core/edisplay-mode.enum';
import { NavigationEnd, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import {
  animate,
  style,
  transition,
  trigger,
  AnimationEvent,
  animateChild,
  query,
} from '@angular/animations';
import { ProjectService } from '../project.service';
import { UserConfigurationService } from '../../core/user-configuration.service';
import { UserConfiguration } from '../../core/user-configuration';

@Component({
  selector: 'la2-participants-overview',
  templateUrl: './participants-overview.component.html',
  styleUrls: ['./participants-overview.component.scss'],
  animations: [
    trigger('fadeInOut', [
      transition(':enter', [
        style({
          opacity: 0,
          maxWidth: 0,
        }),
        query(':enter', animateChild()),
        animate(
          '0.3s 0.1s cubic-bezier(0.4, 0.0, 1, 1)',
          style({
            opacity: 1,
            maxWidth: '*',
          })
        ),
      ]),
      transition(':leave', [
        style({
          opacity: 1,
          maxWidth: '*',
        }),
        query(':leave', animateChild()),
        animate(
          '0.3s 0.1s cubic-bezier(0.4, 0.0, 1, 1)',
          style({
            opacity: 0,
            maxWidth: 0,
          })
        ),
      ]),
    ]),
  ],
})
export class ParticipantsOverviewComponent implements OnInit, OnDestroy {
  listView: boolean | undefined;
  grouping: boolean | undefined;
  currentListView = `${EDisplayMode.Automatic}`;
  currentGrouping = `${EDisplayMode.Automatic}`;

  closeVisible = false;
  under800 = false;
  overviewVisible = true;
  calculateWidth = true;

  projects: IProject[] = [];
  participants: IParticipant[] = [];

  private subscriptions: Subscription[] = [];
  public configuration?: UserConfiguration;

  constructor(
    private participantService: ParticipantService,
    private projectService: ProjectService,
    private router: Router,
    private userConfigurationService: UserConfigurationService,
    private breakpointObserver: BreakpointObserver
  ) {}

  async ngOnInit(): Promise<void> {
    this.projects = await this.projectService.GetProjects();
    this.participants = await this.participantService.GetParticipants();
    this.configuration =
      await this.userConfigurationService.GetUserConfiguration();

    const storedListView =
      this.configuration.editorConfiguration.participantView ??
      +this.currentListView;

    const storedGrouping =
      this.configuration.editorConfiguration.projectView ??
      +this.currentGrouping;

    if (storedListView !== null) {
      await this.setListView(storedListView);
    }

    if (storedGrouping !== null) {
      await this.setGrouping(storedGrouping);
    }

    this.subscriptions.push(
      this.participantService.ParticipantCreated.subscribe(async (next) => {
        this.participants.push(
          await this.participantService.GetParticipant(next)
        );
      }),
      this.participantService.ParticipantDeleted.subscribe((next) => {
        this.participants = this.participants.filter((p) => p.id !== next);
      })
    );

    this.subscriptions.push(
      this.router.events.subscribe((routerEvent) => {
        if (routerEvent instanceof NavigationEnd) {
          this.changeOverviewDisplay(routerEvent.url !== '/www/participant');
          if (routerEvent.url === '/www/participant') {
            this.overviewVisible = true;
          }
        }
      })
    );
    this.changeOverviewDisplay(
      this.router.routerState.snapshot.url !== '/www/participant'
    );

    this.subscriptions.push(
      this.breakpointObserver
        .observe(['(max-width: 800px)'])
        .subscribe((state) => {
          this.under800 = state.breakpoints['(max-width: 800px)'];
        })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach((s) => s.unsubscribe());
  }

  async updateListView(event: string) {
    const newValue = +event as EDisplayMode;
    await this.setListView(newValue);
  }

  async setListView(newValue: EDisplayMode) {
    this.currentListView = `${newValue}`;
    switch (newValue) {
      case EDisplayMode.List:
        this.listView = true;
        break;
      case EDisplayMode.Tiles:
        this.listView = false;
        break;
      case EDisplayMode.Automatic:
        this.listView = undefined;
        break;
      default:
        this.listView = undefined;
        this.currentListView = `${EDisplayMode.Automatic}`;
        break;
    }
    this.changeOverviewDisplay(
      this.router.routerState.snapshot.url !== '/www/participant'
    );
    if (!this.configuration) {
      return;
    }
    this.configuration.editorConfiguration.participantView = newValue;
    await this.userConfigurationService.SetUserConfiguration(
      this.configuration
    );
  }

  async updateGrouping(event: string) {
    const newValue = +event as EDisplayMode;
    await this.setGrouping(newValue);
  }

  async setGrouping(newValue: EDisplayMode) {
    this.currentGrouping = `${newValue}`;
    switch (newValue) {
      case EDisplayMode.Projects:
        this.grouping = true;
        break;
      case EDisplayMode.NoProjects:
        this.grouping = false;
        break;
      case EDisplayMode.Automatic:
        this.grouping = undefined;
        break;
      default:
        this.grouping = undefined;
        this.currentGrouping = `${EDisplayMode.Automatic}`;
        break;
    }
    if (!this.configuration) {
      return;
    }
    this.configuration.editorConfiguration.projectView = newValue;
    await this.userConfigurationService.SetUserConfiguration(
      this.configuration
    );
  }

  changeOverviewDisplay(activate: boolean): void {
    this.closeVisible = activate;
    if (+this.currentListView === EDisplayMode.Automatic) {
      this.listView = activate;
    }
  }

  toggleParticipants(state: boolean): void {
    this.overviewVisible = state;
  }

  startAnimation(event: AnimationEvent): void {
    this.calculateWidth = false;
  }

  endAnimation(event: AnimationEvent): void {
    this.calculateWidth = true;
  }
}
