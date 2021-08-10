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

import { Component, NgZone, OnDestroy, OnInit } from '@angular/core';
import { ElectronService } from '../../core/electron.service';

@Component({
  selector: 'la2-update-check',
  templateUrl: './update-check.component.html',
  styleUrls: ['./update-check.component.scss'],
})
export class UpdateCheckComponent implements OnInit, OnDestroy {
  isVisible = false;
  update = false;
  upToDate = false;
  working = false;
  downloaded = false;
  error = false;

  constructor(
    private electronService: ElectronService,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
    this.isVisible = this.electronService.isElectronApp;
    if (!this.electronService.isElectronApp) {
      return;
    }
    this.electronService.ipcRenderer.on(
      'update-available',
      this.updateAvailable.bind(this)
    );
    this.electronService.ipcRenderer.on(
      'update-not-available',
      this.updateNotAvailable.bind(this)
    );
    this.electronService.ipcRenderer.on(
      'update-error',
      this.updateError.bind(this)
    );
    this.electronService.ipcRenderer.on(
      'update-downloaded',
      this.updateDownloaded.bind(this)
    );
  }

  ngOnDestroy(): void {
    if (!this.electronService.isElectronApp) {
      return;
    }
    this.electronService.ipcRenderer.removeListener(
      'update-available',
      this.updateAvailable
    );
    this.electronService.ipcRenderer.removeListener(
      'update-not-available',
      this.updateNotAvailable
    );
    this.electronService.ipcRenderer.removeListener(
      'update-error',
      this.updateError
    );
  }

  checkForUpdates(): void {
    this.working = true;
    this.upToDate = false;
    this.update = false;
    this.error = false;
    this.electronService.ipcRenderer.send('check-for-updates', {});
  }

  updateAvailable(data: any): void {
    this.ngZone.run(() => {
      this.update = true;
      this.working = false;
    });
  }

  updateNotAvailable(data: any): void {
    this.ngZone.run(() => {
      this.upToDate = true;
      this.working = false;
    });
  }

  updateError(error: any): void {
    console.error('There was an error checking for updates', error);
    this.ngZone.run(() => {
      this.working = false;
      this.error = true;
    });
  }

  downloadUpdate(): void {
    this.working = true;
    this.upToDate = false;
    this.error = false;
    this.electronService.ipcRenderer.send('download-update', {});
  }

  updateDownloaded() {
    this.ngZone.run(() => {
      this.working = false;
      this.downloaded = true;
    });
  }
}
