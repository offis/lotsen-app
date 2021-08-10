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
  Inject,
  Renderer2,
  HostListener,
  ViewChild,
} from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { MatButtonToggleChange } from '@angular/material/button-toggle';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ThemeDto } from './theme-dto';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { MatMenuTrigger } from '@angular/material/menu';

@Component({
  selector: 'la2-theme-selector',
  templateUrl: './theme-selector.component.html',
  styleUrls: ['./theme-selector.component.scss'],
})
export class ThemeSelectorComponent implements OnInit {
  currentTheme = 'light-theme';
  showBadge = false;

  @ViewChild(MatMenuTrigger)
  menuTrigger!: MatMenuTrigger;

  constructor(
    @Inject(DOCUMENT) private document: Document,
    private renderer: Renderer2,
    private http: HttpClient,
    private translate: TranslateService,
    private snackBar: MatSnackBar
  ) {}

  @HostListener('window:keydown', ['$event'])
  async handleKeyDown(event: KeyboardEvent): Promise<void> {
    if (event.key === 'Alt') {
      this.showBadge = true;
    }

    if (event.altKey && event.key === '4') {
      await this.menuTrigger.openMenu();
    }
  }

  @HostListener('window:keyup', ['$event'])
  handleKeyUp(event: KeyboardEvent): void {
    if (event.key === 'Alt') {
      this.showBadge = false;
    }
  }

  async ngOnInit(): Promise<void> {
    try {
      const answer = await this.http
        .get<ThemeDto>('/api/configuration/user/app-theme')
        .toPromise();
      this.currentTheme = answer.theme;
    } catch (error) {
      console.error('Cannot get theme value', error);
      const message = await this.translate
        .get('Application.ThemeSelector.Errors.ReceivingThemeNotPossible')
        .toPromise();
      const dismiss = await this.translate
        .get('Application.Errors.Dismiss')
        .toPromise();
      this.snackBar.open(message, dismiss, {
        duration: 2500,
      });
    } finally {
      this.renderer.addClass(this.document.body, this.currentTheme);
    }
  }

  async updateTheme(event: string): Promise<void> {
    if (event === this.currentTheme) {
      return;
    }
    try {
      const newTheme = event;
      this.renderer.removeClass(this.document.body, this.currentTheme);
      this.renderer.addClass(this.document.body, newTheme);
      this.currentTheme = newTheme;
      await this.http
        .post('/api/configuration/user/app-theme', { Theme: newTheme })
        .toPromise();
    } catch (error) {
      const errorResponse = error as HttpErrorResponse;
      if (errorResponse.status === 400) {
        const message = await this.translate
          .get('Application.ThemeSelector.Errors.ThemeSaveNotPossible')
          .toPromise();
        const dismiss = await this.translate
          .get('Application.Errors.Dismiss')
          .toPromise();
        this.snackBar.open(message, dismiss, {
          duration: 2500,
        });
      }
      if (errorResponse.status === 403) {
        const message = await this.translate
          .get('Application.ThemeSelector.Errors.ThemeSaveForbidden')
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
