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
  Output,
  EventEmitter,
  ViewChild,
} from '@angular/core';
import {
  FormControl,
  Validators,
  ValidatorFn,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'la2-password-set',
  templateUrl: './password-set.component.html',
  styleUrls: ['./password-set.component.scss'],
})
export class PasswordSetComponent implements OnInit {
  completed = false;
  equal = true;
  error = false;

  @ViewChild(MatButton)
  submitButton!: MatButton;

  containsAllRequirements: ValidatorFn = (
    control: AbstractControl
  ): ValidationErrors => {
    const nonAlphanumeric = /[^a-zA-Z\d\s:]/;
    const upperCase = /[A-Z]/;
    const lowerCase = /[a-z]/;
    const digit = /[\d]/;
    const minimumPasswordLength = 6;
    const value: string = control.value as string;
    const errors: ValidationErrors = {};
    if (value.search(nonAlphanumeric) === -1) {
      errors.nonAlphanumeric = true;
    }
    if (value.search(upperCase) === -1) {
      errors.upperCase = true;
    }
    if (value.search(lowerCase) === -1) {
      errors.lowerCase = true;
    }
    if (value.search(digit) === -1) {
      errors.digit = true;
    }
    if (value.length <= minimumPasswordLength) {
      errors.minimumPasswordLength = true;
    }
    return errors;
  };

  // eslint-disable-next-line @typescript-eslint/member-ordering
  passwordFormControl = new FormControl('', [
    Validators.required,
    this.containsAllRequirements,
  ]);

  sameValueValidate: ValidatorFn = (
    control: AbstractControl
  ): ValidationErrors => {
    if (control.value === this.passwordFormControl.value) {
      return {};
    }
    return {
      notEqual: true,
    };
  };

  // eslint-disable-next-line @typescript-eslint/member-ordering
  passwordRepeatFormControl = new FormControl('', [
    Validators.required,
    this.sameValueValidate,
  ]);

  constructor(private http: HttpClient) {}

  async ngOnInit(): Promise<void> {
    this.completed = await this.http
      .get<boolean>('/api/authentication/password')
      .toPromise();
  }

  async setPassword(): Promise<void> {
    this.equal =
      this.passwordFormControl.value === this.passwordRepeatFormControl.value;
    if (
      !this.equal ||
      Object.keys(this.passwordFormControl.errors ?? {}).length
    ) {
      return;
    }
    this.submitButton.disabled = true;
    try {
      await this.http
        .put('/api/authentication/password', {
          password: this.passwordFormControl.value,
        })
        .toPromise();
      this.completed = true;
    } catch (error) {
      this.error = true;
    } finally {
      this.submitButton.disabled = false;
    }
  }
}
