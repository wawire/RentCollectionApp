# Property Image Handling - Current State & Recommendations

**Date:** 2025-12-06
**Status:** FEATURE INCOMPLETE

---

## Current Implementation

### What Works:
✅ **Property entity has ImageUrl field** (Property.cs:10)
```csharp
public string? ImageUrl { get; set; }
```

✅ **DTOs support ImageUrl**
- `CreatePropertyDto` - Accept ImageUrl when creating property
- `UpdatePropertyDto` - Accept ImageUrl when updating property
- `PropertyDto` - Return ImageUrl in API responses

✅ **API accepts ImageUrl strings**
```json
POST /api/properties
{
  "name": "Sunset Apartments",
  "location": "Nairobi",
  "imageUrl": "https://example.com/property.jpg",
  "totalUnits": 20
}
```

### What Doesn't Work:
❌ **No file upload endpoint** - Can't upload actual image files
❌ **No file storage service** - No Azure Blob, AWS S3, or local storage
❌ **No file validation** - No checks for file type, size, or content
❌ **No image processing** - No resize, compression, or format conversion
❌ **Security gaps** - No virus scanning or malicious file detection

---

## Current Workaround

**You can use external image URLs:**

1. Upload image to external service (Imgur, Cloudinary, etc.)
2. Copy the public URL
3. Paste URL into ImageUrl field when creating property

**Limitations:**
- Relies on third-party services
- No control over image lifecycle
- External URLs can break if service goes down
- No standardized image dimensions/quality

---

## Recommended Implementation

### Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                    Client (Next.js)                      │
│  - Image selection (file input)                         │
│  - Preview before upload                                │
│  - Progress indicator                                   │
└─────────────────┬───────────────────────────────────────┘
                  │ HTTP POST with multipart/form-data
                  │ (IFormFile)
┌─────────────────▼───────────────────────────────────────┐
│            PropertiesController                          │
│  - [Authorize(Roles = "SystemAdmin,Landlord")]          │
│  - POST /api/properties/{id}/image                      │
│  - Validates: file type, size, RBAC                     │
└─────────────────┬───────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────────────┐
│               PropertyService                            │
│  - Checks property ownership                            │
│  - Calls IFileStorageService                            │
│  - Updates Property.ImageUrl                            │
└─────────────────┬───────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────────────┐
│           IFileStorageService                            │
│  - Validates file (type, size, content)                 │
│  - Generates unique filename                            │
│  - Stores file (cloud or local)                         │
│  - Returns public URL                                   │
└─────────────────────────────────────────────────────────┘
                  │
    ┌─────────────┴──────────────┐
    │                            │
┌───▼────────────┐   ┌───────────▼──────────┐
│ Azure Blob     │   │ Local File System    │
│ Storage        │   │ /wwwroot/uploads/    │
│ (Production)   │   │ (Development)        │
└────────────────┘   └──────────────────────┘
```

---

## Implementation Plan

### Phase 1: File Storage Service (REQUIRED)

**Create Interface:**

```csharp
// src/RentCollection.Application/Interfaces/IFileStorageService.cs
namespace RentCollection.Application.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Upload a file and return the public URL
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="folder">Folder/container name (e.g., "properties", "payments")</param>
    /// <returns>Public URL of the uploaded file</returns>
    Task<string> UploadFileAsync(IFormFile file, string folder);

    /// <summary>
    /// Delete a file by URL
    /// </summary>
    Task<bool> DeleteFileAsync(string fileUrl);

    /// <summary>
    /// Validate file before upload
    /// </summary>
    Task<(bool IsValid, string ErrorMessage)> ValidateFileAsync(IFormFile file);
}
```

**Implementation Option A: Local File Storage (Development)**

```csharp
// src/RentCollection.Infrastructure/Services/LocalFileStorageService.cs
using Microsoft.AspNetCore.Http;
using RentCollection.Application.Interfaces;

namespace RentCollection.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadPath;
    private readonly IWebHostEnvironment _environment;
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
        _uploadPath = Path.Combine(_environment.WebRootPath, "uploads");

        // Create uploads directory if it doesn't exist
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder)
    {
        // Validate first
        var (isValid, errorMessage) = await ValidateFileAsync(file);
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
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(folderPath, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative URL
        return $"/uploads/{folder}/{fileName}";
    }

    public Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            // Convert URL to file path
            var relativePath = fileUrl.TrimStart('/');
            var filePath = Path.Combine(_environment.WebRootPath, relativePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<(bool IsValid, string ErrorMessage)> ValidateFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Task.FromResult((false, "No file provided"));
        }

        // Check file size
        if (file.Length > _maxFileSize)
        {
            return Task.FromResult((false, $"File size exceeds maximum allowed size of {_maxFileSize / 1024 / 1024}MB"));
        }

        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            return Task.FromResult((false, $"File type not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}"));
        }

        // Check MIME type
        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return Task.FromResult((false, "Invalid file content type"));
        }

        return Task.FromResult((true, string.Empty));
    }
}
```

**Implementation Option B: Azure Blob Storage (Production)**

```csharp
// src/RentCollection.Infrastructure/Services/AzureBlobStorageService.cs
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RentCollection.Application.Interfaces;

namespace RentCollection.Infrastructure.Services;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"];
        _containerName = configuration["AzureStorage:ContainerName"] ?? "rent-collection-images";
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder)
    {
        // Validate first
        var (isValid, errorMessage) = await ValidateFileAsync(file);
        if (!isValid)
        {
            throw new InvalidOperationException(errorMessage);
        }

        // Get container
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        // Generate blob name
        var blobName = $"{folder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var blobClient = containerClient.GetBlobClient(blobName);

        // Upload
        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = file.ContentType
            });
        }

        // Return public URL
        return blobClient.Uri.ToString();
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var blobName = uri.AbsolutePath.TrimStart('/').Substring(_containerName.Length + 1);

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            return await blobClient.DeleteIfExistsAsync();
        }
        catch
        {
            return false;
        }
    }

    public Task<(bool IsValid, string ErrorMessage)> ValidateFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Task.FromResult((false, "No file provided"));
        }

        if (file.Length > _maxFileSize)
        {
            return Task.FromResult((false, $"File size exceeds maximum allowed size of {_maxFileSize / 1024 / 1024}MB"));
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            return Task.FromResult((false, $"File type not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}"));
        }

        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return Task.FromResult((false, "Invalid file content type"));
        }

        return Task.FromResult((true, string.Empty));
    }
}
```

---

### Phase 2: Property Image Upload Endpoint

**Add to PropertiesController:**

```csharp
// src/RentCollection.API/Controllers/PropertiesController.cs

/// <summary>
/// Upload property image
/// </summary>
/// <param name="id">Property ID</param>
/// <param name="file">Image file (JPG, PNG, WEBP, max 5MB)</param>
/// <returns>Updated property with new image URL</returns>
[HttpPost("{id}/image")]
[Authorize(Roles = "SystemAdmin,Landlord")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public async Task<IActionResult> UploadPropertyImage(int id, IFormFile file)
{
    if (file == null || file.Length == 0)
    {
        return BadRequest(new { message = "No file provided" });
    }

    var result = await _propertyService.UploadPropertyImageAsync(id, file);

    if (!result.IsSuccess)
    {
        return BadRequest(result);
    }

    return Ok(result);
}

/// <summary>
/// Delete property image
/// </summary>
/// <param name="id">Property ID</param>
/// <returns>Success message</returns>
[HttpDelete("{id}/image")]
[Authorize(Roles = "SystemAdmin,Landlord")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public async Task<IActionResult> DeletePropertyImage(int id)
{
    var result = await _propertyService.DeletePropertyImageAsync(id);

    if (!result.IsSuccess)
    {
        return BadRequest(result);
    }

    return Ok(result);
}
```

---

### Phase 3: Property Service Methods

**Add to IPropertyService:**

```csharp
// src/RentCollection.Application/Services/Interfaces/IPropertyService.cs

Task<Result<PropertyDto>> UploadPropertyImageAsync(int propertyId, IFormFile file);
Task<Result> DeletePropertyImageAsync(int propertyId);
```

**Implementation in PropertyService:**

```csharp
// src/RentCollection.Application/Services/Implementations/PropertyService.cs

private readonly IFileStorageService _fileStorageService;

public PropertyService(
    IPropertyRepository propertyRepository,
    IMapper mapper,
    ILogger<PropertyService> logger,
    ICurrentUserService currentUserService,
    IFileStorageService fileStorageService) // ADD THIS
{
    _propertyRepository = propertyRepository;
    _mapper = mapper;
    _logger = logger;
    _currentUserService = currentUserService;
    _fileStorageService = fileStorageService; // ADD THIS
}

public async Task<Result<PropertyDto>> UploadPropertyImageAsync(int propertyId, IFormFile file)
{
    try
    {
        var property = await _propertyRepository.GetByIdAsync(propertyId);

        if (property == null)
        {
            return Result<PropertyDto>.Failure($"Property with ID {propertyId} not found");
        }

        // RBAC: Only SystemAdmin or property owner can upload images
        if (!_currentUserService.IsSystemAdmin)
        {
            if (!_currentUserService.IsLandlord)
            {
                return Result<PropertyDto>.Failure("Only landlords can upload property images");
            }

            var landlordId = _currentUserService.UserIdInt;
            if (landlordId.HasValue && property.LandlordId != landlordId.Value)
            {
                return Result<PropertyDto>.Failure("You do not have permission to upload images for this property");
            }
        }

        // Delete old image if exists
        if (!string.IsNullOrEmpty(property.ImageUrl))
        {
            await _fileStorageService.DeleteFileAsync(property.ImageUrl);
        }

        // Upload new image
        var imageUrl = await _fileStorageService.UploadFileAsync(file, "properties");

        // Update property
        property.ImageUrl = imageUrl;
        property.UpdatedAt = DateTime.UtcNow;
        await _propertyRepository.UpdateAsync(property);

        var propertyDto = _mapper.Map<PropertyDto>(property);

        _logger.LogInformation("Property image uploaded successfully: {PropertyId}, URL: {ImageUrl}",
            propertyId, imageUrl);

        return Result<PropertyDto>.Success(propertyDto, "Property image uploaded successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error uploading property image for property {PropertyId}", propertyId);
        return Result<PropertyDto>.Failure("An error occurred while uploading the property image");
    }
}

public async Task<Result> DeletePropertyImageAsync(int propertyId)
{
    try
    {
        var property = await _propertyRepository.GetByIdAsync(propertyId);

        if (property == null)
        {
            return Result.Failure($"Property with ID {propertyId} not found");
        }

        // RBAC check
        if (!_currentUserService.IsSystemAdmin)
        {
            if (!_currentUserService.IsLandlord)
            {
                return Result.Failure("Only landlords can delete property images");
            }

            var landlordId = _currentUserService.UserIdInt;
            if (landlordId.HasValue && property.LandlordId != landlordId.Value)
            {
                return Result.Failure("You do not have permission to delete images for this property");
            }
        }

        if (string.IsNullOrEmpty(property.ImageUrl))
        {
            return Result.Failure("Property has no image to delete");
        }

        // Delete file
        await _fileStorageService.DeleteFileAsync(property.ImageUrl);

        // Update property
        property.ImageUrl = null;
        property.UpdatedAt = DateTime.UtcNow;
        await _propertyRepository.UpdateAsync(property);

        _logger.LogInformation("Property image deleted successfully: {PropertyId}", propertyId);

        return Result.Success("Property image deleted successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error deleting property image for property {PropertyId}", propertyId);
        return Result.Failure("An error occurred while deleting the property image");
    }
}
```

---

### Phase 4: Register Service in DI Container

**Update Program.cs or Startup.cs:**

```csharp
// For Development (Local Storage)
builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();

// OR for Production (Azure Blob)
builder.Services.AddSingleton<IFileStorageService, AzureBlobStorageService>();

// OR environment-based
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();
}
else
{
    builder.Services.AddSingleton<IFileStorageService, AzureBlobStorageService>();
}
```

**For Azure Blob, add to appsettings.json:**

```json
{
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=youraccountname;AccountKey=yourkey;EndpointSuffix=core.windows.net",
    "ContainerName": "rent-collection-images"
  }
}
```

---

### Phase 5: Frontend Implementation (Next.js)

**Property Image Upload Component:**

```tsx
// components/PropertyImageUpload.tsx
'use client';

import { useState } from 'react';

interface Props {
  propertyId: number;
  currentImageUrl?: string;
  onUploadSuccess?: (newImageUrl: string) => void;
}

export function PropertyImageUpload({ propertyId, currentImageUrl, onUploadSuccess }: Props) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(currentImageUrl || null);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validate file type
    if (!file.type.startsWith('image/')) {
      setError('Please select an image file');
      return;
    }

    // Validate file size (5MB)
    if (file.size > 5 * 1024 * 1024) {
      setError('File size must be less than 5MB');
      return;
    }

    setSelectedFile(file);
    setError(null);

    // Create preview
    const reader = new FileReader();
    reader.onloadend = () => {
      setPreview(reader.result as string);
    };
    reader.readAsDataURL(file);
  };

  const handleUpload = async () => {
    if (!selectedFile) return;

    setUploading(true);
    setError(null);

    const formData = new FormData();
    formData.append('file', selectedFile);

    try {
      const response = await fetch(`/api/properties/${propertyId}/image`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
        },
        body: formData,
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Upload failed');
      }

      const result = await response.json();
      onUploadSuccess?.(result.data.imageUrl);
      setError(null);
    } catch (err: any) {
      setError(err.message || 'Failed to upload image');
    } finally {
      setUploading(false);
    }
  };

  const handleDelete = async () => {
    if (!currentImageUrl) return;

    try {
      const response = await fetch(`/api/properties/${propertyId}/image`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
        },
      });

      if (!response.ok) {
        throw new Error('Delete failed');
      }

      setPreview(null);
      setSelectedFile(null);
      onUploadSuccess?.('');
    } catch (err: any) {
      setError(err.message || 'Failed to delete image');
    }
  };

  return (
    <div className="space-y-4">
      <div className="border-2 border-dashed border-gray-300 rounded-lg p-6">
        {preview ? (
          <div className="relative">
            <img src={preview} alt="Property" className="w-full h-64 object-cover rounded" />
            <button
              onClick={handleDelete}
              className="absolute top-2 right-2 bg-red-600 text-white px-3 py-1 rounded"
            >
              Delete
            </button>
          </div>
        ) : (
          <div className="text-center">
            <input
              type="file"
              accept="image/*"
              onChange={handleFileSelect}
              className="hidden"
              id="property-image-input"
            />
            <label
              htmlFor="property-image-input"
              className="cursor-pointer text-blue-600 hover:text-blue-700"
            >
              Choose an image
            </label>
          </div>
        )}
      </div>

      {error && (
        <div className="text-red-600 text-sm">{error}</div>
      )}

      {selectedFile && (
        <button
          onClick={handleUpload}
          disabled={uploading}
          className="w-full bg-blue-600 text-white py-2 rounded hover:bg-blue-700 disabled:opacity-50"
        >
          {uploading ? 'Uploading...' : 'Upload Image'}
        </button>
      )}
    </div>
  );
}
```

---

## Security Considerations

### File Validation (CRITICAL)

1. **File Type Validation:**
   - Check file extension (.jpg, .jpeg, .png, .webp only)
   - Verify MIME type matches extension
   - Read file header bytes to confirm actual file type

2. **File Size Limits:**
   - Maximum 5MB per image
   - Configure in appsettings.json for easy updates

3. **Malicious Content Detection:**
   - Consider using antivirus scanning service
   - Validate image dimensions (reject 0x0 or extremely large)
   - Strip EXIF metadata (may contain sensitive location data)

4. **File Name Sanitization:**
   - Always generate new filename (GUID-based)
   - Never use user-provided filenames directly
   - Prevent path traversal attacks

### RBAC for Image Upload

**Who can upload property images:**
- ✅ SystemAdmin (all properties)
- ✅ Landlord (own properties only)
- ❌ Caretaker (blocked)
- ❌ Accountant (blocked)
- ❌ Tenant (blocked)

**Rationale:** Property images are part of property marketing/listing. Only property owners should control property presentation.

---

## Missing Features (Future Enhancements)

1. **Image Processing:**
   - Auto-resize to standard dimensions (e.g., 1200x800)
   - Generate thumbnails (e.g., 300x200)
   - Compress images to reduce storage/bandwidth
   - Convert to WebP for better compression

2. **Multiple Images Per Property:**
   - Support image gallery (5-10 images per property)
   - Set primary/featured image
   - Reorder images

3. **Unit Images:**
   - Each unit can have its own images
   - Show unit-specific photos (kitchen, bathroom, living room)

4. **Image CDN:**
   - Integrate with CDN (Cloudflare, CloudFront)
   - Faster global image delivery
   - Automatic caching

5. **Payment Proof Upload:**
   - Implement TenantPaymentsController.UploadPaymentProof (currently TODO)
   - Tenants upload M-Pesa screenshots
   - Landlords review payment proof

---

## Cost Considerations

### Azure Blob Storage Pricing (Approximate)

**Storage Costs:**
- LRS (Locally Redundant Storage): ~$0.0184/GB/month
- 1000 properties × 1 image × 1MB = ~1GB = **$0.02/month**

**Operation Costs:**
- Write operations: $0.055 per 10,000 transactions
- Read operations: $0.0044 per 10,000 transactions
- 1000 uploads/month = **$0.006**

**Total Monthly Cost (estimated):** ~$0.03 - $0.10 for small deployments

### Local Storage (Free)

- Uses server disk space
- No external service costs
- Requires backup strategy
- Limited scalability

---

## Recommended Next Steps

### Immediate (This Sprint):

1. ✅ **Decide on storage strategy:**
   - Development: Local file storage
   - Production: Azure Blob Storage OR AWS S3

2. ✅ **Implement IFileStorageService:**
   - Create interface
   - Implement LocalFileStorageService for dev
   - Add validation logic

3. ✅ **Add property image upload endpoint:**
   - POST /api/properties/{id}/image
   - DELETE /api/properties/{id}/image
   - RBAC: SystemAdmin, Landlord only

4. ✅ **Test with Postman/frontend:**
   - Upload .jpg, .png images
   - Verify URLs returned correctly
   - Test RBAC (Caretaker should be blocked)

### Future (Next Sprint):

1. Implement payment proof upload
2. Add unit images support
3. Add image processing (resize, compress)
4. Integrate with CDN
5. Support multiple images per property

---

## References

- **Azure Blob Storage Docs:** https://learn.microsoft.com/en-us/azure/storage/blobs/
- **AWS S3 .NET SDK:** https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/s3.html
- **File Upload Security:** https://owasp.org/www-community/vulnerabilities/Unrestricted_File_Upload
- **Image Processing with ImageSharp:** https://docs.sixlabors.com/articles/imagesharp/

---

**Conclusion:**

Property images are **partially implemented** (URL field exists) but **file upload is missing**. Recommend implementing file storage service + upload endpoint following the architecture above. Start with local storage for development, then migrate to Azure Blob for production.

**RBAC:** Only SystemAdmin and Landlords should upload property images.
**Security:** Validate file type, size, and content before storing.
**Cost:** Extremely low (~$0.03-$0.10/month for small deployments).
