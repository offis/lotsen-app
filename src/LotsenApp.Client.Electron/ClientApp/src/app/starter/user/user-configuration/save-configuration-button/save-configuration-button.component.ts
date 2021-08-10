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

import { Component, Input, OnInit } from '@angular/core';
import { UserConfigurationService } from '../../../core/user-configuration.service';
import { UserConfiguration } from '../../../core/user-configuration';
import { TranslateService } from '@ngx-translate/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'la2-save-configuration-button',
  templateUrl: './save-configuration-button.component.html',
  styleUrls: ['./save-configuration-button.component.scss'],
})
export class SaveConfigurationButtonComponent {
  @Input()
  userConfiguration!: UserConfiguration;

  working = false;

  constructor(
    private configurationService: UserConfigurationService,
    private translate: TranslateService,
    private snackBar: MatSnackBar
  ) {}

  async saveConfiguration() {
    this.working = true;
    try {
      await this.configurationService.SetUserConfiguration(
        this.userConfiguration
      );
      const message = await this.translate
        .get('Application.UserConfiguration.SaveSuccess')
        .toPromise();
      const dismiss = await this.translate
        .get('Application.Errors.Dismiss')
        .toPromise();
      this.snackBar.open(message, dismiss, {
        duration: 2500,
      });
    } finally {
      this.working = false;
    }
  }
}