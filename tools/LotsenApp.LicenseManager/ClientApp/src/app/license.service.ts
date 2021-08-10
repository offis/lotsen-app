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
import {HttpClient} from "@angular/common/http";
import {IDependencyInformation} from "./idependency-information";
import {ILicenseInformation} from "./ilicense-information";
import {Observable, Subject} from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class LicenseService {

  private _licenseSelectedSubject = new Subject<string>();
  private _licenseDetailSubject = new Subject<string>();

  public get LicenseSelected(): Observable<string> {
    return this._licenseSelectedSubject;
  }

  public get LicenseDetail(): Observable<string> {
    return this._licenseDetailSubject;
  }

  constructor(private httpClient: HttpClient) { }

  public async GetDependencies(): Promise<IDependencyInformation[]> {
    return await (this.httpClient.get<IDependencyInformation[]>('/license/association').toPromise());
  }

  public async GetLicenses(): Promise<ILicenseInformation[]> {
    return await (this.httpClient.get<ILicenseInformation[]>('/license/licenses').toPromise());
  }

  public SelectLicense(identifier: string): void {
    this._licenseSelectedSubject.next(identifier);
  }

  public ShowLicenseDetail(identifier: string): void {
    this._licenseDetailSubject.next(identifier);
  }

  public async UpdateLicense(license: ILicenseInformation): Promise<void> {
    await this.httpClient.post('/license', license).toPromise();
  }

  public async CreateLicenseHeader(): Promise<void> {
    await this.httpClient.get('/license/header').toPromise();
  }

  public async TheThing(): Promise<void> {
    await this.httpClient.get('/license/the-thing').toPromise();
  }


}
