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
  Component,
  ComponentFactoryResolver,
  Input,
  ViewContainerRef,
} from '@angular/core';
import { Components } from '../../core/components.enum';
import { ReminderListComponent } from '../reminder-list/reminder-list.component';
import { ParticipantListComponent } from '../participant-list/participant-list.component';
import { EmptyGridItemComponent } from '../empty-grid-item/empty-grid-item.component';
import { ProgramGridTileComponent } from '../program-grid-tile/program-grid-tile.component';

@Component({
  selector: 'la2-grid-item',
  templateUrl: './grid-item.component.html',
  styleUrls: ['./grid-item.component.scss'],
})
export class GridItemComponent {
  @Input()
  set tile(type: Components | undefined) {
    this.showComponent(type);
  }

  constructor(
    private viewContainer: ViewContainerRef,
    private componentFactoryResolver: ComponentFactoryResolver
  ) {}

  showComponent(type: Components | undefined): void {
    let factory;
    this.viewContainer.clear();
    switch (type) {
      case Components.Reminder:
        factory = this.componentFactoryResolver.resolveComponentFactory(
          ReminderListComponent
        );
        break;
      case Components.ParticipantList:
        factory = this.componentFactoryResolver.resolveComponentFactory(
          ParticipantListComponent
        );
        break;
      case Components.Programmes:
        factory = this.componentFactoryResolver.resolveComponentFactory(
          ProgramGridTileComponent
        );
        break;
      default:
        factory = this.componentFactoryResolver.resolveComponentFactory(
          EmptyGridItemComponent
        );
        break;
    }
    this.viewContainer.createComponent(factory);
  }
}
