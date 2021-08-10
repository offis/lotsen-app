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

import { Component, OnDestroy, OnInit } from '@angular/core';
import { GridsterConfig } from 'angular-gridster2';
import { environment } from '../../../../environments/environment';
import { Components } from '../../core/components.enum';
import { GridTile } from '../../core/grid-tile';
import { UserConfigurationService } from '../../core/user-configuration.service';
import { DashboardService } from '../dashboard.service';

@Component({
  selector: 'la2-user-overview',
  templateUrl: './user-overview.component.html',
  styleUrls: ['./user-overview.component.scss'],
})
export class UserOverviewComponent implements OnInit, OnDestroy {
  options!: GridsterConfig;
  dashboard: GridTile[] = [];

  constructor(
    private userConfigurationService: UserConfigurationService,
    private dashboardService: DashboardService
  ) {}

  async ngOnInit(): Promise<void> {
    this.options = {
      minCols: 1,
      maxCols: 100,
      minRows: 1,
      maxRows: 100,
      disableWarnings: environment.production,
      displayGrid: 'onDrag&Resize',
      draggable: {
        enabled: true,
      },
      resizable: {
        enabled: true,
      },
      itemChangeCallback: this.handleItemChanged.bind(this),
    };
    // Load configuration
    this.dashboard =
      await this.userConfigurationService.GetDashboardConfiguration();
  }

  async ngOnDestroy(): Promise<void> {
    // Save configuration
    await this.userConfigurationService.UpdateDashboardConfiguration(
      this.dashboard
    );
  }

  handleItemChanged(args: any) {
    this.dashboardService.UpdateDashboard();
  }

  deleteItem(index: number): void {
    this.dashboard.splice(index, 1);
  }

  addTile(): void {
    this.dashboard.push({
      x: 0,
      y: 0,
      rows: 1,
      cols: 1,
    });
  }

  updateType(index: number, type: number): void {
    this.dashboard[index].component = type as Components;
  }
}
