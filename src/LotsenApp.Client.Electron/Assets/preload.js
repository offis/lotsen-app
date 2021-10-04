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

const { ipcRenderer, contextBridge, shell } = require('electron');

const validChannels = ['close-channel', 
    'minimize-channel', 
    'maximize-channel', 
    'window-maximized',
    'developer-tools',
    'force-reload',
    'dotnet-version',
    'check-for-updates',
    'download-update',
    'download-progress',
    'update-error',
    'update-available',
    'update-not-available',
    'update-downloaded',
    'update-restart',
    'update-cache',
    'open-save-file-dialog',
    'save-file-dialog-complete',
    'save-file-dialog-abort',
    'print-page'];
// All of the Node.js APIs are available in the preload process.
// It has the same sandbox as a Chrome extension.

// The context bridge is used to add server side and potential sensitive information into the client process without exposing internal api
contextBridge.exposeInMainWorld('electron', {
    'versions': process.versions,
    'platform': process.platform,
    'openExternal': (url) => shell.openExternal(url),
    'ipc': {
        send: (channel, data) => {
            if (validChannels.includes(channel)) {
                ipcRenderer.send(channel, data);
            } else {
                console.error(`The channel '${channel}' is not supported.`);
            }
        },
        on: (channel, callback) => {
            if (validChannels.includes(channel)) {
                // Filtering the event param from ipcRenderer
                const newCallback = (_, data) => callback(data);
                ipcRenderer.on(channel, newCallback);
            } else {
                console.error(`The channel '${channel}' is not supported.`);
            }
        },
        once: (channel, callback) => {
            if (validChannels.includes(channel)) {
                const newCallback = (_, data) => callback(data);
                ipcRenderer.once(channel, newCallback);
            } else {
                console.error(`The channel '${channel}' is not supported.`);
            }
        },
        removeListener: (channel, callback) => {
            if (validChannels.includes(channel)) {
                ipcRenderer.removeListener(channel, callback);
            } else {
                console.error(`The channel '${channel}' is not supported.`);
            }
        },
        removeAllListeners: (channel) => {
            if (validChannels.includes(channel)) {
                ipcRenderer.removeAllListeners(channel)
            } else {
                console.error(`The channel '${channel}' is not supported.`);
            }
        },
    },
    'debug': {
        'cpuUsage': process.getCPUUsage,
        'memoryUsage': process.getHeapStatistics,
        'uptime': process.uptime,
    }
});
