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
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';

@Component({
  selector: 'la2-alpha-slider',
  templateUrl: './alpha-slider.component.html',
  styleUrls: ['./alpha-slider.component.scss'],
})
export class AlphaSliderComponent implements AfterViewInit {
  private internalColor: string = 'rgba(255, 255, 255, 1)';
  private internalHue = this.internalColor;

  @Input()
  strokeWidth = 1;
  @Input()
  offset = 10;

  @Input()
  set hue(value: string) {
    this.internalHue = value;
    this.draw();
    this.color = this.getColorAtPosition(this.selectedWidth, 0);
  }

  get hue() {
    return this.internalHue;
  }

  @Input()
  get color() {
    return this.internalColor;
  }

  set color(value: string) {
    this.internalColor = value;
    this.colorChange.emit(value);
  }

  @Output()
  colorChange: EventEmitter<string> = new EventEmitter();

  @ViewChild('canvas')
  canvas!: ElementRef<HTMLCanvasElement>;

  private context!: CanvasRenderingContext2D;
  private mouseDown = false;
  private selectedWidth: number = -1;

  constructor() {}

  ngAfterViewInit() {
    this.draw();
    const position = this.getPositionAtColor(this.color);
    if (position || position === 0) {
      setTimeout(() => {
        this.selectedWidth = position;
        this.draw();
      }, 17);
    }
  }

  draw() {
    if (!this.canvas) {
      return;
    }
    if (!this.context) {
      const context = this.canvas.nativeElement.getContext('2d');
      if (context) {
        this.context = context;
      } else {
        console.error('Cannot create context for canvas');
        return;
      }
    }
    const width = this.canvas.nativeElement.width;
    const height = this.canvas.nativeElement.height;
    if (this.selectedWidth < 0) {
      // this.selectedWidth = width - this.offset;
    }
    // Clear canvas to initial value
    this.context.clearRect(0, 0, width, height);
    // Create a linear gradient in our canvas
    const gradient = this.context.createLinearGradient(
      0,
      0,
      width - this.offset,
      0
    );
    // define color stops. There must be at least 2 stops
    const stops = ['transparent', this.hue];
    for (let i = 0; i < stops.length; ++i) {
      gradient.addColorStop(i / (stops.length - 1), stops[i]);
    }

    // Draw the gradient onto the canvas
    this.context.beginPath();
    this.context.rect(this.offset, 0, width - 2 * this.offset, height);
    this.context.fillStyle = gradient;
    this.context.fill();
    this.context.closePath();

    // Draw the knob onto the canvas
    if (this.strokeWidth || this.selectedWidth === 0) {
      this.context.beginPath();
      this.context.strokeStyle = 'black';
      this.context.lineWidth = this.strokeWidth;
      this.context.rect(this.selectedWidth - 5, 0, 10, height);
      this.context.stroke();
      this.context.closePath();
    }
  }

  @HostListener('window:mouseup', ['$event'])
  onMouseUp(evt: MouseEvent) {
    this.mouseDown = false;
  }

  onMouseDown(evt: MouseEvent) {
    this.mouseDown = true;
    this.selectedWidth = evt.offsetX;
    this.emitColor(evt.offsetX, evt.offsetY);
  }

  onMouseMove(evt: MouseEvent) {
    if (
      evt.offsetX < this.offset ||
      evt.offsetX > this.canvas.nativeElement.width - this.offset
    ) {
      return;
    }
    if (this.mouseDown) {
      evt.preventDefault();
      this.selectedWidth = evt.offsetX;
      this.draw();
      this.emitColor(evt.offsetX, evt.offsetY);
    }
  }

  emitColor(x: number, y: number) {
    const rgbaColor = this.getColorAtPosition(x, y);
    this.color = rgbaColor;
    this.colorChange.emit(rgbaColor);
  }

  getColorInformation(color: string) {
    const match = color.match(/rgba?\((.*)\)/);
    if (!match) {
      return { r: 255, g: 255, b: 255, a: 1 };
    }
    const rgba = match[1].split(',').map(Number);
    const r = rgba[0];
    const g = rgba[1];
    const b = rgba[2];
    const a = rgba[3];
    return { r, g, b, a };
  }

  getColorAtPosition(x: number, y: number) {
    if (!this.canvas) {
      return this.hue;
    }
    const h = this.getColorInformation(this.hue);
    return `rgba(${h.r}, ${h.g}, ${h.b}, ${Math.min(
      x / (this.canvas.nativeElement.width - 2 * this.offset),
      1
    )})`;
  }

  getPositionAtColor(color: string) {
    if (!this.canvas || !this.context || !this.canvas.nativeElement.height) {
      return undefined;
    }
    const height = this.canvas.nativeElement.height;
    const buffer = this.context.getImageData(0, this.offset, 1, height).data;
    const { r, g, b, a } = this.getColorInformation(color);
    for (let y = 0; y < height; ++y) {
      const p = y * 4;
      const distance = this.euclideanDistance(
        { r, g, b, a },
        { r: buffer[p], g: buffer[p + 1], b: buffer[p + 2], a: buffer[p + 3] }
      );
      if (distance < 5) {
        return y;
      }
    }
    // The color was not found
    return null;
  }

  setAlpha(alpha: number) {
    if (!this.canvas) {
      return;
    }
    const width = this.canvas.nativeElement.width;
    this.selectedWidth = (width - this.offset) * alpha;
    this.draw();
  }

  private euclideanDistance(
    p: { [key: string]: number },
    q: { [key: string]: number }
  ) {
    let sum = 0;
    for (const key of Object.keys(p)) {
      sum += Math.pow(p[key] - q[key], 2); // (p_i - q_i)^2
    }
    return Math.sqrt(sum);
  }
}
