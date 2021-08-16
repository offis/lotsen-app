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
import { HttpClient } from '@angular/common/http';
import { IParticipant } from './iparticipant';
import { CreateResponse } from './create-response';
import { CreateParticipantDto } from './create-participant-dto';
import { Observable, Subject } from 'rxjs';
import { ParticipantDocument } from './participant-document';
import { DocumentDto } from './document-dto';
import { DocumentCreation } from './document-creation';
import { IdResponse } from './id-response';
import { DocumentDeletion } from './document-deletion';
import { Displayable } from './displayable';
import { DocumentValue } from './document-value';
import { GroupValue } from './group-value';
import { DocumentRename } from './document-rename';
import { SpecificDocumentDto } from './specific-document-dto';
import { HeaderEntryDto } from './header-entry-dto';
import { HeaderDto } from './header-dto';
import { HeaderEditDto } from './header-edit-dto';

@Injectable({
  providedIn: 'root',
})
export class ParticipantService {
  private createSubject = new Subject<string>();
  private deleteSubject = new Subject<string>();
  private createDocumentSubject = new Subject<DocumentCreation>();
  private deleteDocumentSubject = new Subject<DocumentDeletion>();
  private saveSubject = new Subject<string>();
  private beforeSavingSubject = new Subject<string>();
  private documentRenameSubject = new Subject<DocumentRename>();

  get ParticipantCreated(): Observable<string> {
    return this.createSubject;
  }

  get ParticipantDeleted(): Observable<string> {
    return this.deleteSubject;
  }

  get DocumentCreated(): Observable<DocumentCreation> {
    return this.createDocumentSubject;
  }

  get DocumentDeleted(): Observable<DocumentDeletion> {
    return this.deleteDocumentSubject;
  }

  get DocumentsSaved(): Observable<string> {
    return this.saveSubject;
  }

  get BeforeSaving(): Observable<string> {
    return this.beforeSavingSubject;
  }

  get DocumentRenamed(): Observable<DocumentRename> {
    return this.documentRenameSubject;
  }

  constructor(private httpClient: HttpClient) {}

  public async GetParticipants(): Promise<IParticipant[]> {
    return await this.httpClient
      .get<IParticipant[]>('/api/participants')
      .toPromise();
  }

  public async GetParticipant(participantId: string): Promise<IParticipant> {
    return await this.httpClient
      .get<IParticipant>('/api/participants/' + participantId)
      .toPromise();
  }

  public async CreateParticipant(
    participant: CreateParticipantDto
  ): Promise<CreateResponse> {
    const response = await this.httpClient
      .post<CreateResponse>('/api/participants', participant)
      .toPromise();
    this.createSubject.next(response.id);
    return response;
  }

  public async DeleteParticipant(participant: string): Promise<void> {
    await this.httpClient
      .delete('/api/participants/' + participant)
      .toPromise();
    this.deleteSubject.next(participant);
  }

  public async GetParticipantDocuments(
    participantId: string
  ): Promise<ParticipantDocument> {
    return await this.httpClient
      .get<ParticipantDocument>(
        '/api/participants/' + participantId + '/document'
      )
      .toPromise();
  }

  public async GetParticipantDocument(
    participantId: string,
    documentId: string
  ): Promise<SpecificDocumentDto> {
    return await this.httpClient
      .get<SpecificDocumentDto>(
        `/api/participants/${participantId}/document/${documentId}`
      )
      .toPromise();
  }

  public async DeleteDocument(
    dto: DocumentDto,
    participantId: string
  ): Promise<void> {
    await this.httpClient
      .delete(`/api/participants/${participantId}/document/${dto.id}`)
      .toPromise();
    this.deleteDocumentSubject.next({
      documentId: dto.id,
      participantId,
    });
  }

  public async CreateDocument(
    dto: Displayable,
    projectId: string,
    participantId: string,
    documentName: string | null = null,
    parentDocument: string | null = null
  ): Promise<IdResponse> {
    const idResponse = await this.httpClient
      .post<IdResponse>(`/api/participants/${participantId}/document`, {
        documentId: dto.id,
        name: documentName ?? dto.name,
        parentDocumentId: parentDocument,
      })
      .toPromise();
    this.createDocumentSubject.next({
      displayable: {
        id: idResponse.id,
        documentId: dto.id,
        projectId: projectId,
        name: documentName ?? dto.name,
        isDelta: true,
        documents: [],
      },
      participantId,
      parentDocumentId: parentDocument,
    });
    return idResponse;
  }

  public async ReOrderDocuments(
    participantId: string,
    dtos: DocumentDto[]
  ): Promise<void> {
    await this.httpClient
      .post(`/api/participants/${participantId}/document/reorder`, dtos)
      .toPromise();
  }

  public async CommitChanges(participantId: string): Promise<void> {
    this.beforeSavingSubject.next(participantId);
    await this.httpClient
      .get(`/api/participants/${participantId}/save`)
      .toPromise();
    this.saveSubject.next(participantId);
  }

  public async SaveChanges(
    participantId: string,
    dto: DocumentValue
  ): Promise<void> {
    await this.httpClient
      .put(`/api/participants/${participantId}/document`, dto)
      .toPromise();
  }

  public async GetDocumentValues(
    participantId: string,
    projectId: string,
    documentId: string
  ): Promise<DocumentValue> {
    return await this.httpClient
      .get<DocumentValue>(
        `/api/participants/${participantId}/document/${documentId}/values`
      )
      .toPromise();
  }

  public async CreateGroup(
    participantId: string,
    projectId: string,
    documentId: string,
    groupId: string,
    parentGroup: string | null = null
  ): Promise<IdResponse> {
    return await this.httpClient
      .post<IdResponse>(`/api/participants/${participantId}/group`, {
        documentId,
        groupId,
        parentGroupId: parentGroup,
      })
      .toPromise();
  }

  public async RemoveGroup(
    participantId: string,
    projectId: string,
    documentId: string,
    groupId: string
  ): Promise<void> {
    await this.httpClient
      .delete<IdResponse>(
        `/api/participants/${participantId}/document/${documentId}/group/${groupId}`
      )
      .toPromise();
  }

  public async ReorderGroup(
    participantId: string,
    projectId: string,
    documentId: string,
    groups: GroupValue[]
  ): Promise<void> {
    await this.httpClient
      .put<IdResponse>(
        `/api/participants/${participantId}/document/${documentId}/group`,
        groups
      )
      .toPromise();
  }

  public GetHeaderEntryDtos(): Promise<HeaderEntryDto[]> {
    return this.httpClient
      .get<HeaderEntryDto[]>(`/api/participants/header`)
      .toPromise();
  }

  public async AddHeaderEntry(dto: HeaderDto): Promise<void> {
    await this.httpClient.post(`/api/participants/header`, dto).toPromise();
  }

  public async RemoveHeaderEntry(dto: HeaderDto): Promise<void> {
    await this.httpClient.put(`/api/participants/header`, dto).toPromise();
  }

  public RenameDocument(
    participantId: string,
    documentId: string,
    documentName: string
  ): void {
    this.documentRenameSubject.next({
      participantId: participantId,
      documentId: documentId,
      newName: documentName,
    });
  }

  public UpdateHeader(dto: HeaderEditDto) {
    return this.httpClient
      .post(`/api/participants/${dto.participantId}/header`, dto)
      .toPromise();
  }

  public CopyDocumentValues(
    participantId: string,
    documentId: string,
    documentId2: string,
    preserve: boolean
  ) {
    return this.httpClient
      .get(
        `/api/participants/${participantId}/document/${documentId}/copy/${documentId2}?preserve=${preserve}`
      )
      .toPromise();
  }
}
