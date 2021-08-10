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
  AfterViewChecked,
  Component,
  DoCheck,
  ElementRef,
  Input,
  ViewChild,
} from '@angular/core';
import { IParticipant } from '../iparticipant';
import { Params } from '@angular/router';

@Component({
  selector: 'la2-single-project-overview',
  templateUrl: './single-project-overview.component.html',
  styleUrls: ['./single-project-overview.component.scss'],
})
export class SingleProjectOverviewComponent
  implements DoCheck, AfterViewChecked
{
  @Input()
  participants: IParticipant[] = [];

  @Input()
  calculateWidth = true;

  @Input()
  set forceListView(value: boolean | undefined) {
    if (value === undefined) {
      this.lockView = false;
      return;
    }
    this.lockView = true;
    this.forceList = value;
  }

  @Input()
  dateFormat = 'short';

  @ViewChild('singleProjectContainer')
  container!: ElementRef;

  private oldCols = 4;
  private newLength = 0;

  cols = 4;
  viewList = false;
  lockView = false;
  forceList = false;

  constructor() {}

  ngDoCheck(): void {
    this.newLength = this.container?.nativeElement?.offsetWidth ?? 0;
  }

  ngAfterViewChecked(): void {
    if (this.forceList || !this.calculateWidth) {
      return;
    }
    const newCols = Math.trunc(this.newLength / 200);
    if (newCols !== this.oldCols) {
      this.oldCols = newCols;
      setTimeout(() => (this.cols = newCols), 1);
    }
  }

  getTint(color: string): string {
    if (color.length === 7) {
      return `${color}22`;
    }
    if (color.length === 9) {
      return `${color.substr(0, 7)}22`;
    }
    return color;
  }

  getParams(participant: IParticipant): Params {
    return {
      open: participant.id,
    };
  }
}
