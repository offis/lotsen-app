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

import { Component, Input, Output, EventEmitter } from '@angular/core';
import {
  animate,
  group,
  style,
  transition,
  trigger,
} from '@angular/animations';
import {ElectronService} from "../../core/electron.service";

@Component({
  selector: 'la2-sign-in-workflow',
  templateUrl: './sign-in-workflow.component.html',
  styleUrls: ['./sign-in-workflow.component.scss'],
  animations: [
    trigger('fadeInOut', [
      // state('show', style({
      //   opacity: 1,
      //   maxHeight: '30em',
      // })),
      // state('hide', style({
      //   opacity: 0,
      //   maxHeight: 0,
      // })),
      // transition('show => hide', [
      //   animate('0.3s 0.0s cubic-bezier(0.4, 0.0, 1, 1)')
      // ]),
      // transition('hide => show', [
      //   animate('0.5s 0.1s cubic-bezier(0.4, 0.0, 1, 1)')
      // ]),
      transition(':enter', [
        style({
          opacity: 0,
          maxHeight: 0,
          display: 'none',
        }),
        group([
          animate(
            '0s',
            style({
              display: 'flex',
            })
          ),
          animate(
            '0.5s 0.1s cubic-bezier(0.4, 0.0, 1, 1)',
            style({
              opacity: 1,
              maxHeight: '30em',
            })
          ),
        ]),
      ]),
      transition(':leave', [
        style({
          opacity: 1,
          maxHeight: '30em',
          display: 'flex',
        }),
        group([
          animate(
            '0.5s 0.1s cubic-bezier(0.4, 0.0, 1, 1)',
            style({
              opacity: 0,
              maxHeight: 0,
            })
          ),
          animate(
            '0.6s',
            style({
              display: 'none',
            })
          ),
        ]),
      ]),
    ]),
  ],
})
export class SignInWorkflowComponent {
  @Input()
  set signInOnline(value: boolean) {
    if (value) {
      this.showPasswordAuthentication();
    }
    this.defaultOnline = value;
  }
  defaultOnline = false;
  passwordAuthentication = false;
  passwordReset = false;
  authenticationMode = true;

  @Output()
  signInSuccess = new EventEmitter<boolean>();
  constructor() {}

  showPasswordAuthentication(): void {
    this.passwordAuthentication = true;
    this.passwordReset = false;
    this.authenticationMode = false;
  }

  success(): void {
    this.signInSuccess.emit(true);
  }

  showPasswordReset(): void {
    this.passwordAuthentication = false;
    this.passwordReset = true;
  }

  hidePasswordReset(): void {
    this.passwordReset = false;
    this.passwordAuthentication = true;
  }
}
