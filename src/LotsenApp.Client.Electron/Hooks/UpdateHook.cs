// Copyright (c) 2021 OFFIS e.V.. All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//    
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//    
// 3. Neither the name of the copyright holder nor the names of its contributors
//    may be used to endorse or promote products derived from this software without
//    specific prior written permission.
//    
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.Extensions.Logging;

namespace LotsenApp.Client.Electron.Hooks
{
    public class UpdateHook : IElectronHook
    {
        private static bool UpdateReady { get; set; }
        public static string UpdateVersion { get; set; }
        private static ILogger<UpdateHook> _logger;

        public UpdateHook(ILogger<UpdateHook> logger)
        {
            _logger = logger;
        }

        public void Up()
        {
            // Check for updates
            ElectronNET.API.Electron.AutoUpdater.OnDownloadProgress += DownloadProgress;
            ElectronNET.API.Electron.AutoUpdater.OnError += Error;
            ElectronNET.API.Electron.AutoUpdater.OnUpdateAvailable += UpdateAvailable;
            ElectronNET.API.Electron.AutoUpdater.OnUpdateNotAvailable += UpdateNotAvailable;
            ElectronNET.API.Electron.AutoUpdater.OnUpdateDownloaded += UpdateDownloaded;

            ElectronNET.API.Electron.IpcMain.On("check-for-updates", CheckForUpdate);
            ElectronNET.API.Electron.IpcMain.On("download-update", DownloadUpdate);
            ElectronNET.API.Electron.IpcMain.On("update-restart", Restart);
            ElectronNET.API.Electron.IpcMain.On("update-cache", UpdateCache);

            ElectronNET.API.Electron.AutoUpdater.AutoDownload = false;
            ElectronNET.API.Electron.AutoUpdater.AutoInstallOnAppQuit = true;

            ElectronNET.API.Electron.App.BrowserWindowCreated += () =>
            {
                foreach (var window in ElectronNET.API.Electron.WindowManager.BrowserWindows)
                {
                    _logger.LogInformation($"Attaching cache clearing to {window.Id}");
                    window.OnClose -= ClearCache;
                    window.OnClose += ClearCache;
                }
            };
            
            ElectronNET.API.Electron.App.BeforeQuit += async _ =>
            {
                var assemblyVersion = await ElectronNET.API.Electron.AutoUpdater.CurrentVersionAsync;
                var currentVersion = $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Patch}";
                if (UpdateReady)
                {
                    _logger.LogInformation($"Clearing cache before update of {currentVersion} to {UpdateVersion}");
                    await ClearCacheAsync();
                }
            };
        }

        private void UpdateCache(object sender)
        {
            foreach (var window in ElectronNET.API.Electron.WindowManager.BrowserWindows)
            {
                ClearCacheForWindowAsync(window).ContinueWith(_ =>
                {
                    window.Reload();
                });
            }
        }

        private void DownloadProgress(ProgressInfo info)
        {
            _logger.LogInformation($"Downloading update: {info.Percent}% at {info.BytesPerSecond}B/s");
            foreach (var window in ElectronNET.API.Electron.WindowManager.BrowserWindows)
            {
                ElectronNET.API.Electron.IpcMain.Send(window, "download-progress", info);
            }
        }

        private void Error(string error)
        {
            _logger.LogError(error);
            foreach (var window in ElectronNET.API.Electron.WindowManager.BrowserWindows)
            {
                ElectronNET.API.Electron.IpcMain.Send(window, "update-error", error);
            }
        }

        private void UpdateAvailable(UpdateInfo info)
        {
            _logger.LogInformation($"An update is available: {info.Version}");
            foreach (var window in ElectronNET.API.Electron.WindowManager.BrowserWindows)
            {
                ElectronNET.API.Electron.IpcMain.Send(window, "update-available", info);
            }
        }

        private void UpdateNotAvailable(UpdateInfo info)
        {
            _logger.LogInformation("No Updates are available");
            foreach (var window in ElectronNET.API.Electron.WindowManager.BrowserWindows)
            {
                ElectronNET.API.Electron.IpcMain.Send(window, "update-not-available", info);
            }
        }

        private void UpdateDownloaded(UpdateInfo info)
        {
            _logger.LogInformation("Update downloaded");
            UpdateReady = true;
            foreach (var window in ElectronNET.API.Electron.WindowManager.BrowserWindows)
            {
                ElectronNET.API.Electron.IpcMain.Send(window, "update-downloaded", info);
            }
        }

        private void CheckForUpdate(object data)
        {
            _logger.LogInformation("Checking for updates.");
            // original password F8NwzrhIsk
            var header = new Dictionary<string, string> {{"password", "F8NwzrhIsk"}};
            ElectronNET.API.Electron.AutoUpdater.RequestHeaders = header;
            ElectronNET.API.Electron.AutoUpdater.CheckForUpdatesAsync()
                .ContinueWith(result =>
                {
                    if (result.Exception != null)
                    {
                        _logger.LogError($"There was an error checking for updates {result.Exception}");
                        foreach (var window in ElectronNET.API.Electron.WindowManager.BrowserWindows)
                        {
                            ElectronNET.API.Electron.IpcMain.Send(window, "update-error", "Unable to check for updates");
                        }

                    }
                    var updateMessage = result.Result;
                    _logger.LogInformation("Version available " + updateMessage.UpdateInfo.Version);
                    UpdateVersion = updateMessage.UpdateInfo.Version;
                });
        }

        private void DownloadUpdate(object data)
        {
            var header = new Dictionary<string, string> {{"password", "F8NwzrhIsk"}};
            ElectronNET.API.Electron.AutoUpdater.RequestHeaders = header;
            ElectronNET.API.Electron.AutoUpdater.DownloadUpdateAsync()
                .ContinueWith(result =>
                {
                    var updateMessage = result.Result;
                    _logger.LogCritical("Downloaded Version " + updateMessage);
                });
        }

        private void ClearCache()
        {
            Task.WaitAll(Task.Run(async () => await ClearCacheAsync()));
        }

        private async Task ClearCacheAsync(bool force = false)
        {
            if (!force && !UpdateReady)
            {
                _logger.LogInformation("The Cache will not be cleared.");
                return;
            }
            foreach (var window in ElectronNET.API.Electron.WindowManager.BrowserWindows)
            {
                await ClearCacheForWindowAsync(window, force);
            }
        }

        private async Task ClearCacheForWindowAsync(BrowserWindow window, bool force = true)
        {
            if (!force && !UpdateReady)
            {
                _logger.LogInformation("The Cache will not be cleared.");
                return;
            }
            _logger.LogInformation($"Clearing cache for ({window.Id})");
            await window.WebContents.Session.ClearCacheAsync();
        }

        private void Restart(object data)
        {
            UpdateReady = false;
            Task.Run(async () =>
            {
                _logger.LogInformation("Restarting the application to apply an update");
                await ClearCacheAsync(true);
                
                ElectronNET.API.Electron.AutoUpdater.QuitAndInstall();
            });
        }

        public void Down()
        {
            ElectronNET.API.Electron.AutoUpdater.OnDownloadProgress -= DownloadProgress;
            ElectronNET.API.Electron.AutoUpdater.OnError -= Error;
            ElectronNET.API.Electron.AutoUpdater.OnUpdateAvailable -= UpdateAvailable;
            ElectronNET.API.Electron.AutoUpdater.OnUpdateNotAvailable -= UpdateNotAvailable;
            ElectronNET.API.Electron.AutoUpdater.OnUpdateDownloaded -= UpdateDownloaded;
            ElectronNET.API.Electron.IpcMain.RemoveAllListeners("check-for-updates");
            ElectronNET.API.Electron.IpcMain.RemoveAllListeners("download-update");
            ElectronNET.API.Electron.IpcMain.RemoveAllListeners("update-restart");
            ElectronNET.API.Electron.IpcMain.RemoveAllListeners("update-cache");
        }
    }
}