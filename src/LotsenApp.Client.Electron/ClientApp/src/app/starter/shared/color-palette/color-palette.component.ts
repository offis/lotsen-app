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
  OnChanges,
  Output,
  SimpleChanges,
  ViewChild,
} from '@angular/core';

@Component({
  selector: 'la2-color-palette',
  templateUrl: './color-palette.component.html',
  styleUrls: ['./color-palette.component.scss'],
})
export class ColorPaletteComponent implements AfterViewInit, OnChanges {
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
    if (!this.canvas) {
      this.color = value;
    }
    this.color = this.getColorAtPosition(
      this.selectedPosition.x,
      this.selectedPosition.y
    );
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

  private mousedown: boolean = false;

  public selectedPosition: { x: number; y: number } = { x: 239, y: 11 };

  ngAfterViewInit() {
    this.draw();
    // this.setKnobPosition();
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
    // Reset canvas
    this.context.clearRect(0, 0, width, height);

    this.context.fillStyle = this.hue || 'rgba(255,255,255,1)';
    this.context.fillRect(
      this.offset,
      this.offset,
      width - 2 * this.offset,
      height - 2 * this.offset
    );

    const whiteGradient = this.context.createLinearGradient(0, 0, width, 0);
    whiteGradient.addColorStop(0, 'rgba(255,255,255,1)');
    whiteGradient.addColorStop(1, 'rgba(255,255,255,0)');

    this.context.fillStyle = whiteGradient;
    this.context.fillRect(
      this.offset,
      this.offset,
      width - 2 * this.offset,
      height - 2 * this.offset
    );

    const blackGradient = this.context.createLinearGradient(0, 0, 0, height);
    blackGradient.addColorStop(0, 'rgba(0,0,0,0)');
    blackGradient.addColorStop(1, 'rgba(0,0,0,1)');

    this.context.fillStyle = blackGradient;
    this.context.fillRect(
      this.offset,
      this.offset,
      width - 2 * this.offset,
      height - 2 * this.offset
    );

    if (this.selectedPosition) {
      this.context.strokeStyle = 'black';
      this.context.fillStyle = 'black';
      this.context.beginPath();
      this.context.arc(
        this.selectedPosition.x,
        this.selectedPosition.y,
        10,
        0,
        2 * Math.PI
      );
      this.context.lineWidth = this.strokeWidth;
      this.context.stroke();
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (!this.canvas) {
      return;
    }
    if (changes['hue']) {
      this.draw();
      const pos = this.selectedPosition;
      if (pos) {
        setTimeout(
          () => (this.color = this.getColorAtPosition(pos.x, pos.y)),
          17
        );
      }
    }
  }

  @HostListener('window:mouseup', ['$event'])
  onMouseUp(evt: MouseEvent) {
    this.mousedown = false;
  }

  onMouseDown(evt: MouseEvent) {
    this.mousedown = true;
    if (this.isOutBounds(evt.offsetX, evt.offsetY)) {
      return;
    }
    this.selectedPosition = { x: evt.offsetX, y: evt.offsetY };
    this.draw();
    this.color = this.getColorAtPosition(evt.offsetX, evt.offsetY);
  }

  onMouseMove(evt: MouseEvent) {
    if (this.isOutBounds(evt.offsetX, evt.offsetY)) {
      return;
    }

    if (this.mousedown) {
      evt.preventDefault();
      this.selectedPosition = { x: evt.offsetX, y: evt.offsetY };
      this.draw();
      this.emitColor(evt.offsetX, evt.offsetY);
    }
  }

  isOutBounds(x: number, y: number) {
    const width = this.canvas.nativeElement.width;
    const height = this.canvas.nativeElement.height;
    return (
      x < this.offset ||
      y < this.offset ||
      x > width - this.offset - 1 ||
      y > height - this.offset
    );
  }

  emitColor(x: number, y: number) {
    this.color = this.getColorAtPosition(x, y);
  }

  setKnobPosition() {
    const position = this.getPositionAtColor(this.color);
    if (position) {
      setTimeout(() => {
        this.selectedPosition = position;
        this.draw();
      }, 17);
    }
  }

  getColorAtPosition(x: number, y: number) {
    if (!this.canvas || !this.context) {
      return this.color;
    }
    const width = this.canvas.nativeElement.width;
    const height = this.canvas.nativeElement.height;
    if (
      x < this.offset ||
      y < this.offset ||
      x > width - this.offset ||
      y > height - this.offset
    ) {
      return this.color;
    }
    const imageData = this.context.getImageData(x, y, 1, 1).data;
    return (
      'rgba(' + imageData[0] + ',' + imageData[1] + ',' + imageData[2] + ',1)'
    );
  }

  getPositionAtColor(color: string) {
    if (!this.canvas) {
      return this.selectedPosition;
    }
    const height = this.canvas.nativeElement.height;
    const width = this.canvas.nativeElement.width;
    const buffer = this.context.getImageData(0, 0, width, height).data;
    const match = color.match(/rgba?\((.*)\)/);
    if (!match) {
      return null;
    }
    const rgba = match[1].split(',').map(Number);
    const r = rgba[0];
    const g = rgba[1];
    const b = rgba[2];
    const distances: {
      position: { x: number; y: number };
      distance: number;
    }[] = [];
    for (let y = 0; y < height; ++y) {
      const p = y * 4 * width;
      for (let x = 0; x < width; ++x) {
        const px = p + x * 4;
        const distance = this.euclideanDistance(
          { r, g, b },
          { r: buffer[px], g: buffer[px + 1], b: buffer[px + 2] }
        );
        distances.push({ position: { x, y }, distance: distance });
        if (distance < 5) {
          return { x, y };
        }
      }
    }
    var minDistance = Math.min(...distances.map((d) => d.distance));
    // The color was not found via threshold
    return distances.find((d) => d.distance == minDistance)?.position;
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
