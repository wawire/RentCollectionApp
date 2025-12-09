using Microsoft.AspNetCore.Http;

namespace RentCollection.Application.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderPath);
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<byte[]> DownloadFileAsync(string fileUrl);
        string GetFileUrl(string filePath);
    }
}
