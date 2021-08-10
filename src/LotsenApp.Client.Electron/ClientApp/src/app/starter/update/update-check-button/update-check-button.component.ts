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
  ChangeDetectorRef,
  Component,
  NgZone,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { ElectronService } from '../../core/electron.service';
import { UserConfigurationService } from '../../core/user-configuration.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'la2-update-check-button',
  templateUrl: './update-check-button.component.html',
  styleUrls: ['./update-check-button.component.scss'],
})
export class UpdateCheckButtonComponent implements OnInit, OnDestroy {
  available = false;

  constructor(
    private electronService: ElectronService,
    private ngZone: NgZone,
    private configurationService: UserConfigurationService,
    private translateService: TranslateService
  ) {}

  ngOnInit(): void {
    if (!this.electronService.isElectronApp) {
      return;
    }
    this.electronService.ipcRenderer.on(
      'update-available',
      this.updateAvailable.bind(this)
    );
    this.electronService.ipcRenderer.on(
      'download-progress',
      this.handleDownloadProgress.bind(this)
    );
    this.electronService.ipcRenderer.send('check-for-updates', {});
  }

  ngOnDestroy() {
    if (!this.electronService.isElectronApp) {
      return;
    }
    this.electronService.ipcRenderer.removeListener(
      'update-available',
      this.updateAvailable
    );
    this.electronService.ipcRenderer.removeListener(
      'download-progress',
      this.handleDownloadProgress
    );
  }

  async updateAvailable(data: any) {
    const configuration =
      await this.configurationService.GetUserConfiguration();
    if (
      configuration.notificationConfiguration.displayNotifications &&
      configuration.notificationConfiguration.updateNotifications
    ) {
      const title = await this.translateService
        .get('Application.Update.NotificationTitle')
        .toPromise();
      const content = await this.translateService
        .get('Application.Update.NotificationContent', {
          version: data.version,
        })
        .toPromise();
      const notification = new Notification(title, {
        body: content,
      });
      notification.onclick = (ev) => {
        this.downloadUpdate();
      };
    }
    if (configuration.updateConfiguration.autoDownload) {
      this.downloadUpdate();
    } else {
      this.ngZone.run(() => {
        this.available = true;
      });
    }
  }

  downloadUpdate(): void {
    this.available = false;
    this.electronService.ipcRenderer.send('download-update', {});
  }
  handleDownloadProgress(data: any): void {
    if (this.available) {
      this.ngZone.run(() => {
        this.available = false;
      });
    }
  }
}
