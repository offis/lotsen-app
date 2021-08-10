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
  OnDestroy,
  HostListener,
  ViewChild,
} from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { LanguageDto } from './language-dto';
import { TranslateService } from '@ngx-translate/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { UserService } from '../../core/user.service';
import { Subscription } from 'rxjs';
import { MatMenuTrigger } from '@angular/material/menu';

@Component({
  selector: 'la2-language-selector',
  templateUrl: './language-selector.component.html',
  styleUrls: ['./language-selector.component.scss'],
})
export class LanguageSelectorComponent implements OnInit, OnDestroy {
  currentLanguage = 'de';
  environment = environment;
  isSignedIn = false;
  subscription!: Subscription;
  showBadge = false;

  @ViewChild(MatMenuTrigger)
  menuTrigger!: MatMenuTrigger;

  constructor(
    private http: HttpClient,
    private translate: TranslateService,
    private snackBar: MatSnackBar,
    private userService: UserService
  ) {
    this.translate.setDefaultLang('de');
    this.translate.use('de');
  }

  @HostListener('window:keydown', ['$event'])
  async handleKeyDown(event: KeyboardEvent): Promise<void> {
    if (event.key === 'Alt') {
      this.showBadge = true;
    }

    if (event.altKey && event.key === '5') {
      this.menuTrigger.openMenu();
    }
  }

  @HostListener('window:keyup', ['$event'])
  handleKeyUp(event: KeyboardEvent): void {
    if (event.key === 'Alt') {
      this.showBadge = false;
    }
  }

  async ngOnInit(): Promise<void> {
    this.subscription = this.userService.IsSignedIn.subscribe((next) => {
      this.isSignedIn = next;
    });
    this.translate.use(this.currentLanguage);
    try {
      const answer = await this.http
        .get<LanguageDto>('/api/configuration/user/language')
        .toPromise();
      this.currentLanguage = answer.language;
      this.translate.use(this.currentLanguage);
    } catch (error) {
      // Let this error get unnoticed
    }
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  async updateLanguage(event: string): Promise<void> {
    try {
      const newLanguage = event;
      if (this.currentLanguage === newLanguage) {
        return;
      }
      this.currentLanguage = newLanguage;
      this.translate.use(newLanguage);
      await this.http
        .post('/api/configuration/user/language', { Language: newLanguage })
        .toPromise();
    } catch (error) {
      const errorResponse = error as HttpErrorResponse;
      if (errorResponse.status === 403) {
        const message = await this.translate
          .get('Application.LanguageSelector.Errors.LanguageSaveForbidden')
          .toPromise();
        const dismiss = await this.translate
          .get('Application.Errors.Dismiss')
          .toPromise();
        this.snackBar.open(message, dismiss, {
          duration: 2500,
        });
      }
    }
  }
}
