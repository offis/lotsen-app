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

import { Injectable } from '@angular/core';
import { Debug, Electron, Ipc, Versions } from './electron';

@Injectable({
  providedIn: 'root',
})
export class ElectronService {
  private readonly electron: Electron;

  isElectronApp = false;
  get isWindows(): boolean {
    return this.isElectronApp && this.electron.platform === 'win32';
  }
  get isLinux(): boolean {
    return this.isElectronApp && this.electron.platform === 'linux';
  }
  get isMacOS(): boolean {
    return this.isElectronApp && this.electron.platform === 'darwin';
  }

  get versions(): Versions {
    if (this.isElectronApp) {
      return this.electron.versions;
    }
    throw new Error('This application is not running as an electron app');
  }

  get ipcRenderer(): Ipc {
    if (this.isElectronApp) {
      return this.electron.ipc;
    }
    throw new Error('This application is not running as an electron app');
  }

  get debug(): Debug {
    if (this.isElectronApp) {
      return this.electron.debug;
    }
    throw new Error('This application is not running as an electron app');
  }

  constructor() {
    this.electron = (window as any).electron as Electron;
    if (!this.electron) {
      console.warn(
        'window.electron is not an object. Electron integration will be disabled'
      );
      return;
    }
    this.isElectronApp = true;
  }

  openExternal(url: string): void {
    if (this.isElectronApp) {
      this.electron.openExternal(url);
    }
  }
}
