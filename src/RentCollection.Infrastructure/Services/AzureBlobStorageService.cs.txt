using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.Infrastructure.Services;

/// <summary>
/// Azure Blob Storage service for production deployments
/// Stores files in Azure Blob Storage with configurable container
/// Requires Azure.Storage.Blobs NuGet package
/// </summary>
public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly ILogger<AzureBlobStorageService> _logger;
    private readonly long _defaultMaxFileSize = 10 * 1024 * 1024; // 10MB default
    private readonly string[] _defaultAllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };

    public AzureBlobStorageService(
        IConfiguration configuration,
        ILogger<AzureBlobStorageService> logger)
    {
        _logger = logger;

        var connectionString = configuration["AzureStorage:ConnectionString"]
            ?? throw new InvalidOperationException("Azure Storage connection string not configured");

        _containerName = configuration["AzureStorage:ContainerName"] ?? "rent-collection-files";
        _blobServiceClient = new BlobServiceClient(connectionString);

        _logger.LogInformation("Azure Blob Storage service initialized with container: {ContainerName}", _containerName);
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

            // Get or create container
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Generate blob name with folder structure
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var blobName = $"{folder}/{Guid.NewGuid()}{extension}";
            var blobClient = containerClient.GetBlobClient(blobName);

            // Set content type based on extension
            var contentType = file.ContentType;
            var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };

            // Upload with progress logging
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, blobHttpHeaders);
            }

            // Return public URL
            var publicUrl = blobClient.Uri.ToString();
            _logger.LogInformation("File uploaded to Azure Blob Storage: {FileName} -> {PublicUrl}",
                file.FileName, publicUrl);

            return publicUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Azure Blob Storage: {FileName}", file.FileName);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                return false;
            }

            // Extract blob name from URL
            var uri = new Uri(fileUrl);
            var segments = uri.Segments;

            // The blob name is everything after the container name
            // Format: https://account.blob.core.windows.net/container/folder/filename.ext
            var blobName = string.Join("", segments.Skip(2)); // Skip "/" and "container/"

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var deleted = await blobClient.DeleteIfExistsAsync();

            if (deleted)
            {
                _logger.LogInformation("File deleted from Azure Blob Storage: {FileUrl}", fileUrl);
            }
            else
            {
                _logger.LogWarning("File not found in Azure Blob Storage: {FileUrl}", fileUrl);
            }

            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from Azure Blob Storage: {FileUrl}", fileUrl);
            return false;
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
        // For Azure Blob Storage, if it's already a full URL, return as-is
        if (filePath.StartsWith("http://") || filePath.StartsWith("https://"))
        {
            return filePath;
        }

        // Otherwise, construct the full Azure Blob URL
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(filePath.TrimStart('/'));
        return blobClient.Uri.ToString();
    }

    public async Task<byte[]> DownloadFileAsync(string fileUrl)
    {
        try
        {
            // Extract blob name from URL or use as blob name directly
            string blobName;
            if (fileUrl.StartsWith("http://") || fileUrl.StartsWith("https://"))
            {
                var uri = new Uri(fileUrl);
                var segments = uri.Segments;
                // The blob name is everything after the container name
                blobName = string.Join("", segments.Skip(2)); // Skip "/" and "container/"
            }
            else
            {
                blobName = fileUrl.TrimStart('/');
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException($"File not found: {fileUrl}");
            }

            var downloadResult = await blobClient.DownloadContentAsync();
            return downloadResult.Value.Content.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from Azure Blob Storage: {FileUrl}", fileUrl);
            throw;
        }
    }
}
