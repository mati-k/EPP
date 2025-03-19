using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPP.Services
{
    public interface IFilesService
    {
        public Task<IStorageFile?> OpenFileAsync();
        public Task<IStorageFile?> SaveFileAsync();
        public Task<IAsyncEnumerable<IStorageItem>?> ListFolderAsync(string folderPath);
    }
}
