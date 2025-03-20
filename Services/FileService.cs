using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EPP.Services
{
    public class FileService : IFileService
    {
        private const string _backupFolder = "backups";
        private static string _backupPath = Path.Combine(Directory.GetCurrentDirectory(), _backupFolder);

        private readonly IClassicDesktopStyleApplicationLifetime _desktop;
        private Window _target { get => _desktop.MainWindow!; }

        public FileService(IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;
        }

        public async Task<IStorageFile?> OpenFileAsync(string filePath)
        {
            var file = await _target.StorageProvider.TryGetFileFromPathAsync(filePath);
            return file;
        }


        public async Task<IStorageFile?> OpenFilePickerAsync()
        {
            var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Open Text File",
                AllowMultiple = false
            });

            return files.Count >= 1 ? files[0] : null;
        }

        public async Task<IStorageFile?> SaveFileAsync(string filePath)
        {
            var file = await _target.StorageProvider.TryGetFileFromPathAsync(filePath);
            return file;
        }


        public async Task<IAsyncEnumerable<IStorageItem>?> ListFolderAsync(string folderPath)
        {
            var folder = await _target.StorageProvider.TryGetFolderFromPathAsync(folderPath);

            if (folder == null)
            {
                return null;
            }

            return folder.GetItemsAsync();
        }

        public void CreateFileBackup(string filePath)
        {

            if (Path.Exists(filePath))
            {
                // Ensure all directories exists
                var a = Directory.CreateDirectory(_backupPath);

                var time = DateTime.Now;
                string copyName = Path.GetFileNameWithoutExtension(filePath) + $"_{time:yyyy-MM-dd_HH-mm-ss}" + Path.GetExtension(filePath);
                File.Copy(filePath, Path.Combine(_backupPath, copyName));
            }
        }
    }
}
