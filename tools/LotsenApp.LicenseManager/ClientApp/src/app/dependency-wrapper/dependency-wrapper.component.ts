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

import {ChangeDetectionStrategy, Component, Input, OnInit, ViewChild} from '@angular/core';
import {IDependencyInformation} from "../idependency-information";
import {Observable, Subject} from "rxjs";
import {PageEvent} from "@angular/material/paginator";
import {CdkVirtualScrollViewport} from "@angular/cdk/scrolling";

@Component({
  selector: 'app-dependency-wrapper',
  templateUrl: './dependency-wrapper.component.html',
  styleUrls: ['./dependency-wrapper.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DependencyWrapperComponent implements OnInit {

  private _value: IDependencyInformation[] = [];
  private _filteredValues: Subject<IDependencyInformation[]> = new Subject<IDependencyInformation[]>();

  @Input()
  set dependencies(value: IDependencyInformation[]) {
    this._value = value;
    this._filteredValues.next(value);
  }

  get dependencies() {
    return this._value;
  }

  @ViewChild(CdkVirtualScrollViewport)
  scrollViewPort!: CdkVirtualScrollViewport;

  get filteredDependencies():Observable<IDependencyInformation[]> {
    return this._filteredValues;
  }

  page = 1;
  itemsPerPage = 10;

  constructor() { }

  ngOnInit(): void {
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

  updateFilter(filteredList: IDependencyInformation[]) {
    this._filteredValues.next(filteredList);
  }

}
