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
  OnInit,
  ViewChild,
  ElementRef,
  AfterViewInit,
  Input,
  Output,
  EventEmitter,
} from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { MatButton } from '@angular/material/button';
import {
  trigger,
  state,
  style,
  transition,
  animate,
} from '@angular/animations';

@Component({
  selector: 'la2-data-password',
  templateUrl: './data-password.component.html',
  styleUrls: ['./data-password.component.scss'],
  animations: [
    trigger('fadeInOut', [
      state(
        'show',
        style({
          opacity: 1,
          maxHeight: '5em',
          margin: '1em',
        })
      ),
      state(
        'hide',
        style({
          opacity: 0,
          maxHeight: 0,
          margin: 0,
        })
      ),
      transition('show => hide', [
        animate('0.3s 0s cubic-bezier(0.4, 0.0, 1, 1)'),
      ]),
      transition('hide => show', [
        animate('0.3s 0s cubic-bezier(0.4, 0.0, 1, 1)'),
      ]),
    ]),
  ],
})
export class DataPasswordComponent implements OnInit, AfterViewInit {
  error = false;
  recoveryKey = false;

  dataPasswordControl = new FormControl('', [Validators.required]);
  recoveryKeyControl = new FormControl('', [Validators.required]);

  @ViewChild('submitButton')
  submitButton!: MatButton;
  @ViewChild('fileUpload')
  fileUpload!: ElementRef;
  @ViewChild('dataRecovery')
  dataRecovery!: MatButton;
  @ViewChild('dataPasswordField')
  dataPasswordInput!: ElementRef;

  @Input()
  navigate = true;
  @Output()
  verificationSuccess = new EventEmitter<boolean>();

  constructor(private http: HttpClient, private router: Router) {}

  async ngOnInit(): Promise<void> {
    try {
      const alreadyVerified = await this.http
        .get<boolean>('/api/authentication/data-password')
        .toPromise();
      if (alreadyVerified) {
        if (this.navigate) {
          await this.router.navigate(['/www/participant']);
        }
        this.verificationSuccess.emit(true);
      }
    } catch (error) {
      // Let this error go unnoticed
    }
  }

  ngAfterViewInit(): void {
    setTimeout(() => this.dataPasswordInput.nativeElement.focus(), 300);
  }

  async verifyDataPassword(): Promise<void> {
    this.error = false;
    if (this.dataPasswordControl.errors !== null) {
      return;
    }
    this.submitButton.disabled = true;
    try {
      if (!this.recoveryKey) {
        await this.http
          .post('/api/authentication/data-password', {
            dataPassword: this.dataPasswordControl.value,
          })
          .toPromise();
        if (this.navigate) {
          await this.router.navigate(['/www/participant']);
        }
        this.verificationSuccess.emit(true);
      } else {
        await this.http
          .put('/api/configuration/user/data-password', {
            newDataPassword: this.dataPasswordControl.value,
            recoveryKey: this.recoveryKeyControl.value,
          })
          .toPromise();
        this.recoveryKey = false;
      }
    } catch (error) {
      // forward the error to the user
      this.error = true;
    } finally {
      this.submitButton.disabled = false;
    }
  }

  showDataRecovery(): void {
    this.recoveryKey = !this.recoveryKey;
  }

  startFileUpload(): void {
    this.fileUpload.nativeElement.click();
  }

  readFile(): void {
    if (
      this.fileUpload.nativeElement.files &&
      this.fileUpload.nativeElement.files.length > 0
    ) {
      const file = this.fileUpload.nativeElement.files[0];
      const fileReader = new FileReader();
      fileReader.onload = (evt) =>
        this.recoveryKeyControl.setValue(evt.target?.result ?? '');
      fileReader.readAsText(file);
    }
  }
}
