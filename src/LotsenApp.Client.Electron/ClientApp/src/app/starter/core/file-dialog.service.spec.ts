import { TestBed } from '@angular/core/testing';

import { FileDialogService } from './file-dialog.service';

describe('FileDialogService', () => {
  let service: FileDialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FileDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
