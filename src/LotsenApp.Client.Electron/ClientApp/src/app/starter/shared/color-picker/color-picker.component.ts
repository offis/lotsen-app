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
  Input,
  EventEmitter,
  Output,
  ViewChild,
} from '@angular/core';
import { AlphaSliderComponent } from '../alpha-slider/alpha-slider.component';
import { Color } from './color';
import { ColorPaletteComponent } from '../color-palette/color-palette.component';
import { ColorSliderComponent } from '../color-slider/color-slider.component';

@Component({
  selector: 'la2-color-picker',
  templateUrl: './color-picker.component.html',
  styleUrls: ['./color-picker.component.scss'],
})
export class ColorPickerComponent {
  private internalColor: string = '#ff0000ff';
  private initialColor?: string;

  hue: string = this.calculateRgbaColor(this.internalColor).toString();
  brightness: string = this.hue;

  private internalAlpha = this.hue;
  get alpha() {
    return this.internalAlpha;
  }

  set alpha(value: string) {
    this.internalAlpha = value;
    this.calculateHexColor(value);
  }

  get color() {
    return this.internalColor;
  }

  @ViewChild('alphaSlider')
  alphaSlider!: AlphaSliderComponent;
  @ViewChild('palette')
  palette!: ColorPaletteComponent;
  @ViewChild('slider')
  slider!: ColorSliderComponent;

  @Input()
  set color(value: string) {
    if (!value) {
      return;
    }
    if (!this.initialColor) {
      this.initialColor = value;
      setTimeout(() => {
        const color = this.calculateRgbaColor(value);
        this.hue = color.toString();
        this.alphaSlider.setAlpha(color.alpha);
      }, 17);
    }
    this.internalColor = value;

    this.colorChange.emit(value);
  }
  @Output()
  colorChange = new EventEmitter<string>();

  strokeWidth = 2;
  offset = 5;
  constructor() {}

  calculateColor(value: string) {
    return new Color(value);
  }

  calculateHexColor(rgba: string) {
    const color = this.calculateColor(rgba);
    this.color = color.toHexString();
  }

  calculateRgbaColor(color: string) {
    return new Color(color);
  }

  updateColor(value: string) {
    console.log(value);
    const color = new Color(value);
    if (color.red < 0 || value.length < 9) {
      return;
    }
    this.initialColor = undefined;
    this.color = color.toHexString();
  }

  draw() {
    this.alphaSlider.draw();
    this.slider.draw();
    this.palette.draw();
  }
}
