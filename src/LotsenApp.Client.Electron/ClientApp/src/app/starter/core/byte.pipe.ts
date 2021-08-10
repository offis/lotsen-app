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

import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'byte',
})
export class BytePipe implements PipeTransform {
  transform(value: unknown, ...args: unknown[]): unknown {
    let bytes = value as number;
    if (!bytes) {
      return '0 B';
    }
    let iterations = 0;
    while (bytes > 1024) {
      iterations++;
      bytes /= 1024;
    }
    return this.formatValue(bytes) + this.getUnit(iterations);
  }

  private formatValue(value: number): number {
    return Math.trunc(value * 100) / 100;
  }

  private getUnit(iterations: number): string {
    switch (iterations) {
      case 0:
        return ' B';
      case 1:
        return ' kB';
      case 2:
        return ' MB';
      case 3:
        return ' GB';
      case 4:
        return ' TB';
      case 5:
        return ' PB';
      case 6:
        return ' EB';
      case 7:
        return ' ZB';
      case 8:
        return ' YB';
    }
    return '';
  }
}
