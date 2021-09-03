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
  selector: 'la2-color-slider',
  templateUrl: './color-slider.component.html',
  styleUrls: ['./color-slider.component.scss'],
})
export class ColorSliderComponent implements AfterViewInit {
  @Input()
  strokeWidth = 1;
  @Input()
  offset = 5;
  @Input()
  hue: string = 'rgba(255, 0, 0, 1)';
  @Output()
  hueChange = new EventEmitter<string>();
  @ViewChild('canvas')
  canvas!: ElementRef<HTMLCanvasElement>;

  private context!: CanvasRenderingContext2D;
  private mouseDown = false;
  private selectedHeight: number = 0;

  constructor() {}

  ngAfterViewInit() {
    this.draw();
    const position = this.getPositionAtColor(this.hue);
    if (position || position === 0) {
      setTimeout(() => {
        this.selectedHeight = position;
        this.draw();
      }, 17);
    }
  }

  draw() {
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
    // Clear canvas to initial value
    this.context.clearRect(0, 0, width, height);
    // Create a linear gradient in our canvas
    const gradient = this.context.createLinearGradient(0, 0, 0, height);
    // define color stops. There must be at least 2 stops
    const stops = [
      'rgba(255, 0, 0, 1)', // red
      'rgba(255, 255, 0, 1)', // yellow
      'rgba(0, 255, 0, 1)', // green
      'rgba(0, 255, 255, 1)', // turquoise
      'rgba(0, 0, 255, 1)', // blue
      'rgba(255, 0, 255, 1)', // pink
      'rgba(255, 0, 0, 1)', // red
    ];
    for (let i = 0; i < stops.length; ++i) {
      gradient.addColorStop(i / (stops.length - 1), stops[i]);
    }

    // Draw the gradient onto the canvas
    this.context.beginPath();
    this.context.rect(0, 2 * this.offset, width, height - 4 * this.offset);
    this.context.fillStyle = gradient;
    this.context.fill();
    this.context.closePath();

    // Draw the knob onto the canvas
    if (this.selectedHeight || this.selectedHeight === 0) {
      this.context.beginPath();
      this.context.strokeStyle = 'black';
      this.context.lineWidth = this.strokeWidth;
      this.context.rect(0, this.selectedHeight - 5, width, 10);
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
    if (this.isOutBounds(evt.offsetX, evt.offsetY)) {
      return;
    }
    this.selectedHeight = evt.offsetY;
    this.emitColor(evt.offsetX, evt.offsetY);
  }

  onMouseMove(evt: MouseEvent) {
    if (this.isOutBounds(evt.offsetX, evt.offsetY)) {
      return;
    }
    if (this.mouseDown) {
      evt.preventDefault();
      this.selectedHeight = evt.offsetY;
      this.draw();
      this.emitColor(evt.offsetX, evt.offsetY);
    }
  }

  isOutBounds(x: number, y: number) {
    const height = this.canvas.nativeElement.height;
    return y < 2 * this.offset || y > height - 2 * this.offset - 1;
  }

  emitColor(x: number, y: number) {
    const rgbaColor = this.getColorAtPosition(x, y);
    this.hue = rgbaColor;
    this.hueChange.emit(rgbaColor);
  }

  getColorAtPosition(x: number, y: number) {
    const imageData = this.context.getImageData(x, y, 1, 1).data;
    return `rgba(${imageData[0]}, ${imageData[1]}, ${imageData[2]}, 1)`;
  }

  getPositionAtColor(color: string) {
    if (!this.canvas || !this.context || !this.canvas.nativeElement.height) {
      return undefined;
    }
    const height = this.canvas.nativeElement.height;
    const buffer = this.context.getImageData(0, 0, 1, height).data;
    const match = color.match(/rgba?\((.*)\)/);
    if (!match) {
      return null;
    }
    const rgba = match[1].split(',').map(Number);
    const r = rgba[0];
    const g = rgba[1];
    const b = rgba[2];
    for (let y = 0; y < height; ++y) {
      const p = y * 4;
      const distance = this.euclideanDistance(
        { r, g, b },
        { r: buffer[p], g: buffer[p + 1], b: buffer[p + 2] }
      );
      if (distance < 5) {
        return y;
      }
    }
    // The color was not found
    return null;
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
