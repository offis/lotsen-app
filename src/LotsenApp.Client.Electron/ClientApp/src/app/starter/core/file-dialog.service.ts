import { Injectable } from '@angular/core';
import { ElectronService } from './electron.service';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class FileDialogService {
  constructor(private electronService: ElectronService) {}

  async OpenSaveFileDialog(name: string, data: string) {
    if (this.electronService.isElectronApp) {
      return this.OpenSaveFileDialogViaElectron(name, data);
    }
    return this.OpenSaveFileDialogViaBrowser(name, data);
  }

  private OpenSaveFileDialogViaElectron(
    name: string,
    data: string
  ): Promise<string> {
    return new Promise((resolve, reject) => {
      this.electronService.ipcRenderer.once(
        'save-file-dialog-complete',
        (result) => {
          this.electronService.ipcRenderer.removeAllListeners(
            'save-file-dialog-abort'
          );
          resolve(result);
        }
      );
      this.electronService.ipcRenderer.once(
        'save-file-dialog-abort',
        (result) => {
          this.electronService.ipcRenderer.removeAllListeners(
            'save-file-dialog-complete'
          );
          reject(new Error('The user cancelled the file dialog'));
        }
      );
      this.electronService.ipcRenderer.send('open-save-file-dialog', {
        name: name,
        data: data,
      });
    });
  }

  private async OpenSaveFileDialogViaBrowser(name: string, data: string) {
    const options = {
      suggestedName: 'tan-list.txt',
      startIn: 'documents',
      types: [
        {
          description: 'Text Files',
          accept: {
            'text/plain': ['.txt'],
          },
        },
      ],
    };
    console.log('Show Save file dialog');
    const handle = await (window as any).showSaveFilePicker(options);
    console.log('Checking permissions', handle, await handle.getFile());
    if ((await handle.queryPermission({ mode: 'readwrite' })) !== 'granted') {
      console.log('No write permissions have been granted');
      console.log('Requesting permission');
      if (
        (await handle.requestPermission({ mode: 'readwrite' })) !== 'granted'
      ) {
        console.log(
          'Write permissions have not been granted. Cannot save tan list'
        );
        return;
      }
    }

    console.log('Requesting new tan');
    const list = await this.http
      .get<string[]>('/api/authorization/tan/list')
      .pipe(map((input) => input.join('\n')))
      .toPromise();
    console.log('Creating writeable handle');
    const writeable = await handle.createWritable();
    console.log('Writing content');
    await writeable.write(list);
    console.log('closing file');
    await writeable.close();
  }
}
