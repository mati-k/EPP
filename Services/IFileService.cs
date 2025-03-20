using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPP.Services
{
    public interface IFileService
    {
        public Task<IStorageFile?> OpenFileAsync(string filePath);
        public Task<IAsyncEnumerable<IStorageItem>?> ListFolderAsync(string folderPath);
        public void CreateFileBackup(string filePath);
    }
}
