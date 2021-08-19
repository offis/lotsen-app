import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { DialogData } from './dialog-data';
import { ColorPickerComponent } from '../color-picker/color-picker.component';
import { MatTabChangeEvent } from '@angular/material/tabs';

@Component({
  selector: 'la2-color-picker-dialog',
  templateUrl: './color-picker-dialog.component.html',
  styleUrls: ['./color-picker-dialog.component.scss'],
})
export class ColorPickerDialogComponent implements OnInit {
  predefinedColors: string[] = [];
  initialColor: string = '';

  resultColor?: string;

  @ViewChild('picker')
  picker!: ColorPickerComponent;

  constructor(
    public dialogRef: MatDialogRef<ColorPickerDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData
  ) {}

  ngOnInit(): void {
    this.initialColor = this.data.initialColor;
    this.predefinedColors = this.data.predefinedColors;
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
