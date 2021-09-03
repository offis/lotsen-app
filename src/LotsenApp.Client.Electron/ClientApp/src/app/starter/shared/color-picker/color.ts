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

export class Color {
  red: number = -1; // [0;255]
  green: number = -1; // [0;255]
  blue: number = -1; // [0;255]
  alpha: number = -1; // [0;1]

  constructor(value: string) {
    if (value.startsWith('rgba')) {
      this.calculateFromRgbaColor(value);
    } else if (value.startsWith('#')) {
      this.calculateFromHexColor(value);
    }
  }

  private calculateFromRgbaColor(rgba: string) {
    const match = rgba.match(/rgba?\((.*)\)/);
    if (!match) {
      return;
    }
    const color = match[1].split(',').map(Number);
    this.red = Math.max(color[0], 0);
    this.green = Math.max(color[1], 0);
    this.blue = Math.max(color[2], 0);
    this.alpha = Math.max(color[3], 0);
  }

  private calculateFromHexColor(color: string) {
    if (!color.startsWith('#')) {
      return;
    }
    if (color.length === 4) {
      // #rgb
      const parts = color.split('');
      color =
        parts
          .map((p) => p + p)
          .join('')
          .substring(1) + 'ff';
    }
    if (color.length === 5) {
      // #rgba
      const parts = color.split('');
      color = parts
        .map((p) => p + p)
        .join('')
        .substring(1);
    }
    if (color.length === 7) {
      // #rrggbb
      color += 'ff';
    }
    if (color.length !== 9) {
      return;
    }
    const r = color.substring(1, 3);
    const g = color.substring(3, 5);
    const b = color.substring(5, 7);
    const a = color.substring(7, 9);
    this.red = parseInt(r, 16);
    this.green = parseInt(g, 16);
    this.blue = parseInt(b, 16);
    this.alpha = parseInt(a, 16) / 255;
  }

  toString() {
    return `rgba(${this.red}, ${this.green}, ${this.blue}, ${this.alpha})`;
  }

  toHexString() {
    const r = this.red.toString(16).padStart(2, '0');
    const g = this.green.toString(16).padStart(2, '0');
    const b = this.blue.toString(16).padStart(2, '0');
    const a = Math.trunc(this.alpha * 255)
      .toString(16)
      .padStart(2, '0');
    return `#${r}${g}${b}${a}`;
  }
}
