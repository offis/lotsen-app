using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace LotsenApp.Client.Electron.Hooks
{
    public class SaveFileDialogHook: IElectronHook
    {
        private readonly ILogger<SaveFileDialogHook> _logger;

        public SaveFileDialogHook(ILogger<SaveFileDialogHook> logger) 
        {
            _logger = logger;
        }

        public void Up()
        {
            ElectronNET.API.Electron.IpcMain.On("open-save-file-dialog", OpenSaveFileDialog);
        }

        private void OpenSaveFileDialog(object sender)
        {
            _logger.LogDebug("Opening save file dialog");
            var documentName = "document";
            string data = null;
            if (sender is JObject obj)
            {
                documentName = obj.ContainsKey("name") ? obj["name"]?.Value<string>() : documentName;
                data = obj.ContainsKey("data") ? obj["data"]?.Value<string>() : null;
            }
            var window = ElectronNET.API.Electron.WindowManager.BrowserWindows.First();
            ElectronNET.API.Electron.Dialog.ShowSaveDialogAsync(window, new SaveDialogOptions
            {
                DefaultPath = documentName,
                Filters = new[]
                {
                    new FileFilter
                    {
                        Name = "Text Files",
                        Extensions = new[] { "txt" }
                    }
                }
            }).ContinueWith(continuationTask =>
            {
                var result = continuationTask.Result;
                _logger.LogDebug($"User selected file {result}");
                if (data != null)
                {
                    System.IO.File.WriteAllText(result, data);
                }
                ElectronNET.API.Electron.IpcMain.Send(window, "save-file-dialog-complete", result);
            });
            

        }

        public void Down()
        {
            ElectronNET.API.Electron.IpcMain.RemoveAllListeners("open-save-file-dialog");
        }
    }
}