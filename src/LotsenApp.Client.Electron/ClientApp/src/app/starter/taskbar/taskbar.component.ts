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
  EventEmitter,
  Output,
  HostListener,
  ViewChild,
  Input,
  OnChanges,
} from '@angular/core';
import { ElectronService } from '../core/electron.service';
import { MatAnimatedIconComponent } from '../shared/mat-animated-icon/mat-animated-icon.component';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Subscription } from 'rxjs';

@Component({
  selector: 'la2-taskbar',
  templateUrl: './taskbar.component.html',
  styleUrls: ['./taskbar.component.scss'],
})
export class TaskbarComponent implements OnInit, OnDestroy {
  isMaximized = false;
  @Output() toggleMenu = new EventEmitter<boolean>();
  menuState = false;

  @ViewChild(MatAnimatedIconComponent)
  menuIcon!: MatAnimatedIconComponent;

  under800 = false;
  under500 = false;

  private breakPointSubscriptions: Subscription[] = [];

  constructor(
    public electronService: ElectronService,
    private changeDetector: ChangeDetectorRef,
    private breakpointObserver: BreakpointObserver
  ) {}

  private listener = (evt: any, ...args: boolean[]) => {
    this.isMaximized = args[0];
    this.changeDetector.detectChanges();
  };

  ngOnInit(): void {
    if (this.electronService.isElectronApp) {
      this.electronService.ipcRenderer.on('window-maximized', this.listener);
    }
    this.breakPointSubscriptions.push(
      this.breakpointObserver
        .observe(['(max-width: 800px)', '(max-width: 500px)'])
        .subscribe((state) => {
          this.under800 = state.breakpoints['(max-width: 800px)'];
          this.under500 = state.breakpoints['(max-width: 500px)'];
        })
    );
  }

  ngOnDestroy(): void {
    this.electronService.ipcRenderer.removeListener(
      'window-maximized',
      this.listener
    );
    this.breakPointSubscriptions.forEach((s) => s.unsubscribe());
  }

  @HostListener('window:keydown', ['$event'])
  handleKeyDown(event: KeyboardEvent): void {
    if (event.code === 'F4' && event.altKey) {
      this.close();
    }

    if (event.code === 'F2') {
      this.openMenu();
    }

    if (event.key === 'Alt') {
      this.openMenu(true);
    }
  }

  @HostListener('window:keyup', ['$event'])
  handleKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Alt') {
      this.openMenu(false);
    }
  }

  close(): void {
    this.electronService.ipcRenderer.send('close-channel');
  }

  minimize(): void {
    this.electronService.ipcRenderer.send('minimize-channel');
  }

  maximize(): void {
    this.electronService.ipcRenderer.send('maximize-channel');
  }

  openMenu(desiredState: boolean | null = null): void {
    if (this.menuState === desiredState) {
      return;
    }
    this.menuState = !this.menuState;
    this.toggleMenu.emit(this.menuState);
    this.toggleMenuAnimation();
  }

  toggleMenuAnimation(): void {
    this.menuIcon.toggle();
  }
}
