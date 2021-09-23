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
  ChangeDetectorRef,
  ChangeDetectionStrategy,
  OnDestroy,
} from '@angular/core';
import { environment } from 'src/environments/environment';
import { Subscription } from 'rxjs';
import { Router, NavigationEnd } from '@angular/router';
import { ElectronService } from '../core/electron.service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'la2-debug',
  templateUrl: './debug.component.html',
  styleUrls: ['./debug.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DebugComponent implements OnInit, OnDestroy {
  production = environment.production;
  currentRoute = '<no-route>';
  heapMemory = 0;
  memoryUsage = 0;
  cpuUsage = 0;
  private routerSubscription!: Subscription;
  private memoryInterval!: number;
  private previousCpuTime = 0;
  private previousUpTime = 0;
  private cpuCount = 1;

  constructor(
    private router: Router,
    private electronService: ElectronService,
    private changeDetector: ChangeDetectorRef,
    private http: HttpClient
  ) {}

  async ngOnInit(): Promise<void> {
    this.changeDetector.detach();
    if (this.production) {
      return;
    }
    this.cpuCount = await this.http
      .get<number>('/api/configuration/cpu-count')
      .toPromise();
    this.routerSubscription = this.router.events.subscribe((routerEvent) => {
      if (routerEvent instanceof NavigationEnd) {
        this.currentRoute = routerEvent.url;
        this.changeDetector.detectChanges();
      }
    });
    this.memoryInterval = setInterval(async () => {
      // console.log(this.electronService.debug);
      const memoryUsage = this.electronService.debug.memoryUsage() as any;
      // console.log(memoryUsage);
      this.heapMemory = memoryUsage.totalHeapSize * 1000;
      this.memoryUsage = memoryUsage.usedHeapSize * 1000;
      const cpuUsage = this.electronService.debug.cpuUsage() as any;
      // console.log(cpuUsage);
      // const cpuTime = cpuUsage.user - this.previousCpuTime; // cpu time in micro seconds
      // this.previousCpuTime = cpuUsage.user;
      const upTime = this.electronService.debug.uptime() * 1000;
      // const processTime = upTime - this.previousUpTime; // uptime in micro seconds
      // this.cpuUsage = cpuTime / this.cpuCount / processTime;
      this.cpuUsage = (cpuUsage.percentCPUUsage * 100) / this.cpuCount;
      // this.previousUpTime = upTime;
      this.changeDetector.detectChanges();
    }, 1000);
  }

  ngOnDestroy(): void {
    if (this.production) {
      return;
    }
    this.routerSubscription.unsubscribe();
    clearInterval(this.memoryInterval);
  }
}
