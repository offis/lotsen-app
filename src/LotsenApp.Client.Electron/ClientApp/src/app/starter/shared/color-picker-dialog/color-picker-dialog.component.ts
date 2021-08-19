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
