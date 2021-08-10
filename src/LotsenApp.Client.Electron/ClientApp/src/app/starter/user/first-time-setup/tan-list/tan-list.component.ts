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

import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

@Component({
  selector: 'la2-tan-list',
  templateUrl: './tan-list.component.html',
  styleUrls: ['./tan-list.component.scss'],
})
export class TanListComponent implements OnInit {
  @Input()
  tan: string | null = null;
  @Output()
  stepSuccess = new EventEmitter<boolean>();

  get requestUrl(): string {
    const requestParams = this.tan ? `?tan=${this.tan}` : '';
    return encodeURI(`/api/authorization/tan${requestParams}`);
  }
  completed = false;
  acknowledgement = false;
  constructor(private http: HttpClient) {}

  async ngOnInit(): Promise<void> {
    try {
      this.completed = await this.http
        .get<boolean>('/api/authorization/tan/exists')
        .toPromise();
      if (this.completed) {
        this.stepSuccess.emit(true);
      }
    } catch (error) {
      // Let this error go unnoticed
      this.completed = false;
    }
  }

  generateTan(): void {
    this.completed = true;
    this.stepSuccess.emit(true);
  }
}
