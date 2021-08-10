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
import { CoreModule } from '../core/core.module';
import { SharedModule } from '../shared/shared.module';
import { ParticipantRoutingModule } from './participant-routing.module';
import { ParticipantsOverviewComponent } from './participants-overview/participants-overview.component';
import { SingleProjectOverviewComponent } from './single-project-overview/single-project-overview.component';
import { MultiProjectOverviewComponent } from './multi-project-overview/multi-project-overview.component';
import { MatCardModule } from '@angular/material/card';
import { MatGridListModule } from '@angular/material/grid-list';
import { ParticipantListItemComponent } from './participant-list-item/participant-list-item.component';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatInputModule } from '@angular/material/input';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CreateParticipantComponent } from './create-participant/create-participant.component';
import { MatSelectModule } from '@angular/material/select';
import { ColorPickerModule } from 'ngx-color-picker';
import { MatChipsModule } from '@angular/material/chips';
import { ParticipantsDisplayComponent } from './participants-display/participants-display.component';
import { ParticipantDisplayComponent } from './participant-display/participant-display.component';
import { ProjectDisplayComponent } from './project-display/project-display.component';
import { ParticipantSearchComponent } from './participant-search/participant-search.component';
import { CreateFabButtonComponent } from './create-fab-button/create-fab-button.component';
import { CloseEditorButtonComponent } from './close-editor-button/close-editor-button.component';
import { ParticipantItemComponent } from './participant-item/participant-item.component';
import { ParticipantComponent } from './participant/participant.component';
import { MatTabsModule } from '@angular/material/tabs';

@NgModule({
  declarations: [
    ParticipantsOverviewComponent,
    SingleProjectOverviewComponent,
    MultiProjectOverviewComponent,
    ParticipantItemComponent,
    ParticipantListItemComponent,
    CreateParticipantComponent,
    ParticipantsDisplayComponent,
    ParticipantDisplayComponent,
    ProjectDisplayComponent,
    ParticipantSearchComponent,
    CreateFabButtonComponent,
    CloseEditorButtonComponent,
    ParticipantComponent,
  ],
  imports: [
    CommonModule,
    ParticipantRoutingModule,
    CoreModule,
    SharedModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatGridListModule,
    MatExpansionModule,
    MatInputModule,
    ColorPickerModule,
    MatSelectModule,
    MatChipsModule,
    MatTabsModule,
  ],
  exports: [ParticipantsOverviewComponent, ParticipantsDisplayComponent],
})
export class ParticipantModule {}
