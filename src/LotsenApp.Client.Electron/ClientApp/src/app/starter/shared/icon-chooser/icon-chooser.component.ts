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
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Icons } from './icons';
import { IconCategory } from './icon-category';
import { FormControl } from '@angular/forms';
import { MatAccordion } from '@angular/material/expansion';
import { MatMenu, MatMenuTrigger } from '@angular/material/menu';

@Component({
  selector: 'la2-icon-chooser',
  templateUrl: './icon-chooser.component.html',
  styleUrls: ['./icon-chooser.component.scss'],
})
export class IconChooserComponent implements OnInit {
  private interalIcon?: string;
  @Input()
  set icon(value: string) {
    this.interalIcon = value;
    this.iconChange.emit(value);
  }

  get icon() {
    return this.interalIcon ?? '';
  }

  @Input()
  fallbackIcon = 'face';

  @Output()
  iconSelected = new EventEmitter<string>();
  @Output()
  iconChange = new EventEmitter<string>();

  @ViewChild(MatAccordion)
  categories!: MatAccordion;
  @ViewChild(MatMenuTrigger)
  menu!: MatMenuTrigger;
  @ViewChild('searchField')
  searchField!: ElementRef;

  icons: IconCategory[] = [];
  searchControl = new FormControl();

  private allIcons: IconCategory[] = [];
  private timeoutId?: number;

  constructor(private http: HttpClient) {}

  async ngOnInit(): Promise<void> {
    await this.initializeIcons();
    this.icons = this.allIcons;
  }

  async initializeIcons(): Promise<void> {
    const icons = await this.http
      .get<Icons>('assets/icons/mat-icons.json')
      .toPromise();
    const categories = Object.getOwnPropertyNames(icons);
    for (const category of categories) {
      this.allIcons.push({
        categoryName: category,
        icons: icons[category],
      });
    }
  }

  iconChanged(icon: string): void {
    if (icon === this.icon) {
      return;
    }
    this.icon = icon;
    this.iconSelected.emit(icon);
    this.menu.closeMenu();
  }

  nextPhrase(): void {
    this.icons = this.filterIcons(this.searchControl.value);
    if (this.timeoutId) {
      clearTimeout(this.timeoutId);
    }
    this.timeoutId = window.setTimeout(() => {
      if (this.searchControl.value) {
        this.categories.openAll();
      }
      this.timeoutId = undefined;
    }, 300);
  }

  filterIcons(phrase: string): IconCategory[] {
    if (!phrase?.trim()) {
      return this.allIcons;
    }
    return this.allIcons.map((category) => ({
      categoryName: category.categoryName,
      icons: category.icons.filter((icon) => icon.startsWith(phrase)),
    }));
  }

  resetPhrase(): void {
    this.searchControl.setValue('');
    this.categories.closeAll();
    this.icons = this.filterIcons('');
  }

  focusSearch(): void {
    setTimeout(() => this.searchField.nativeElement.focus());
  }
}
