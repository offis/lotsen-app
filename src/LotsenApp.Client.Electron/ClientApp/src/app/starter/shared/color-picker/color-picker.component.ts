import { Component, Input, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'la2-color-picker',
  templateUrl: './color-picker.component.html',
  styleUrls: ['./color-picker.component.scss'],
})
export class ColorPickerComponent {
  private internalColor: string = '#ff0000ff';

  hue: string = this.calculateRgbaColor(this.internalColor);
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
  @Input()
  set color(value: string) {
    this.internalColor = value;
    // setTimeout(() => (this.hue = this.calculateRgbaColor(value)), 17);
    this.colorChange.emit(value);
  }
  @Output()
  colorChange = new EventEmitter<string>();

  strokeWidth = 2;
  offset = 5;
  constructor() {}

  calculateHexColor(rgba: string) {
    const match = rgba.match(/rgba?\((.*)\)/);
    if (!match) {
      return;
    }
    const color = match[1].split(',').map(Number);
    const r = color[0].toString(16).padStart(2, '0');
    const g = color[1].toString(16).padStart(2, '0');
    const b = color[2].toString(16).padStart(2, '0');
    const a = Math.trunc(color[3] * 255)
      .toString(16)
      .padStart(2, '0');
    this.color = `#${r}${g}${b}${a}`;
  }

  calculateRgbaColor(color: string) {
    const r = color.substring(1, 3);
    const g = color.substring(3, 5);
    const b = color.substring(5, 7);
    const a = color.substring(7, 9);
    console.log(r, g, b, a);
    const decimalR = parseInt(r, 16);
    const decimalG = parseInt(g, 16);
    const decimalB = parseInt(b, 16);
    const decimalA = parseInt(a, 16);
    console.log(decimalR, decimalG, decimalB, decimalA);
    return `rgba(${decimalR}, ${decimalG}, ${decimalB}, ${decimalA / 255})`;
  }
}
