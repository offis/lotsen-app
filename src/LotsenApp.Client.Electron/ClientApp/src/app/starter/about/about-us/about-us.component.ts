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
import { ElectronService } from '../../core/electron.service';
import { LegalService } from '../legal.service';

@Component({
  selector: 'la2-about-us',
  templateUrl: './about-us.component.html',
  styleUrls: ['./about-us.component.scss'],
})
export class AboutUsComponent implements OnInit {
  versions: { [key: string]: string } = {};
  Object = Object;
  license = '';
  showInformation = false;

  constructor(
    private electronService: ElectronService,
    private legalService: LegalService
  ) {}

  async ngOnInit(): Promise<void> {
    this.license = await this.legalService.LoadLicense();
    this.showInformation = this.electronService.isElectronApp;
    if (!this.electronService.isElectronApp) {
      return;
    }
    this.electronService.ipcRenderer.once('dotnet-version', (message) =>
      this.addVersions(message)
    );
    this.electronService.ipcRenderer.send('dotnet-version');
  }

  private addVersions(message: string): void {
    const deserializedMessage = JSON.parse(message);
    for (let key of Object.keys(deserializedMessage)) {
      this.versions[key] = deserializedMessage[key];
    }
    for (let key of Object.keys(this.electronService.versions).sort(
      this.sortKeys
    )) {
      // @ts-ignore
      this.versions[key] = this.electronService.versions[key];
    }
  }

  private sortKeys(a: string, b: string): number {
    if (a === 'electron') {
      return -1;
    }
    if (a === 'chrome' && b !== 'electron') {
      return -1;
    }
    if (a === 'node' && b !== 'electron' && b !== 'chrome') {
      return -1;
    }
    if (b === 'electron') {
      return 1;
    }
    if (b === 'chrome' && a !== 'electron') {
      return 1;
    }
    if (b === 'node' && a !== 'electron' && a !== 'chrome') {
      return 1;
    }
    return a < b ? -1 : 1;
  }
}
