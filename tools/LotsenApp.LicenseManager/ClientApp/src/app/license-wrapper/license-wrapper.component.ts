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

import {Component, Input, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {Observable, Subject, Subscription} from "rxjs";
import {CdkVirtualScrollViewport} from "@angular/cdk/scrolling";
import {PageEvent} from "@angular/material/paginator";
import {ILicenseInformation} from "../ilicense-information";
import {LicenseService} from "../license.service";

@Component({
  selector: 'app-license-wrapper',
  templateUrl: './license-wrapper.component.html',
  styleUrls: ['./license-wrapper.component.scss']
})
export class LicenseWrapperComponent implements OnInit, OnDestroy {

  private _value: ILicenseInformation[] = [];
  private _filteredValues: Subject<ILicenseInformation[]> = new Subject<ILicenseInformation[]>();

  @Input()
  set licenses(value: ILicenseInformation[]) {
    this._value = value;
    this._filteredValues.next(value);
  }

  get licenses() {
    return this._value;
  }

  @ViewChild(CdkVirtualScrollViewport)
  scrollViewPort!: CdkVirtualScrollViewport;

  get filteredLicenses():Observable<ILicenseInformation[]> {
    return this._filteredValues;
  }

  page = 1;
  itemsPerPage = 10;

  private _licenseSelectionSubscription!: Subscription;

  constructor(private licenseService: LicenseService) { }

  ngOnInit(): void {
    this._licenseSelectionSubscription = this.licenseService.LicenseSelected.subscribe(i => this.selectLicense(i));
  }

  ngOnDestroy(): void {
    this._licenseSelectionSubscription.unsubscribe();
  }

  updatePage(evt: number) {
    this.page = Math.trunc(evt / this.itemsPerPage);
  }

  changePage(evt: PageEvent) {
    if(evt.pageIndex !== this.page) {
      this.page = evt.pageIndex;
      this.scrollViewPort.scrollToIndex(this.page * this.itemsPerPage, 'smooth')
    }
    if(evt.pageSize !== this.itemsPerPage) {
      this.itemsPerPage = evt.pageSize;
    }
  }

  updateFilter(filteredList: ILicenseInformation[]) {
    this._filteredValues.next(filteredList);
  }

  selectLicense(identifier: string): void {
    this._filteredValues.next(this._value);
    const index = this._value.findIndex(v => v.licenseId === identifier);
    this.scrollViewPort.scrollToIndex(index, 'smooth');
  }

}
