using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.Infrastructure.Services;

/// <summary>
/// Local file storage service for development and small deployments
/// Stores files in wwwroot/uploads directory
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadPath;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly long _defaultMaxFileSize = 10 * 1024 * 1024; // 10MB default
    private readonly string[] _defaultAllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
        _uploadPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads");

        // Create uploads directory if it doesn't exist
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
            _logger.LogInformation("Created uploads directory at: {UploadPath}", _uploadPath);
        }
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder)
    {
        try
        {
            // Validate first with defaults
            var (isValid, errorMessage) = await ValidateFileAsync(file, _defaultAllowedExtensions, _defaultMaxFileSize);
            if (!isValid)
            {
                throw new InvalidOperationException(errorMessage);
            }

            // Create folder if needed
            var folderPath = Path.Combine(_uploadPath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Generate unique filename
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative URL
            var relativeUrl = $"/uploads/{folder}/{fileName}";
            _logger.LogInformation("File uploaded successfully: {FileName} -> {RelativeUrl}", file.FileName, relativeUrl);

            return relativeUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
            throw;
        }
    }

    public Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            // Convert URL to file path
            if (string.IsNullOrEmpty(fileUrl))
            {
                return Task.FromResult(false);
            }

            // Handle both relative URLs (/uploads/...) and full paths
            var relativePath = fileUrl.TrimStart('/');
            var filePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, relativePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted: {FilePath}", filePath);
                return Task.FromResult(true);
            }

            _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileUrl}", fileUrl);
            return Task.FromResult(false);
        }
    }

    public Task<(bool isValid, string errorMessage)> ValidateFileAsync(
        IFormFile file,
        string[] allowedExtensions,
        long maxSizeInBytes)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return Task.FromResult((false, "No file provided"));
            }

            // Check file size
            if (file.Length > maxSizeInBytes)
            {
                var maxSizeMB = maxSizeInBytes / 1024 / 1024;
                return Task.FromResult((false, $"File size exceeds maximum allowed size of {maxSizeMB}MB"));
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
            {
                return Task.FromResult((false, "File has no extension"));
            }

            if (!allowedExtensions.Contains(extension))
            {
                return Task.FromResult((false, $"File type not allowed. Allowed types: {string.Join(", ", allowedExtensions)}"));
            }

            // Check MIME type (basic validation)
            var allowedMimeTypes = new Dictionary<string, string[]>
            {
                { ".jpg", new[] { "image/jpeg", "image/jpg" } },
                { ".jpeg", new[] { "image/jpeg", "image/jpg" } },
                { ".png", new[] { "image/png" } },
                { ".webp", new[] { "image/webp" } },
                { ".pdf", new[] { "application/pdf" } },
                { ".doc", new[] { "application/msword" } },
                { ".docx", new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" } }
            };

            if (allowedMimeTypes.ContainsKey(extension))
            {
                var validMimeTypes = allowedMimeTypes[extension];
                if (!validMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                {
                    return Task.FromResult((false, $"Invalid file content type: {file.ContentType}. Expected {string.Join(" or ", validMimeTypes)}"));
                }
            }

            return Task.FromResult((true, string.Empty));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file: {FileName}", file?.FileName);
            return Task.FromResult((false, $"File validation error: {ex.Message}"));
        }
    }

    public string GetFileUrl(string filePath)
    {
        // If it's already a URL (starts with /), return as-is
        if (filePath.StartsWith("/"))
        {
            return filePath;
        }

        // Otherwise, assume it's a relative path and prepend /uploads/
        return $"/uploads/{filePath.TrimStart('/')}";
    }

    public async Task<byte[]> DownloadFileAsync(string fileUrl)
    {
        try
        {
            // Convert URL to file path
            var relativePath = fileUrl.TrimStart('/');
            var filePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, relativePath);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {fileUrl}");
            }

            return await File.ReadAllBytesAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {FileUrl}", fileUrl);
            throw;
        }
    }
}
