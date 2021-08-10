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
  EventEmitter,
  Output,
  ViewChild,
} from '@angular/core';
import {
  FormControl,
  Validators,
  ValidatorFn,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatButton } from '@angular/material/button';
import { KeySample } from './key-sample';

@Component({
  selector: 'la2-data-password',
  templateUrl: './data-password.component.html',
  styleUrls: ['./data-password.component.scss'],
})
export class DataPasswordComponent implements OnInit {
  completed = false;
  equal = true;
  error = false;

  get recoveryAddress(): string {
    return `/api/recovery-key/download?key=${encodeURIComponent(
      this.recoveryControl.value
    )}`;
  }

  @Output()
  stepSuccess = new EventEmitter<boolean>();

  dataPasswordControl = new FormControl('', [Validators.required]);

  recoveryControl = new FormControl('', [Validators.required]);

  @ViewChild(MatButton)
  submitButton!: MatButton;

  sameValueValidate: ValidatorFn = (
    control: AbstractControl
  ): ValidationErrors => {
    if (control.value === this.dataPasswordControl.value) {
      return {};
    }
    return {
      notEqual: true,
    };
  };

  // eslint-disable-next-line @typescript-eslint/member-ordering
  dataPasswordRepeatControl = new FormControl('', [
    Validators.required,
    this.sameValueValidate,
  ]);

  constructor(private http: HttpClient) {}

  async ngOnInit(): Promise<void> {
    try {
      const recoveryKey = await this.http
        .get<KeySample>('/api/recovery-key')
        .toPromise();
      this.recoveryControl.setValue(recoveryKey.keySample);
      const hasDataPassword = await this.http
        .get<boolean>('/api/configuration/user/data-password')
        .toPromise();
      if (hasDataPassword) {
        this.completed = true;
        this.stepSuccess.emit(true);
      }
    } catch (error) {
      // let this error go unnoticed
      this.completed = false;
      console.error(error);
    }
  }

  async submitDataPassword(): Promise<void> {
    this.equal =
      this.dataPasswordControl.value === this.dataPasswordRepeatControl.value;
    if (
      !this.equal ||
      this.dataPasswordControl.errors !== null ||
      this.recoveryControl.errors !== null
    ) {
      return;
    }
    this.submitButton.disabled = true;
    // submit data password
    try {
      await this.http
        .post('/api/configuration/user/data-password', {
          dataPassword: this.dataPasswordControl.value,
          recoveryKey: this.recoveryControl.value,
        })
        .toPromise();
      this.completed = true;
      this.stepSuccess.emit(true);
    } catch (error) {
      // Let this error go unnoticed
      this.error = true;
    } finally {
      this.submitButton.disabled = false;
    }
  }
}
