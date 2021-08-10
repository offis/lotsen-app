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

import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { DocumentCreation } from './document-creation';
import { DocumentDeletion } from './document-deletion';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { IParticipant } from './iparticipant';
import { IProject } from './iproject';
import { CreateParticipantDto } from './create-participant-dto';
import { CreateResponse } from './create-response';
import { ParticipantDocument } from './participant-document';
import { Displayable } from './displayable';
import { DocumentDto } from './document-dto';
import { IdResponse } from './id-response';
import { DocumentValue } from './document-value';
import { DocumentDetail } from './document-detail';
import { GroupValue } from './group-value';
import { DataDefinition } from './data-definition';

@Injectable({
  providedIn: 'root',
})
export class ProjectService {
  constructor(private httpClient: HttpClient) {}

  public async GetProjects(): Promise<IProject[]> {
    return await this.httpClient.get<IProject[]>('/api/project').toPromise();
  }

  public async GetCreatableDocuments(
    projectId: string
  ): Promise<Displayable[]> {
    return await this.httpClient
      .get<Displayable[]>('/api/project/' + projectId + '/document')
      .toPromise();
  }

  public async GetDocument(
    projectId: string,
    documentId: string
  ): Promise<Displayable> {
    return await this.httpClient
      .get<Displayable>('/api/project/' + projectId + '/document/' + documentId)
      .toPromise();
  }

  public async GetSubDocuments(
    projectId: string,
    documentId: string
  ): Promise<Displayable[]> {
    return await this.httpClient
      .get<Displayable[]>(
        `/api/project/${projectId}/documentation-event/${documentId}`
      )
      .toPromise();
  }

  public async GetDocumentMetaData(
    projectId: string,
    documentId: string
  ): Promise<DocumentDetail> {
    try {
      return await this.httpClient
        .get<DocumentDetail>(
          `/api/project/${projectId}/document/${documentId}/detail`
        )
        .toPromise();
    } catch (error) {
      const errorResponse = error as HttpErrorResponse;
      if (errorResponse.status === 500) {
        console.error(errorResponse.message);
      }
      throw error;
    }
  }

  public async GetDataDefinitions(): Promise<DataDefinition[]> {
    return await this.httpClient
      .get<DataDefinition[]>('/api/project/detail')
      .toPromise();
  }

  public async AddDataFormat(project: any) {
    return await this.httpClient.post('/api/data-format', project).toPromise();
  }

  public async AddI18N(projectId: string, locale: string, i18n: string) {
    return await this.httpClient
      .post(`/api/data-format/${projectId}/i18n/${locale}`, { i18n })
      .toPromise();
  }

  public GetDocumentationProjects() {
    return this.httpClient
      .get<IProject[]>('/api/project/document')
      .toPromise();
  }
}
