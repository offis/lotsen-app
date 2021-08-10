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
  ChangeDetectorRef,
  HostListener,
  ViewChild,
} from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { UserService } from '../../core/user.service';
import { Subscription } from 'rxjs';
import { SignInDialogComponent } from '../sign-in-dialog/sign-in-dialog.component';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslateService } from '@ngx-translate/core';
import { MatMenuTrigger } from '@angular/material/menu';
import { Router } from '@angular/router';

@Component({
  selector: 'la2-user-display',
  templateUrl: './user-display.component.html',
  styleUrls: ['./user-display.component.scss'],
})
export class UserDisplayComponent implements OnInit, OnDestroy {
  isSignedIn = false;
  userName = '';
  isOnline = false;
  badgeHidden = true;

  @ViewChild(MatMenuTrigger)
  menuButton!: MatMenuTrigger;

  private subscription!: Subscription;
  constructor(
    public dialog: MatDialog,
    private userService: UserService,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
    private router: Router
  ) {}

  @HostListener('window:keydown', ['$event'])
  handleKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Alt') {
      this.badgeHidden = false;
    }
    if (event.altKey && event.key === '0') {
      this.openComponent();
    }
  }

  @HostListener('window:keyup', ['$event'])
  handleKeyUp(event: KeyboardEvent): void {
    if (event.key === 'Alt') {
      this.badgeHidden = true;
    }
  }

  async ngOnInit(): Promise<void> {
    this.subscription = this.userService.IsSignedIn.subscribe((next) => {
      this.isSignedIn = next;
      if (next) {
        const newUsername = this.userService.User?.name;
        if (newUsername) {
          this.userName = newUsername;
        } else {
          this.userName = '<unknown-name>';
        }
        this.isOnline = this.userService.User?.isOnline || false;
      }
    });
    try {
      await this.userService.SignInWithRefreshToken();
    } catch (error) {
      // let this error go unnoticed
    }
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  openSignInDialog(isOnline: boolean): void {
    this.dialog.open(SignInDialogComponent, {
      data: {
        signInOnline: isOnline,
        showSuccess: !isOnline,
      },
      autoFocus: true,
      restoreFocus: true,
    });
  }

  async signOut(): Promise<void> {
    try {
      await this.userService.SignOut();
    } catch (error) {
      const errorResponse = error as HttpErrorResponse;
      if (errorResponse.status === 403) {
        const message = await this.translate
          .get('Application.UserDisplay.Errors.SignOutForbidden')
          .toPromise();
        const dismiss = await this.translate
          .get('Application.Errors.Dismiss')
          .toPromise();
        this.snackBar.open(message, dismiss, {
          duration: 2500,
        });
      }
    } finally {
      await this.router.navigate(['/www']);
    }
  }

  openComponent(): void {
    if (this.isSignedIn) {
      this.menuButton.openMenu();
    } else {
      this.openSignInDialog(false);
    }
  }
}
