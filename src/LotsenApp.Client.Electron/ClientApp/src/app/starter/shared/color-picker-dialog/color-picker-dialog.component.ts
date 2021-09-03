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
  Inject,
  OnInit,
  ViewChild,
} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { DialogData } from './dialog-data';
import { ColorPickerComponent } from '../color-picker/color-picker.component';
import { MatTabChangeEvent, MatTabGroup } from '@angular/material/tabs';

@Component({
  selector: 'la2-color-picker-dialog',
  templateUrl: './color-picker-dialog.component.html',
  styleUrls: ['./color-picker-dialog.component.scss'],
})
export class ColorPickerDialogComponent implements OnInit, AfterViewInit {
  predefinedColors: string[] = [];
  initialColor: string = '';

  resultColor?: string;

  @ViewChild('picker')
  picker!: ColorPickerComponent;
  @ViewChild(MatTabGroup)
  group!: MatTabGroup;

  constructor(
    public dialogRef: MatDialogRef<ColorPickerDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData
  ) {}

  ngOnInit(): void {
    this.initialColor = this.data.initialColor;
    this.predefinedColors = this.data.predefinedColors;
  }

  ngAfterViewInit() {
    if (this.predefinedColors.length) {
      return;
    }
    setTimeout(() => (this.group.selectedIndex = 1), 175);
    this.draw();
  }

  draw() {
    if (this.picker) {
      this.picker.draw();
    }
  }

  confirm() {
    this.dialogRef.close(this.resultColor);
  }

  cancel() {
    this.dialogRef.close();
  }

  updateColor(newColor: string) {
    this.resultColor = newColor;
  }

  updateInitialColor(change: MatTabChangeEvent) {
    if (this.resultColor) {
      this.initialColor = this.resultColor;
    }
  }

  updateColorAndCloseDialog(newColor: string) {
    this.updateColor(newColor);
    this.confirm();
  }
}
