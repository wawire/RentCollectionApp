using Microsoft.AspNetCore.Http;

namespace RentCollection.Application.Services.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Upload a file and return the public URL
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="folder">Folder/container name (e.g., "properties", "payments", "documents")</param>
    /// <returns>Public URL of the uploaded file</returns>
    Task<string> UploadFileAsync(IFormFile file, string folder);

    /// <summary>
    /// Delete a file by URL
    /// </summary>
    /// <param name="fileUrl">URL or path of the file to delete</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteFileAsync(string fileUrl);

    /// <summary>
    /// Validate file before upload
    /// </summary>
    /// <param name="file">File to validate</param>
    /// <param name="allowedExtensions">Optional: allowed file extensions (e.g., [".jpg", ".png"])</param>
    /// <param name="maxSizeInBytes">Optional: max file size in bytes</param>
    /// <returns>Tuple with validation result and error message if invalid</returns>
    Task<(bool IsValid, string ErrorMessage)> ValidateFileAsync(
        IFormFile file,
        string[]? allowedExtensions = null,
        long? maxSizeInBytes = null);
}
