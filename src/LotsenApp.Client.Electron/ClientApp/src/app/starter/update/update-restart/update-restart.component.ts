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
import { MatDialog } from '@angular/material/dialog';
import { UpdateRestartConfirmComponent } from '../update-restart-confirm/update-restart-confirm.component';
import { UserConfigurationService } from '../../core/user-configuration.service';

@Component({
  selector: 'la2-update-restart',
  templateUrl: './update-restart.component.html',
  styleUrls: ['./update-restart.component.scss'],
})
export class UpdateRestartComponent implements OnInit, OnDestroy {
  done = false;

  constructor(
    private electronService: ElectronService,
    private ngZone: NgZone,
    private dialog: MatDialog,
    private configurationService: UserConfigurationService
  ) {}

  ngOnInit(): void {
    if (!this.electronService.isElectronApp) {
      return;
    }
    this.electronService.ipcRenderer.on(
      'update-downloaded',
      this.updateDownloaded.bind(this)
    );
  }

  ngOnDestroy() {
    this.electronService.ipcRenderer.removeListener(
      'update-downloaded',
      this.updateDownloaded
    );
  }

  async updateDownloaded() {
    const configuration =
      await this.configurationService.GetUserConfiguration();
    if (configuration.updateConfiguration.autoInstall) {
      this.electronService.ipcRenderer.send('update-restart', {});
    } else {
      this.ngZone.run(() => {
        this.done = true;
      });
    }
  }

  async restart() {
    const ref = this.dialog.open(UpdateRestartConfirmComponent);
    const result = await ref.afterClosed().toPromise();
    if (result) {
      this.electronService.ipcRenderer.send('update-restart', {});
    }
  }
}
