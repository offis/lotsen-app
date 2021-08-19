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
