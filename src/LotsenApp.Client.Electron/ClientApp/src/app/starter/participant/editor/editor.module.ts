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

import { EditorRoutingModule } from './editor-routing.module';
import { EditorComponent } from './editor/editor.component';
import { CoreModule } from '../../core/core.module';
import { SharedModule } from '../../shared/shared.module';
import { MatTabsModule } from '@angular/material/tabs';
import { ParticipantEditorComponent } from './participant-editor/participant-editor.component';
import { MatTreeModule } from '@angular/material/tree';
import { DocumentOverviewComponent } from './document-overview/document-overview.component';
import { DocumentEditorComponent } from './document-editor/document-editor.component';
import { DocumentCreateMenuComponent } from './document-create-menu/document-create-menu.component';
import { ParticipantDeleteConfirmComponent } from './participant-delete-confirm/participant-delete-confirm.component';
import { DocumentOverviewDisplayComponent } from './document-overview-display/document-overview-display.component';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { DocumentDeleteConfirmComponent } from './document-delete-confirm/document-delete-confirm.component';
import { StringEditorComponent } from './string-editor/string-editor.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ReactiveFormsModule } from '@angular/forms';
import { FieldEditorComponent } from './field-editor/field-editor.component';
import { BooleanEditorComponent } from './boolean-editor/boolean-editor.component';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { EnumEditorComponent } from './enum-editor/enum-editor.component';
import { MatSelectModule } from '@angular/material/select';
import { DateEditorComponent } from './date-editor/date-editor.component';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { GroupEditorComponent } from './group-editor/group-editor.component';
import { MatExpansionModule } from '@angular/material/expansion';
import { IntegerEditorComponent } from './integer-editor/integer-editor.component';
import { DoubleEditorComponent } from './double-editor/double-editor.component';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { ParticipantMetadataEditorComponent } from './participant-metadata-editor/participant-metadata-editor.component';

@NgModule({
  declarations: [
    EditorComponent,
    ParticipantEditorComponent,
    DocumentOverviewComponent,
    DocumentEditorComponent,
    DocumentCreateMenuComponent,
    ParticipantDeleteConfirmComponent,
    DocumentOverviewDisplayComponent,
    DocumentDeleteConfirmComponent,
    StringEditorComponent,
    FieldEditorComponent,
    BooleanEditorComponent,
    EnumEditorComponent,
    DateEditorComponent,
    GroupEditorComponent,
    IntegerEditorComponent,
    DoubleEditorComponent,
    ParticipantMetadataEditorComponent,
  ],
  imports: [
    CommonModule,
    EditorRoutingModule,
    CoreModule,
    SharedModule,
    MatTabsModule,
    MatTreeModule,
    DragDropModule,
    MatFormFieldModule,
    MatSlideToggleModule,
    MatInputModule,
    ReactiveFormsModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatExpansionModule,
    ScrollingModule,
  ],
})
export class EditorModule {}
