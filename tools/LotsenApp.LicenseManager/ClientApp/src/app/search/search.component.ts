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

import {Component, Input, OnInit, Output, EventEmitter, ViewChild} from '@angular/core';
import {MatInput} from "@angular/material/input";

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss']
})
export class SearchComponent implements OnInit {

  _searchableList: {[key: string]: any}[] = [];

  @ViewChild(MatInput)
  input!: MatInput;

  @Input()
  set searchableList(value: {[key: string]: any}[]) {
    if(!value) {
      return;
    }
    this._searchableList = value;
    this.search(this.input?.value ?? '');
  }

  @Output()
  filteredList = new EventEmitter<any[]>();

  constructor() { }

  ngOnInit(): void {
  }

  search(keyword: string) {
    if(!keyword) {
      this.filteredList.emit(this._searchableList);
      return;
    }
    const searchPhrases = keyword.toLowerCase().split(' ').filter(s => s);
    const filteredList = this._searchableList.filter(l => {
      let values = '';
      for(const key of Object.keys(l)) {
        try {
          values += (l[key]?.toString() ?? '').toLowerCase() + ' ';
        } catch(error) {
          console.error('Error in ', l, key, error);
        }
      }
      const keyPhrases = values.split(' ').filter(s => s);
      return searchPhrases.every(s => keyPhrases.some(ks => ks.includes(s)));
    });
    this.filteredList.emit(filteredList);
  }

}
