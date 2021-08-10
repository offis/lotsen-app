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
  AfterViewInit,
  Component,
  ElementRef,
  OnInit,
  ViewChild,
} from '@angular/core';
import { ProjectService } from '../../participant/project.service';
import { DataDefinition } from '../../participant/data-definition';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { MatSort } from '@angular/material/sort';
import { Subject } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'la2-user-project',
  templateUrl: './user-project.component.html',
  styleUrls: ['./user-project.component.scss'],
})
export class UserProjectComponent implements OnInit, AfterViewInit {
  dataDefinitions = new MatTableDataSource<DataDefinition>([]);

  displayedColumns = ['id', 'name', 'version', 'locales', 'isParticipant'];
  page = 0;
  itemsPerPage = 10;
  @ViewChild(MatPaginator)
  paginator!: MatPaginator;
  @ViewChild(MatSort)
  sort!: MatSort;
  @ViewChild('fileUpload')
  fileUpload!: ElementRef;

  private fileObservable = new Subject<{
    fileName: File;
    fileContent: string;
  }>();

  constructor(
    private projectService: ProjectService,
    private snackBar: MatSnackBar,
    private translate: TranslateService
  ) {}

  async ngOnInit() {
    await this.updateDataFormats();
  }

  async updateDataFormats() {
    const definitions = await this.projectService.GetDataDefinitions();
    const lastElement = this.displayedColumns[this.displayedColumns.length - 1];
    if (
      definitions.every((d) => !d.isParticipant) &&
      lastElement === 'isParticipant'
    ) {
      this.displayedColumns.pop();
    }
    if (
      definitions.some((d) => d.isParticipant) &&
      lastElement !== 'isParticipant'
    ) {
      this.displayedColumns.push('isParticipant');
    }
    this.dataDefinitions.data = definitions;
  }

  ngAfterViewInit() {
    this.dataDefinitions.paginator = this.paginator;
    this.dataDefinitions.sort = this.sort;
  }

  readFile(event: Event): void {
    if (
      this.fileUpload.nativeElement.files &&
      this.fileUpload.nativeElement.files.length > 0
    ) {
      const file = this.fileUpload.nativeElement.files[0];
      const fileReader = new FileReader();
      fileReader.onload = (evt) => {
        const fileContent = evt.target?.result as string;
        this.fileObservable.next({ fileName: file, fileContent });
      };
      fileReader.readAsText(file);
    }
  }

  async uploadDataFormat() {
    const subscription = this.fileObservable.subscribe(async (next) => {
      try {
        const project = JSON.parse(next.fileContent);
        await this.projectService.AddDataFormat(project);
        const dismiss = await this.translate
          .get('Application.Errors.Dismiss')
          .toPromise();
        const text = await this.translate
          .get('Application.UserProject.UploadSuccessful')
          .toPromise();
        this.snackBar.open(text, dismiss);
        await this.updateDataFormats();
      } finally {
        subscription.unsubscribe();
      }
    });
    this.fileUpload.nativeElement.click();
  }

  async uploadI18N() {
    const subscription = this.fileObservable.subscribe(async (next) => {
      try {
        const fileName = next.fileName.name;
        const parts = fileName.replace('.json', '').split('_');
        await this.projectService.AddI18N(parts[0], parts[1], next.fileContent);
        const dismiss = await this.translate
          .get('Application.Errors.Dismiss')
          .toPromise();
        const text = await this.translate
          .get('Application.UserProject.UploadSuccessful')
          .toPromise();
        this.snackBar.open(text, dismiss);
        await this.updateDataFormats();
      } finally {
        subscription.unsubscribe();
      }
    });
    this.fileUpload.nativeElement.click();
  }
}
