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
  HostListener,
  Input,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { ParticipantService } from '../../participant.service';
import { ParticipantDocument } from '../../participant-document';
import { FlatTreeControl } from '@angular/cdk/tree';
import { TreeNode } from './tree-node';
import { DocumentDto } from '../../document-dto';
import { TranslateService } from '@ngx-translate/core';
import {
  MatTreeFlatDataSource,
  MatTreeFlattener,
} from '@angular/material/tree';
import { animate, style, transition, trigger } from '@angular/animations';
import { ElectronService } from '../../../core/electron.service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';
import { ParticipantDeleteConfirmComponent } from '../participant-delete-confirm/participant-delete-confirm.component';
import { IParticipant } from '../../iparticipant';
import {
  CdkDragDrop,
  moveItemInArray,
  transferArrayItem,
} from '@angular/cdk/drag-drop';
import { W } from '@angular/cdk/keycodes';
import { UserConfiguration } from '../../../core/user-configuration';

@Component({
  selector: 'la2-participant-editor',
  templateUrl: './participant-editor.component.html',
  styleUrls: ['./participant-editor.component.scss'],
  animations: [
    trigger('fadeInOut', [
      transition(':enter', [
        style({
          opacity: 0,
          maxWidth: 0,
        }),
        animate(
          '0.3s 0.1s cubic-bezier(0.4, 0.0, 1, 1)',
          style({
            opacity: 1,
            maxWidth: '*',
          })
        ),
      ]),
      transition(':leave', [
        style({
          opacity: 1,
          maxWidth: '*',
        }),
        animate(
          '0.3s 0.1s cubic-bezier(0.4, 0.0, 1, 1)',
          style({
            opacity: 0,
            maxWidth: 0,
          })
        ),
      ]),
    ]),
    trigger('toggleItems', [
      transition(':enter', [
        style({
          opacity: 0,
          padding: 0,
          margin: 0,
          maxHeight: 0,
        }),
        animate(
          '0.0s 0.5s cubic-bezier(.38,.08,.6,.91)',
          style({
            maxHeight: '*',
            opacity: 1,
            padding: '*',
            margin: '*',
          })
        ),
      ]),
      transition(':leave', [
        style({
          maxHeight: '*',
          opacity: 1,
          padding: '*',
          margin: '*',
        }),
        animate(
          '0.0s 0.5s cubic-bezier(.38,.08,.6,.91)',
          style({
            opacity: 0,
            padding: 0,
            margin: 0,
            maxHeight: 0,
          })
        ),
      ]),
    ]),
  ],
})
export class ParticipantEditorComponent implements OnInit, OnDestroy {
  @Input()
  participant!: IParticipant;
  @Input()
  userConfiguration!: UserConfiguration;

  documents: ParticipantDocument = {
    documents: [],
    participantId: '',
    onlineId: '',
    defaultDocument: '',
  };

  treeControl: FlatTreeControl<TreeNode> = new FlatTreeControl<TreeNode>(
    (node) => node.level,
    (node) => node.expandable
  );
  dataSource!: MatTreeFlatDataSource<DocumentDto, TreeNode>;

  documentOverviewVisible = true;

  private subscriptions: Subscription[] = [];

  constructor(
    private participantService: ParticipantService,
    private translate: TranslateService,
    public electronService: ElectronService,
    private router: Router,
    private dialog: MatDialog
  ) {}

  async ngOnInit(): Promise<void> {
    this.documents = await this.participantService.GetParticipantDocuments(
      this.participant.id
    );
    const treeTransformer = (node: DocumentDto, level: number): TreeNode => {
      return {
        expandable: (node.documents ?? []).length > 0,
        displayable: node,
        level,
      };
    };

    this.subscriptions.push(
      this.participantService.DocumentCreated.subscribe((next) => {
        if (next.participantId !== this.participant.id) {
          return;
        }
        if (!next.parentDocumentId) {
          this.documents.documents.push(next.displayable);
          this.dataSource.data = this.documents.documents ?? [];
        }

        if (next.parentDocumentId) {
          const foundDocument = this.documents.documents.find(
            (d) => d.id === next.parentDocumentId
          );
          if (!foundDocument) {
            return;
          }
          if (!foundDocument.documents) {
            foundDocument.documents = [];
          }
          foundDocument.documents.push(next.displayable);
          this.dataSource.data = this.documents.documents ?? [];
        }
      }),
      this.participantService.DocumentDeleted.subscribe(async (next) => {
        if (next.participantId !== this.participant.id || !this.documents) {
          return;
        }
        const path = this.findDocument(
          next.documentId,
          this.documents.documents
        );
        const result = this.removeDocument(path, this.documents.documents);
        this.documents.documents = result.newLevel;
        // Close command for subdocuments
        for (const child of result.children) {
          await this.router.navigate(['/www/participant/editor'], {
            queryParams: {
              closeDocument: child,
            },
          });
        }
        await this.router.navigate(['/www/participant/editor'], {
          queryParams: {
            closeDocument: next.documentId,
          },
        });
        this.dataSource.data = this.documents.documents;
      }),
      this.participantService.DocumentRenamed.subscribe((next) => {
        if (next.participantId !== this.participant.id) {
          return;
        }
        const document = this.findDocumentDto(
          next.documentId,
          this.documents.documents
        );
        if (!document) {
          return;
        }
        document.name = next.newName;
      })
    );

    const treeFlattener = new MatTreeFlattener(
      treeTransformer,
      (node) => node.level,
      (node) => node.expandable,
      (node) => node.documents
    );
    this.dataSource = new MatTreeFlatDataSource(
      this.treeControl,
      treeFlattener
    );

    this.dataSource.data = this.documents.documents;
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach((s) => s.unsubscribe());
  }

  @HostListener('window:keyup', ['$event'])
  handleKeyUp(event: KeyboardEvent): void {
    if (event.ctrlKey && event.key === 'p') {
      this.print();
    }
  }

  hasChild = (_: number, node: TreeNode) => node.expandable;

  toggleDocuments(state: boolean): void {
    this.documentOverviewVisible = state;
  }

  print(): void {
    if (!this.electronService.isElectronApp) {
      return;
    }

    this.electronService.ipcRenderer.send('print-page');
  }

  async openDocument(event: MouseEvent, node: TreeNode): Promise<void> {
    if (!event.isTrusted) {
      return;
    }
    await this.router.navigate(['/www/participant/editor'], {
      queryParams: {
        openDocument: node.displayable.id,
      },
    });
  }

  async deleteParticipant(): Promise<void> {
    const ref = this.dialog.open(ParticipantDeleteConfirmComponent, {
      data: {
        name: this.participant.header.name,
      },
    });
    const result = await ref.afterClosed().toPromise();
    if (result) {
      await this.participantService.DeleteParticipant(this.participant.id);
      await this.router.navigate(['/www/participant/editor'], {
        queryParams: {
          close: this.participant.id,
        },
      });
    }
  }

  async reorder(event: CdkDragDrop<TreeNode[]>): Promise<void> {
    if (
      event.currentIndex === event.previousIndex &&
      Math.abs(event.distance.x) <= 20
    ) {
      return;
    }
    const dragItem = event.item.data as TreeNode;
    const allNodes = event.previousContainer.data;
    // Save state of opened nodes
    const expandedNodes = allNodes.filter((n) =>
      this.treeControl.isExpanded(n)
    );
    // construct a list of visible nodes
    const visibleChildren = allNodes
      .map((n) =>
        this.treeControl.isExpanded(n)
          ? [n].concat(this.treeControl.getDescendants(n))
          : [n]
      )
      .filter((n) => n[0].level === 0 || this.treeControl.isExpanded(n[0]))
      .reduce((prev, cur) => prev.concat(cur), []);
    // find element in tree
    const path = this.findDocument(
      dragItem.displayable.id,
      this.documents.documents
    );
    path.pop();
    // Get parent array of element
    let parentArray: DocumentDto[] | undefined = this.resolveContainingArray(
      this.documents.documents,
      path
    );
    if (!parentArray) {
      console.error('Cannot find the array that contains the dragged item.');
      return;
    }
    // Get target array
    const predecessorNodeAtDestination =
      event.currentIndex - 1 > 0
        ? visibleChildren[event.currentIndex - 1]
        : undefined;
    const nodeAtDestination = visibleChildren[event.currentIndex];
    const successorNodeAtDestination =
      event.currentIndex + 1 < visibleChildren.length
        ? visibleChildren[event.currentIndex + 1]
        : visibleChildren[0];
    // The target is calculated by the distance to swap between levels of the array. A negative value (-20 or higher)
    // will result in the element going into the lower level (e.g. root), otherwise it will go into the higher level
    const destinationPath = this.findDocument(
      nodeAtDestination.displayable.id,
      this.documents.documents
    );
    destinationPath.pop();
    const destinationArray: DocumentDto[] | undefined =
      this.resolveContainingArray(this.documents.documents, destinationPath);
    if (!destinationArray) {
      console.error('Could not find the array of the destination');
      return;
    }
    // Normal case: we are done and found the target array
    let targetArray = destinationArray;
    let targetIndex = targetArray.indexOf(nodeAtDestination.displayable);

    // Special case: if our sibling node is the last element of its array and we have a movement up,
    // then we need to check whether or not to add it to the parent array
    if (
      predecessorNodeAtDestination &&
      nodeAtDestination.level < predecessorNodeAtDestination.level &&
      event.distance.x > -20 &&
      event.currentIndex < event.previousIndex
    ) {
      // console.log('Special case 1 occuring')
      const siblingPath = this.findDocument(
        predecessorNodeAtDestination.displayable.id,
        this.documents.documents
      );
      siblingPath.pop();

      const siblingArray: DocumentDto[] | undefined =
        this.resolveContainingArray(this.documents.documents, siblingPath);
      if (!siblingArray) {
        console.error(
          'Cannot find the predecessor array parent that the item was dragged to.'
        );
        return;
      }
      // we need to find the parent of our destination element
      targetArray = siblingArray;
      targetIndex =
        targetArray.indexOf(predecessorNodeAtDestination.displayable) + 1;
    }

    // Special case: if our movement is downwards and we transfer into another array, then we need to add 1 to the index
    if (
      parentArray != targetArray &&
      event.currentIndex > event.previousIndex
    ) {
      // console.log('Special case 4');
      targetIndex++;
    }

    // Special case: if our node is expandable and expanded and we have a movement down,
    // then we need to check whether or not to add it to the nodes array
    if (
      nodeAtDestination.expandable &&
      event.currentIndex > event.previousIndex &&
      this.treeControl.isExpanded(nodeAtDestination)
    ) {
      // console.log('Special case 3 occuring')

      if (!nodeAtDestination.displayable.documents) {
        nodeAtDestination.displayable.documents = [];
      }
      // we need to find the parent of our destination element
      targetArray = nodeAtDestination.displayable.documents;
      targetIndex = 0;
    }
    // Special case: the node itself is the last element of its array and
    // it is dragged to the left and dropped on itself
    if (
      dragItem == nodeAtDestination &&
      event.distance.x < -20 &&
      parentArray.indexOf(dragItem.displayable) === parentArray.length - 1
    ) {
      // console.log('Special case 2 occuring');
      const successorPath = this.findDocument(
        successorNodeAtDestination.displayable.id,
        this.documents.documents
      );
      successorPath.pop();

      const successorArray = this.resolveContainingArray(
        this.documents.documents,
        successorPath
      );
      if (!successorArray) {
        console.error(
          'Cannot find the successor array parent that the item was dragged to.'
        );
        return;
      }

      targetArray = successorArray;
      targetIndex = successorArray.indexOf(
        successorNodeAtDestination.displayable
      );
      // Special case: successor node is first node of the array
      if (successorNodeAtDestination == visibleChildren[0]) {
        targetIndex = this.documents.documents.length;
      }
    }

    // Special case: the node is dropped on itself and
    // it is dragged to the right while the predecessor is in another array
    if (
      predecessorNodeAtDestination &&
      dragItem == nodeAtDestination &&
      event.distance.x > 20
    ) {
      // console.log('Special case 5 occuring');
      const predecessorPath = this.findDocument(
        predecessorNodeAtDestination.displayable.id,
        this.documents.documents
      );
      predecessorPath.pop();

      const predecessorArray = this.resolveContainingArray(
        this.documents.documents,
        predecessorPath
      );
      if (!predecessorArray) {
        console.error(
          'Cannot find the successor array parent that the item was dragged to.'
        );
        return;
      }

      if (predecessorArray != parentArray) {
        targetArray = predecessorArray;
        targetIndex = predecessorArray.length;
      }
    }
    // Calculate target insert point
    const originIndex = parentArray.indexOf(dragItem.displayable);
    const insertIndex = targetIndex;
    // console.log('inserting from ', parentArray, ' at ', originIndex , ' into ', targetArray, ' at ', insertIndex);
    // Insert or move to new position
    if (parentArray == targetArray) {
      moveItemInArray(targetArray, originIndex, insertIndex);
    } else {
      transferArrayItem(parentArray, targetArray, originIndex, insertIndex);
    }
    this.dataSource.data = this.documents.documents;
    // Restore opened state
    for (let node of expandedNodes) {
      const treeNode = this.treeControl.dataNodes.find(
        (n) => n.displayable.id === node.displayable.id
      );
      if (!treeNode) {
        continue;
      }
      this.treeControl.expand(treeNode);
    }

    // sync with server
    await this.participantService.ReOrderDocuments(
      this.participant.id,
      this.documents.documents
    );
  }

  async commitChanges(): Promise<void> {
    await this.participantService.CommitChanges(this.participant.id);
    this.documents = await this.participantService.GetParticipantDocuments(
      this.participant.id
    );
    this.dataSource.data = this.documents.documents;
  }

  async editMetadata() {
    await this.router.navigate([`/www/participant/editor/${this.participant.id}/metadata`]);
  }

  private removeDocument(
    path: string[],
    level: DocumentDto[]
  ): { children: string[]; newLevel: DocumentDto[] } {
    const currentId = path[0];
    if (path.length === 1) {
      const document = level.find((d) => d.id === currentId);
      const children = this.getAllChildren(document);
      const newLevel = level.filter((d) => d.id !== currentId);
      return { children, newLevel };
    }

    const currentDocument = level.find((d) => d.id === currentId);
    if (!currentDocument) {
      throw new Error('The document could not be removed');
    }
    const result = this.removeDocument(
      path.filter((v, i) => i > 0),
      currentDocument.documents ?? []
    );
    currentDocument.documents = result.newLevel;
    return {
      children: result.children,
      newLevel: level,
    };
  }

  private getAllChildren(document: DocumentDto | undefined): string[] {
    const children =
      document?.documents
        ?.map((d) => this.getAllChildren(d))
        .reduce((prev, cur) => prev.concat(cur), []) ?? [];
    return document?.documents?.map((d) => d.id).concat(children) ?? [];
  }

  private findDocument(documentId: string, level: DocumentDto[]): string[] {
    const document = level.find((d) => d.id === documentId);
    if (document) {
      return [documentId];
    }
    for (const documentDto of level) {
      const result = this.findDocument(documentId, documentDto.documents ?? []);
      if (result.length > 0) {
        result.unshift(documentDto.id);
        return result;
      }
    }
    return [];
  }

  private findDocumentDto(
    documentId: string,
    documents: DocumentDto[]
  ): DocumentDto | undefined {
    const path = this.findDocument(documentId, documents);

    if (path.length === 0) {
      return undefined;
    }
    const firstLevel = path.shift();
    let returnValue = documents.find((d) => d.id === firstLevel);
    while (path.length > 0) {
      const currentLevel = path.shift();
      returnValue = returnValue?.documents?.find((d) => d.id === currentLevel);
    }
    return returnValue;
  }

  private resolveContainingArray(
    array: DocumentDto[] | undefined,
    path: string[]
  ): DocumentDto[] | undefined {
    while (path.length > 0) {
      const currentElement = path.shift();
      array = array?.find((p) => p.id === currentElement)?.documents;
    }
    return array;
  }
}
