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
  EventEmitter,
  Output,
} from '@angular/core';
import { UserService } from '../../core/user.service';
import { Subscription } from 'rxjs';
import { ElectronService } from '../../core/electron.service';
import { BreakpointObserver } from '@angular/cdk/layout';
import { Router } from '@angular/router';

@Component({
  selector: 'la2-main-navigation',
  templateUrl: './main-navigation.component.html',
  styleUrls: ['./main-navigation.component.scss'],
})
export class MainNavigationComponent implements OnInit, OnDestroy {
  signedIn = false;
  signedInSubscription!: Subscription;
  showBadge = false;
  under500 = false;

  developerView = false;

  @Output()
  navigated = new EventEmitter<string>();

  private breakPointSubscriptions: Subscription[] = [];

  constructor(
    private userService: UserService,
    public electronService: ElectronService,
    private breakpointObserver: BreakpointObserver,
    private router: Router
  ) {}

  @HostListener('window:keydown', ['$event'])
  async handleKeyDown(event: KeyboardEvent): Promise<void> {
    if (event.key === 'Alt') {
      this.showBadge = true;
    }

    if (event.altKey && event.key === '1') {
      await this.router.navigate(['/www/home']);
    }

    if (event.altKey && event.key === '2') {
      await this.router.navigate(['/www/participant']);
    }
    if (event.altKey && event.key === '3') {
      await this.router.navigate(['/www/user']);
    }
    if (event.altKey && event.key === '9') {
      await this.router.navigate(['/www/about']);
    }
    if (event.code === 'F9') {
      this.developerView = !this.developerView;
    }
  }

  @HostListener('window:keyup', ['$event'])
  handleKeyUp(event: KeyboardEvent): void {
    if (event.key === 'Alt') {
      this.showBadge = false;
    }
  }

  ngOnInit(): void {
    this.signedInSubscription = this.userService.IsSignedIn.subscribe(
      (next) => {
        this.signedIn = next;
      }
    );
    this.breakPointSubscriptions.push(
      this.breakpointObserver
        .observe(['(max-width: 500px)'])
        .subscribe((state) => {
          this.under500 = state.breakpoints['(max-width: 500px)'];
        })
    );
  }

  ngOnDestroy(): void {
    this.signedInSubscription.unsubscribe();
    this.breakPointSubscriptions.forEach((s) => s.unsubscribe());
  }

  emitNavigation(target: string): void {
    this.navigated.emit(target);
  }
}
