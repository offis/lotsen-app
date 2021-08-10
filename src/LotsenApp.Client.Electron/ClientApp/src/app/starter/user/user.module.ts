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

import { UserRoutingModule } from './user-routing.module';
import { CoreModule } from '../core/core.module';
import { SharedModule } from '../shared/shared.module';

import { DataPasswordComponent } from './data-password/data-password.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatInputModule } from '@angular/material/input';
import { UserOverviewComponent } from './user-overview/user-overview.component';
import { ReminderEditorComponent } from './reminder-editor/reminder-editor.component';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { NgxMaterialTimepickerModule } from 'ngx-material-timepicker';
import { ReminderListComponent } from './reminder-list/reminder-list.component';
import { MatCardModule } from '@angular/material/card';
import { DeleteReminderDialogComponent } from './delete-reminder-dialog/delete-reminder-dialog.component';
import { CreateReminderDialogComponent } from './create-reminder-dialog/create-reminder-dialog.component';
import { ParticipantListComponent } from './participant-list/participant-list.component';
import { ParticipantModule } from '../participant/participant.module';
import { GridsterModule } from 'angular-gridster2';
import { GridItemComponent } from './grid-item/grid-item.component';
import { EmptyGridItemComponent } from './empty-grid-item/empty-grid-item.component';
import { DashboardTileMenuComponent } from './dashboard-tile-menu/dashboard-tile-menu.component';
import { ProgramGridTileComponent } from './program-grid-tile/program-grid-tile.component';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { FullCalendarModule } from '@fullcalendar/angular';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';
import timeGridPlugin from '@fullcalendar/timegrid';
import listPlugin from '@fullcalendar/list';
import icalendarPlugin from '@fullcalendar/icalendar';
import { ViewReminderDialogComponent } from './view-reminder-dialog/view-reminder-dialog.component';
import { UserComponent } from './user/user.component';
import { MatTabsModule } from '@angular/material/tabs';
import { UserProjectComponent } from './user-project/user-project.component';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatChipsModule } from '@angular/material/chips';

FullCalendarModule.registerPlugins([
  dayGridPlugin,
  timeGridPlugin,
  listPlugin,
  interactionPlugin,
  icalendarPlugin,
]);

@NgModule({
  declarations: [
    DataPasswordComponent,
    UserOverviewComponent,
    ReminderEditorComponent,
    ReminderListComponent,
    DeleteReminderDialogComponent,
    CreateReminderDialogComponent,
    ParticipantListComponent,
    GridItemComponent,
    EmptyGridItemComponent,
    DashboardTileMenuComponent,
    ProgramGridTileComponent,
    ViewReminderDialogComponent,
    UserComponent,
    UserProjectComponent,
  ],
  imports: [
    CommonModule,
    UserRoutingModule,
    CoreModule,
    SharedModule,
    FormsModule,
    ReactiveFormsModule,
    MatCheckboxModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    NgxMaterialTimepickerModule.setLocale('de-DE'),
    MatCardModule,
    ParticipantModule,
    GridsterModule,
    DragDropModule,
    FullCalendarModule,
    MatTabsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatChipsModule,
  ],
  exports: [
    ReminderEditorComponent,
    ReminderListComponent,
    ParticipantListComponent,
  ],
})
export class UserModule {}
