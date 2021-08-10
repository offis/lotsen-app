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
  Output,
  EventEmitter,
  ViewChild,
  ElementRef,
} from '@angular/core';
import {
  AbstractControl,
  FormControl,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { MatMenuTrigger } from '@angular/material/menu';
import { HttpClient } from '@angular/common/http';
import { MatButton } from '@angular/material/button';
import { TranslateService } from '@ngx-translate/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'la2-password-reset',
  templateUrl: './password-reset.component.html',
  styleUrls: ['./password-reset.component.scss'],
})
export class PasswordResetComponent {
  @Output()
  resetComplete = new EventEmitter<boolean>();

  @ViewChild('fileUpload')
  fileUpload!: ElementRef;

  @ViewChild('tanMenuTrigger')
  tanMenu!: MatMenuTrigger;

  @ViewChild('submitButton')
  submitButton!: MatButton;

  tanControl = new FormControl('', [Validators.required]);

  userNameControl = new FormControl('', [Validators.required]);

  tans: string[] = [];

  numberOfErrors = 0;

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
    this.numberOfErrors = Object.keys(errors).length + (value ? 0 : 1);
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

  constructor(
    private http: HttpClient,
    private translate: TranslateService,
    private snackBar: MatSnackBar
  ) {}

  async setPassword(): Promise<void> {
    if (
      Object.keys(this.userNameControl.errors ?? {}).length ||
      Object.keys(this.passwordFormControl.errors ?? {}).length ||
      Object.keys(this.passwordRepeatFormControl.errors ?? {}).length ||
      Object.keys(this.tanControl.errors ?? {}).length
    ) {
      return;
    }
    this.submitButton.disabled = true;
    try {
      await this.http
        .post('/api/user/password-recovery', {
          userName: this.userNameControl.value,
          newPassword: this.passwordFormControl.value,
          newPasswordRepeat: this.passwordRepeatFormControl.value,
          tan: this.tanControl.value,
        })
        .toPromise();
      this.success();
    } catch (error) {
      console.error(error);
      // Show the error
      const message = await this.translate
        .get('Application.FirstTimeSetup.Errors.PasswordSetFailed')
        .toPromise();
      const dismiss = await this.translate
        .get('Application.Errors.Dismiss')
        .toPromise();
      this.snackBar.open(message, dismiss, {
        duration: 2500,
      });
    } finally {
      this.submitButton.disabled = false;
    }
  }

  success(): void {
    this.resetComplete.emit(true);
  }

  loadFromFile(): void {
    this.fileUpload.nativeElement.click();
  }

  readFile(event: Event): void {
    if (
      this.fileUpload.nativeElement.files &&
      this.fileUpload.nativeElement.files.length > 0
    ) {
      const file = this.fileUpload.nativeElement.files[0];
      const fileReader = new FileReader();
      fileReader.onload = (evt) =>
        this.showTanList((evt.target?.result as string) ?? '');
      fileReader.readAsText(file);
    }
  }

  showTanList(tanFile: string): void {
    this.tans = tanFile.split('\n');
    this.tanMenu.openMenu();
  }

  selectTan(tan: string): void {
    this.tanControl.setValue(tan);
  }

  clearTans(): void {
    this.tans = [];
    this.fileUpload.nativeElement.value = '';
  }
}
