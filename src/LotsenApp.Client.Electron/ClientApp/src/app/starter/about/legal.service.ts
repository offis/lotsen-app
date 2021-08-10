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
import { TranslateService } from '@ngx-translate/core';
import { IDirectDependency } from './idirect-dependency';

@Injectable({
  providedIn: 'root',
})
export class LegalService {
  public get DataProtectionUri(): string {
    const locale = this.translateService.currentLang;
    return `assets/legal/data-protection_${locale}.md`;
  }

  public get LegalNoticeUri(): string {
    const locale = this.translateService.currentLang;
    return `assets/legal/legal-notice_${locale}.md`;
  }

  public get ChangelogUri(): string {
    return '/api/changelog.md';
  }

  constructor(
    private httpClient: HttpClient,
    private translateService: TranslateService
  ) {}

  public LoadDataProtection(): Promise<string> {
    return this.httpClient
      .get<string>(this.DataProtectionUri, { responseType: 'text' as 'json' })
      .toPromise();
  }

  public LoadLegalNotice(): Promise<string> {
    return this.httpClient
      .get<string>(this.LegalNoticeUri, { responseType: 'text' as 'json' })
      .toPromise();
  }

  public LoadLicense(): Promise<string> {
    return this.httpClient
      .get<string>(`assets/legal/LICENSE.txt`, {
        responseType: 'text' as 'json',
      })
      .toPromise();
  }

  public LoadThirdPartyLicenses(): Promise<string> {
    return this.httpClient
      .get<string>(`assets/legal/THIRD_PARTY_LICENSE.txt`, {
        responseType: 'text' as 'json',
      })
      .toPromise();
  }

  public async LoadDirectDependencies(): Promise<IDirectDependency[]> {
    const response = await this.httpClient
      .get<IDirectDependency[]>('assets/legal/direct_dependencies.json')
      .toPromise();

    return response
      .map((r) => {
        const invalidUrl = r.RepositoryUrl?.startsWith('git+https') ?? false;
        r.RepositoryUrl = invalidUrl
          ? r.RepositoryUrl.substr(4)
          : r.RepositoryUrl;
        const gitUrl = r.RepositoryUrl?.startsWith('git');
        r.RepositoryUrl = gitUrl
          ? 'https' + r.RepositoryUrl.substr(3)
          : r.RepositoryUrl;
        return r;
      })
      .sort((r1, r2) => (r1.DependencyName <= r2.DependencyName ? -1 : 1));
  }
}
