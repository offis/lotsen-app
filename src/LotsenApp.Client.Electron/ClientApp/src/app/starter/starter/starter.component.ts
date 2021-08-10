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
import {MatSidenav} from '@angular/material/sidenav';
import {TaskbarComponent} from '../taskbar/taskbar.component';
import {Subscription} from 'rxjs';
import {NavigationEnd, Router} from '@angular/router';
import {ElectronService} from '../core/electron.service';
import {UserService} from '../core/user.service';
import {ReminderService} from '../core/reminder.service';
import {UserConfigurationService} from '../core/user-configuration.service';

@Component({
  selector: 'la2-starter',
  templateUrl: './starter.component.html',
  styleUrls: ['./starter.component.scss'],
})
export class StarterComponent implements OnInit, OnDestroy {
  @ViewChild(MatSidenav)
  drawer!: MatSidenav;
  @ViewChild(TaskbarComponent)
  taskbar!: TaskbarComponent;

  isVisible = false;

  @HostListener('window:keydown', ['$event'])
  handleKeyDown(event: KeyboardEvent): void {
    if (event.code === 'F9') {
      this.isVisible = !this.isVisible;
    }
  }

  private subscriptions: Subscription[] = [];

  constructor(
    private router: Router,
    private electronService: ElectronService,
    private userService: UserService,
    private reminderService: ReminderService
  ) {}

  async ngOnInit(): Promise<void> {
    if (this.router.url === '/www' || this.router.url === '/www/home') {
      await this.router.navigate(['/www/home']);
    }
    this.subscriptions.push(
      this.router.events.subscribe(async (o) => {
        if (o instanceof NavigationEnd && o.url === '/www') {
          await this.router.navigate(['/www/home']);
        }
      }),
      this.userService.IsSignedIn.subscribe((signedIn) => {
        if (signedIn) {
          this.scheduleReminder();
        } else {
          this.clearScheduledReminder();
        }
      }),
      this.reminderService.ReminderCreated.subscribe((next) =>
        this.scheduleReminder()
      ),
      this.reminderService.ReminderDeleted.subscribe((next) =>
        this.scheduleReminder()
      )
    );

    // TODO Reminder updated needed

    if (!this.electronService.isElectronApp) {
      return;
    }
    this.electronService.ipcRenderer.once('dotnet-version', (message) =>
      this.checkVersion(message)
    );
    this.electronService.ipcRenderer.send('dotnet-version');
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach((s) => s.unsubscribe());
  }

  private clearScheduledReminder() {
    this.reminderService.clearReminderSchedule();
  }

  private async scheduleReminder() {
    const requestInterval = 10 * 60 * 1000; // 10 minutes
    try {
      const tillMilliseconds = +new Date() + requestInterval;
      const tillDate = new Date(tillMilliseconds);
      console.debug('Scheduling notifications until ', tillDate);
      await this.reminderService.scheduleNotifications(tillDate);
    } finally {
      setTimeout(() => this.scheduleReminder(), requestInterval);
    }
  }

  private checkVersion(message: string): void {
    console.debug('Before version check');
    const deserializedMessage = JSON.parse(message);
    const appVersion = deserializedMessage['app-version'];
    const storedVersion = localStorage.getItem('version');
    if (storedVersion !== appVersion) {
      console.log('Clearing cache after update');
      localStorage.setItem('version', appVersion);
      this.electronService.ipcRenderer.send('update-cache', {});
    }
  }

  async menuChanged(): Promise<void> {
    await this.drawer.toggle();
  }

  notifyTaskbar(): void {
    this.taskbar.toggleMenuAnimation();
  }

  async toggleMenuState(): Promise<void> {
    await this.menuChanged();
    this.notifyTaskbar();
  }
}
