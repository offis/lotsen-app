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
import { MatDialog } from '@angular/material/dialog';
import { ColorPickerDialogComponent } from '../../shared/color-picker-dialog/color-picker-dialog.component';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { ElectronService } from '../../core/electron.service';

@Component({
  selector: 'la2-developer-sandbox',
  templateUrl: './developer-sandbox.component.html',
  styleUrls: ['./developer-sandbox.component.scss'],
})
export class DeveloperSandboxComponent implements OnInit {
  private tan = '';
  tanList = '';
  constructor(
    private http: HttpClient,
    private electronService: ElectronService
  ) {}

  ngOnInit(): void {
    // setTimeout(() => this.openDialog(), 100);
    // (window as any).showSaveFilePicker();
  }

  async openLoadFileDialog() {
    const options = {
      types: [
        {
          description: 'Text Files',
          accept: {
            'text/plain': ['.txt'],
          },
        },
      ],
    };
    const [handle] = await (window as any).showOpenFilePicker(options);
    const file = await handle.getFile();
    const text = (await file.text()) as string;
    const tan = text.replace('\r', '').split('\n');
    this.tan = tan[0];
  }

  async openSaveFileDialog() {
    if (this.electronService.isElectronApp) {
      await this.OpenSaveFileDialogInElectron();
    } else {
      await this.OpenSaveFileDialogInBrowser();
    }
  }

  async OpenSaveFileDialogInElectron() {
    console.log('Requesting new tan');
    const list = await this.http
      .get<string[]>('/api/authorization/tan/list')
      .pipe(map((input) => input.join('\n')))
      .toPromise();
    console.log(list);
    // this.tanList = `data:text/plain;charset=utf-8,${encodeURIComponent(list)}`;
    this.electronService.ipcRenderer.once(
      'save-file-dialog-complete',
      (result) => {
        console.log('Saving file to ', result);
      }
    );
    console.log('Requesting save file dialog');
    this.electronService.ipcRenderer.send('open-save-file-dialog', {
      name: 'tan-list',
      data: list,
    });
  }

  async OpenSaveFileDialogInBrowser() {
    const options = {
      suggestedName: 'tan-list.txt',
      startIn: 'documents',
      types: [
        {
          description: 'Text Files',
          accept: {
            'text/plain': ['.txt'],
          },
        },
      ],
    };
    console.log('Show Save file dialog');
    const handle = await (window as any).showSaveFilePicker(options);
    console.log('Checking permissions', handle, await handle.getFile());
    if ((await handle.queryPermission({ mode: 'readwrite' })) !== 'granted') {
      console.log('No write permissions have been granted');
      console.log('Requesting permission');
      if (
        (await handle.requestPermission({ mode: 'readwrite' })) !== 'granted'
      ) {
        console.log(
          'Write permissions have not been granted. Cannot save tan list'
        );
        return;
      }
    }

    console.log('Requesting new tan');
    const list = await this.http
      .get<string[]>('/api/authorization/tan/list')
      .pipe(map((input) => input.join('\n')))
      .toPromise();
    console.log('Creating writeable handle');
    const writeable = await handle.createWritable();
    console.log('Writing content');
    await writeable.write(list);
    console.log('closing file');
    await writeable.close();
  }
}
