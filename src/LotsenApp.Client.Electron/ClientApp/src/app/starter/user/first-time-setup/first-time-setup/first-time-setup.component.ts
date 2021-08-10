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

import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { STEPPER_GLOBAL_OPTIONS } from '@angular/cdk/stepper';
import {
  MatHorizontalStepper,
  MatVerticalStepper,
} from '@angular/material/stepper';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BreakpointObserver } from '@angular/cdk/layout';
import { Subscription } from 'rxjs';

@Component({
  selector: 'la2-first-time-setup',
  templateUrl: './first-time-setup.component.html',
  styleUrls: ['./first-time-setup.component.scss'],
  providers: [
    {
      provide: STEPPER_GLOBAL_OPTIONS,
      useValue: { displayDefaultIndicatorType: false, showError: true },
    },
  ],
})
export class FirstTimeSetupComponent implements OnInit, OnDestroy {
  @ViewChild(MatHorizontalStepper)
  horizontalStepper!: MatHorizontalStepper;

  @ViewChild(MatVerticalStepper)
  verticalStepper!: MatHorizontalStepper;

  under500 = false;
  private breakPointSubscriptions: Subscription[] = [];

  constructor(
    private http: HttpClient,
    private router: Router,
    private breakpointObserver: BreakpointObserver
  ) {}

  ngOnInit(): void {
    this.breakPointSubscriptions.push(
      this.breakpointObserver
        .observe(['(max-width: 500px)'])
        .subscribe((state) => {
          this.under500 = state.matches;
        })
    );
  }

  ngOnDestroy(): void {
    this.breakPointSubscriptions.forEach((s) => s.unsubscribe());
  }

  stepForward(index: number): void {
    if (this.under500) {
      if (this.verticalStepper.selectedIndex === index) {
        this.verticalStepper.next();
      }
    } else {
      if (this.horizontalStepper.selectedIndex === index) {
        this.horizontalStepper.next();
      }
    }
  }

  reset(): void {
    // Do reset the stepper
  }

  async complete(): Promise<void> {
    await this.http
      .get('/api/configuration/user/first-time-complete')
      .toPromise();
    await this.router.navigate(['/www/participant']);
  }
}
