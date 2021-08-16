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

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatAnimatedIconComponent } from './mat-animated-icon/mat-animated-icon.component';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { A11yModule } from '@angular/cdk/a11y';
import { MatBadgeModule } from '@angular/material/badge';
import { OverviewSpacerComponent } from './overview-spacer/overview-spacer.component';
import { MatPaginatorIntl } from '@angular/material/paginator';
import { PaginatorIntl } from './paginator-intl';
import { ParticipantNameComponent } from './participant-name/participant-name.component';
import { ParticipantProjectComponent } from './participant-project/participant-project.component';
import { ParticipantTintComponent } from './participant-tint/participant-tint.component';
import { ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { CoreModule } from '../core/core.module';
import { ColorPickerModule } from 'ngx-color-picker';
import { MatSelectModule } from '@angular/material/select';
import { IconChooserComponent } from './icon-chooser/icon-chooser.component';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatGridListModule } from '@angular/material/grid-list';

@NgModule({
  declarations: [
    MatAnimatedIconComponent,
    OverviewSpacerComponent,
    ParticipantNameComponent,
    ParticipantProjectComponent,
    ParticipantTintComponent,
    IconChooserComponent,
  ],
  imports: [
    CommonModule,
    MatIconModule,
    MatTooltipModule,
    ReactiveFormsModule,
    MatInputModule,
    CoreModule,
    ColorPickerModule,
    MatSelectModule,
    MatExpansionModule,
    MatButtonModule,
    MatGridListModule,
    MatMenuModule,
  ],
  exports: [
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatAnimatedIconComponent,
    MatMenuModule,
    MatButtonToggleModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatToolbarModule,
    MatListModule,
    MatDividerModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatAutocompleteModule,
    A11yModule,
    MatBadgeModule,
    OverviewSpacerComponent,
    ParticipantNameComponent,
    ParticipantProjectComponent,
    ParticipantTintComponent,
    IconChooserComponent,
  ],
  providers: [
    {
      provide: MatPaginatorIntl,
      useClass: PaginatorIntl,
    },
  ],
})
export class SharedModule {}
