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
  Input,
  ViewChild,
  Output,
  EventEmitter,
} from '@angular/core';
import {
  FormControl,
  Validators,
  AbstractControl,
  ValidationErrors,
  AsyncValidatorFn,
} from '@angular/forms';
import { PingDto } from './ping-dto';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { UserService } from '../../core/user.service';
import { MatButton } from '@angular/material/button';
import {
  animate,
  state,
  style,
  transition,
  trigger,
} from '@angular/animations';

@Component({
  selector: 'la2-sign-in',
  templateUrl: './sign-in.component.html',
  styleUrls: ['./sign-in.component.scss'],
  animations: [
    trigger('toggleServer', [
      transition(':enter', [
        style({
          opacity: 0,
          maxHeight: 0,
        }),
        animate(
          '0.3s 0.0s cubic-bezier(0.4, 0.0, 1, 1)',
          style({
            opacity: 1,
            maxHeight: '30em',
          })
        ),
      ]),
      transition(':leave', [
        style({
          opacity: 1,
          maxHeight: '30em',
        }),
        animate(
          '0.5s 0.1s cubic-bezier(0.4, 0.0, 1, 1)',
          style({
            opacity: 0,
            maxHeight: 0,
          })
        ),
      ]),
    ]),
  ],
})
export class SignInComponent implements OnInit {
  constructor(private http: HttpClient, private userService: UserService) {}

  server = 'https://localhost:5001';
  knownServer: string[] = [];
  username = '';
  password = '';
  rememberMe = true;
  error = '';

  @Input()
  signInOnline = false;

  @Output()
  signInSuccess = new EventEmitter<boolean>();

  @Output()
  resetPassword = new EventEmitter<boolean>();

  @ViewChild('signInButton')
  signInButton!: MatButton;

  userNameFormControl = new FormControl('', [Validators.required]);

  passwordFormControl = new FormControl('', [Validators.required]);

  reachServer: AsyncValidatorFn = async (
    control: AbstractControl
  ): Promise<ValidationErrors> => {
    let server = control.value as string;
    server = server.endsWith('/') ? server : `${server}/`;
    let ping = new PingDto();
    try {
      ping = await this.http
        .post<PingDto>(`/api/server/test`, {
          Server: server,
        })
        .toPromise();
    } catch (error) {
      // Let this error go unnoticed
    }
    if (ping.success) {
      return {};
    }
    return {
      notReachable: true,
    };
  };

  // eslint-disable-next-line @typescript-eslint/member-ordering
  serverFormControl = new FormControl(
    '',
    [Validators.required, this.validUrl],
    [this.reachServer]
  );

  async ngOnInit(): Promise<void> {
    // Get predefined values from server
    this.knownServer = await this.http
      .get<string[]>('/api/configuration/known-server')
      .toPromise();
    if (this.knownServer.length < 1) {
      this.knownServer.push(this.server);
    }
  }

  validUrl(control: AbstractControl): ValidationErrors {
    const urlRegex =
      /((([A-Za-z]{3,9}:(?:\/\/)?)(?:[\-;:&=\+\$,\w]+@)?[A-Za-z0-9\.\-]+|(?:www\.|[\-;:&=\+\$,\w]+@)[A-Za-z0-9\.\-]+)((?:\/[\+~%\/\.\w\-_]*)?(?::\d+)?\??(?:[\-\+=&;%@\.\w_]*)#?(?:[\.\!\/\\\w]*))?)/;
    if (urlRegex.test(control.value)) {
      return {};
    }
    return {
      valid: true,
    };
  }

  async passwordSignIn(): Promise<void> {
    this.signInButton.disabled = true;
    this.error = '';
    try {
      await this.userService.SignInWithPassword(
        this.userNameFormControl.value,
        this.passwordFormControl.value,
        true,
        this.serverFormControl.value,
        this.signInOnline
      );
      this.signInSuccess.emit(true);
    } catch (error) {
      const responseError = error as HttpErrorResponse;
      console.error(responseError);
      if (responseError.status === 503) {
        this.error = 'Application.SignIn.Errors.ServerUnreachable';
      } else {
        this.error = 'Application.SignIn.Errors.PasswordSignInFailed';
      }
    } finally {
      this.signInButton.disabled = false;
    }
  }

  passwordRecovery(): void {
    this.resetPassword.emit(true);
  }
}
