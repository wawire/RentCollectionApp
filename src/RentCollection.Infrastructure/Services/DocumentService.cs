using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Documents;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAuditLogService _auditLogService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<DocumentService> _logger;
    private readonly ApplicationDbContext _context;

    // Allowed file extensions for documents
    private static readonly string[] AllowedExtensions = new[]
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx"
    };

    // Max file size: 10MB
    private const long MaxFileSize = 10 * 1024 * 1024;

    public DocumentService(
        IDocumentRepository documentRepository,
        ITenantRepository tenantRepository,
        IPropertyRepository propertyRepository,
        IUnitRepository unitRepository,
        IUserRepository userRepository,
        IFileStorageService fileStorageService,
        IAuditLogService auditLogService,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<DocumentService> logger,
        ApplicationDbContext context)
    {
        _documentRepository = documentRepository;
        _tenantRepository = tenantRepository;
        _propertyRepository = propertyRepository;
        _unitRepository = unitRepository;
        _userRepository = userRepository;
        _fileStorageService = fileStorageService;
        _auditLogService = auditLogService;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
        _context = context;
    }

    public async Task<Result<DocumentDto>> UploadDocumentAsync(IFormFile file, UploadDocumentDto uploadDto)
    {
        try
        {
            // Validate file
            var (isValid, errorMessage) = await _fileStorageService.ValidateFileAsync(
                file, AllowedExtensions, MaxFileSize);

            if (!isValid)
            {
                return Result<DocumentDto>.Failure(errorMessage);
            }

            // RBAC: Validate access based on document associations
            if (uploadDto.TenantId.HasValue)
            {
                var tenant = await _tenantRepository.GetTenantWithDetailsAsync(uploadDto.TenantId.Value);
                if (tenant == null)
                {
                    return Result<DocumentDto>.Failure($"Tenant with ID {uploadDto.TenantId} not found");
                }

                // Only tenant themselves, their landlord, or admin can upload tenant documents
                if (!_currentUserService.IsSystemAdmin)
                {
                    var landlordId = tenant.Unit?.Property?.LandlordId;

                    if (_currentUserService.IsTenant)
                    {
                        // Tenants can only upload their own documents
                        if (_currentUserService.TenantId != uploadDto.TenantId)
                        {
                            return Result<DocumentDto>.Failure("You can only upload documents for yourself");
                        }
                    }
                    else if (_currentUserService.IsLandlord || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
                    {
                        // Verify they manage this tenant
                        var userLandlordId = _currentUserService.IsLandlord
                            ? _currentUserService.UserIdInt
                            : _currentUserService.LandlordIdInt;

                        if (landlordId != userLandlordId)
                        {
                            return Result<DocumentDto>.Failure("You don't have permission to upload documents for this tenant");
                        }
                    }
                }
            }

            if (uploadDto.PropertyId.HasValue)
            {
                var property = await _propertyRepository.GetByIdAsync(uploadDto.PropertyId.Value);
                if (property == null)
                {
                    return Result<DocumentDto>.Failure($"Property with ID {uploadDto.PropertyId} not found");
                }

                // Only property owner or admin can upload property documents
                if (!_currentUserService.IsSystemAdmin)
                {
                    var userLandlordId = _currentUserService.IsLandlord
                        ? _currentUserService.UserIdInt
                        : _currentUserService.LandlordIdInt;

                    if (property.LandlordId != userLandlordId)
                    {
                        return Result<DocumentDto>.Failure("You don't have permission to upload documents for this property");
                    }
                }
            }

            if (uploadDto.UnitId.HasValue)
            {
                var unit = await _unitRepository.GetUnitWithDetailsAsync(uploadDto.UnitId.Value);
                if (unit == null)
                {
                    return Result<DocumentDto>.Failure($"Unit with ID {uploadDto.UnitId} not found");
                }

                // Only unit's landlord or admin can upload unit documents
                if (!_currentUserService.IsSystemAdmin)
                {
                    var userLandlordId = _currentUserService.IsLandlord
                        ? _currentUserService.UserIdInt
                        : _currentUserService.LandlordIdInt;

                    if (unit.Property?.LandlordId != userLandlordId)
                    {
                        return Result<DocumentDto>.Failure("You don't have permission to upload documents for this unit");
                    }
                }
            }

            // Upload file to storage
            var fileUrl = await _fileStorageService.UploadFileAsync(file, "documents");

            // Create document entity
            var document = new Document
            {
                DocumentType = uploadDto.DocumentType,
                TenantId = uploadDto.TenantId,
                PropertyId = uploadDto.PropertyId,
                UnitId = uploadDto.UnitId,
                FileName = file.FileName,
                FileUrl = fileUrl,
                FileSize = file.Length,
                ContentType = file.ContentType,
                UploadedByUserId = _currentUserService.UserIdInt!.Value,
                Description = uploadDto.Description
            };

            await _documentRepository.AddAsync(document);

            // Reload with details for DTO mapping
            var documentWithDetails = await _documentRepository.GetDocumentWithDetailsAsync(document.Id);

            var documentDto = MapToDto(documentWithDetails!);

            // Audit log
            await _auditLogService.LogActionAsync(
                "Document.Upload",
                $"Uploaded {uploadDto.DocumentType} document: {file.FileName}",
                document.Id.ToString());

            _logger.LogInformation("Document {FileName} uploaded successfully by user {UserId}",
                file.FileName, _currentUserService.UserId);

            return Result<DocumentDto>.Success(documentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return Result<DocumentDto>.Failure("An error occurred while uploading the document");
        }
    }

    public async Task<Result<IEnumerable<DocumentDto>>> GetAllDocumentsAsync()
    {
        try
        {
            var documents = await _documentRepository.GetAllAsync();

            // Apply RBAC filtering
            documents = ApplyRbacFilter(documents);

            var documentDtos = documents.Select(MapToDto);

            return Result<IEnumerable<DocumentDto>>.Success(documentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all documents");
            return Result<IEnumerable<DocumentDto>>.Failure("An error occurred while retrieving documents");
        }
    }

    public async Task<Result<DocumentDto>> GetDocumentByIdAsync(int id)
    {
        try
        {
            var document = await _documentRepository.GetDocumentWithDetailsAsync(id);

            if (document == null)
            {
                return Result<DocumentDto>.Failure($"Document with ID {id} not found");
            }

            // RBAC: Verify access
            if (!CanAccessDocument(document))
            {
                return Result<DocumentDto>.Failure("You don't have permission to view this document");
            }

            var documentDto = MapToDto(document);

            return Result<DocumentDto>.Success(documentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document {Id}", id);
            return Result<DocumentDto>.Failure("An error occurred while retrieving the document");
        }
    }

    public async Task<Result<IEnumerable<DocumentDto>>> GetDocumentsByTenantIdAsync(int tenantId)
    {
        try
        {
            var tenant = await _tenantRepository.GetTenantWithDetailsAsync(tenantId);
            if (tenant == null)
            {
                return Result<IEnumerable<DocumentDto>>.Failure($"Tenant with ID {tenantId} not found");
            }

            // RBAC: Verify access to tenant
            if (!_currentUserService.IsSystemAdmin)
            {
                var landlordId = tenant.Unit?.Property?.LandlordId;

                if (_currentUserService.IsTenant)
                {
                    if (_currentUserService.TenantId != tenantId)
                    {
                        return Result<IEnumerable<DocumentDto>>.Failure("You can only view your own documents");
                    }
                }
                else
                {
                    var userLandlordId = _currentUserService.IsLandlord
                        ? _currentUserService.UserIdInt
                        : _currentUserService.LandlordIdInt;

                    if (landlordId != userLandlordId)
                    {
                        return Result<IEnumerable<DocumentDto>>.Failure("You don't have permission to view documents for this tenant");
                    }
                }
            }

            var documents = await _documentRepository.GetDocumentsByTenantIdAsync(tenantId);
            var documentDtos = documents.Select(MapToDto);

            return Result<IEnumerable<DocumentDto>>.Success(documentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents for tenant {TenantId}", tenantId);
            return Result<IEnumerable<DocumentDto>>.Failure("An error occurred while retrieving documents");
        }
    }

    public async Task<Result<IEnumerable<DocumentDto>>> GetDocumentsByPropertyIdAsync(int propertyId)
    {
        try
        {
            var property = await _propertyRepository.GetByIdAsync(propertyId);
            if (property == null)
            {
                return Result<IEnumerable<DocumentDto>>.Failure($"Property with ID {propertyId} not found");
            }

            // RBAC: Verify access to property
            if (!_currentUserService.IsSystemAdmin)
            {
                var userLandlordId = _currentUserService.IsLandlord
                    ? _currentUserService.UserIdInt
                    : _currentUserService.LandlordIdInt;

                if (property.LandlordId != userLandlordId)
                {
                    return Result<IEnumerable<DocumentDto>>.Failure("You don't have permission to view documents for this property");
                }
            }

            var documents = await _documentRepository.GetDocumentsByPropertyIdAsync(propertyId);
            var documentDtos = documents.Select(MapToDto);

            return Result<IEnumerable<DocumentDto>>.Success(documentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents for property {PropertyId}", propertyId);
            return Result<IEnumerable<DocumentDto>>.Failure("An error occurred while retrieving documents");
        }
    }

    public async Task<Result<IEnumerable<DocumentDto>>> GetDocumentsByUnitIdAsync(int unitId)
    {
        try
        {
            var unit = await _unitRepository.GetUnitWithDetailsAsync(unitId);
            if (unit == null)
            {
                return Result<IEnumerable<DocumentDto>>.Failure($"Unit with ID {unitId} not found");
            }

            // RBAC: Verify access to unit
            if (!_currentUserService.IsSystemAdmin)
            {
                var userLandlordId = _currentUserService.IsLandlord
                    ? _currentUserService.UserIdInt
                    : _currentUserService.LandlordIdInt;

                if (unit.Property?.LandlordId != userLandlordId)
                {
                    return Result<IEnumerable<DocumentDto>>.Failure("You don't have permission to view documents for this unit");
                }
            }

            var documents = await _documentRepository.GetDocumentsByUnitIdAsync(unitId);
            var documentDtos = documents.Select(MapToDto);

            return Result<IEnumerable<DocumentDto>>.Success(documentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents for unit {UnitId}", unitId);
            return Result<IEnumerable<DocumentDto>>.Failure("An error occurred while retrieving documents");
        }
    }

    public async Task<Result<IEnumerable<DocumentDto>>> GetDocumentsByTypeAsync(DocumentType documentType)
    {
        try
        {
            var documents = await _documentRepository.GetDocumentsByTypeAsync(documentType);

            // Apply RBAC filtering
            documents = ApplyRbacFilter(documents);

            var documentDtos = documents.Select(MapToDto);

            return Result<IEnumerable<DocumentDto>>.Success(documentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents of type {DocumentType}", documentType);
            return Result<IEnumerable<DocumentDto>>.Failure("An error occurred while retrieving documents");
        }
    }

    public async Task<Result<IEnumerable<DocumentDto>>> GetUnverifiedDocumentsAsync()
    {
        try
        {
            // Only landlords and admins can see unverified documents
            if (!_currentUserService.IsSystemAdmin && !_currentUserService.IsLandlord && !_currentUserService.IsAccountant)
            {
                return Result<IEnumerable<DocumentDto>>.Failure("You don't have permission to view unverified documents");
            }

            var documents = await _documentRepository.GetUnverifiedDocumentsAsync();

            // Apply RBAC filtering for landlords/accountants
            documents = ApplyRbacFilter(documents);

            var documentDtos = documents.Select(MapToDto);

            return Result<IEnumerable<DocumentDto>>.Success(documentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unverified documents");
            return Result<IEnumerable<DocumentDto>>.Failure("An error occurred while retrieving unverified documents");
        }
    }

    public async Task<Result<IEnumerable<DocumentDto>>> GetMyDocumentsAsync()
    {
        try
        {
            if (!_currentUserService.IsTenant || !_currentUserService.TenantId.HasValue)
            {
                return Result<IEnumerable<DocumentDto>>.Failure("You must be a tenant to view your documents");
            }

            var documents = await _documentRepository.GetDocumentsByTenantIdAsync(_currentUserService.TenantId.Value);
            var documentDtos = documents.Select(MapToDto);

            return Result<IEnumerable<DocumentDto>>.Success(documentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents for current tenant");
            return Result<IEnumerable<DocumentDto>>.Failure("An error occurred while retrieving your documents");
        }
    }

    public async Task<Result<DocumentDto>> VerifyDocumentAsync(int id, VerifyDocumentDto verifyDto)
    {
        try
        {
            // Only landlords and admins can verify documents
            if (!_currentUserService.IsSystemAdmin && !_currentUserService.IsLandlord && !_currentUserService.IsAccountant)
            {
                return Result<DocumentDto>.Failure("You don't have permission to verify documents");
            }

            var document = await _documentRepository.GetDocumentWithDetailsAsync(id);
            if (document == null)
            {
                return Result<DocumentDto>.Failure($"Document with ID {id} not found");
            }

            // RBAC: Verify access
            if (!CanAccessDocument(document))
            {
                return Result<DocumentDto>.Failure("You don't have permission to verify this document");
            }

            document.IsVerified = verifyDto.IsVerified;
            document.VerifiedByUserId = _currentUserService.UserIdInt!.Value;
            document.VerifiedAt = DateTime.UtcNow;

            await _documentRepository.UpdateAsync(document);

            // Reload with details
            var documentWithDetails = await _documentRepository.GetDocumentWithDetailsAsync(id);
            var documentDto = MapToDto(documentWithDetails!);

            // Audit log
            await _auditLogService.LogActionAsync(
                "Document.Verify",
                $"Verified document: {document.FileName} ({document.DocumentType})",
                id.ToString());

            return Result<DocumentDto>.Success(documentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying document {Id}", id);
            return Result<DocumentDto>.Failure("An error occurred while verifying the document");
        }
    }

    public async Task<Result> DeleteDocumentAsync(int id)
    {
        try
        {
            var document = await _documentRepository.GetDocumentWithDetailsAsync(id);
            if (document == null)
            {
                return Result.Failure($"Document with ID {id} not found");
            }

            // RBAC: Verify access
            if (!CanAccessDocument(document))
            {
                return Result.Failure("You don't have permission to delete this document");
            }

            // Delete file from storage
            await _fileStorageService.DeleteFileAsync(document.FileUrl);

            // Delete from database
            await _documentRepository.DeleteAsync(document);

            // Audit log
            await _auditLogService.LogActionAsync(
                "Document.Delete",
                $"Deleted document: {document.FileName} ({document.DocumentType})",
                id.ToString());

            _logger.LogInformation("Document {Id} deleted successfully by user {UserId}",
                id, _currentUserService.UserId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {Id}", id);
            return Result.Failure("An error occurred while deleting the document");
        }
    }

    // Helper methods

    private IEnumerable<Document> ApplyRbacFilter(IEnumerable<Document> documents)
    {
        if (_currentUserService.IsSystemAdmin)
        {
            return documents;
        }

        if (_currentUserService.IsTenant)
        {
            // Tenants can only see their own documents
            if (_currentUserService.TenantId.HasValue)
            {
                return documents.Where(d => d.TenantId == _currentUserService.TenantId.Value);
            }
            return Enumerable.Empty<Document>();
        }

        // Landlords, Caretakers, and Accountants can see documents for their properties
        var landlordId = _currentUserService.IsLandlord
            ? _currentUserService.UserIdInt
            : _currentUserService.LandlordIdInt;

        if (landlordId.HasValue)
        {
            return documents.Where(d =>
                d.Property?.LandlordId == landlordId.Value ||
                d.Unit?.Property?.LandlordId == landlordId.Value ||
                d.Tenant?.Unit?.Property?.LandlordId == landlordId.Value);
        }

        return Enumerable.Empty<Document>();
    }

    private bool CanAccessDocument(Document document)
    {
        if (_currentUserService.IsSystemAdmin)
        {
            return true;
        }

        if (_currentUserService.IsTenant)
        {
            return _currentUserService.TenantId == document.TenantId;
        }

        // Landlords, Caretakers, and Accountants
        var landlordId = _currentUserService.IsLandlord
            ? _currentUserService.UserIdInt
            : _currentUserService.LandlordIdInt;

        if (landlordId.HasValue)
        {
            return document.Property?.LandlordId == landlordId.Value ||
                   document.Unit?.Property?.LandlordId == landlordId.Value ||
                   document.Tenant?.Unit?.Property?.LandlordId == landlordId.Value;
        }

        return false;
    }

    private static DocumentDto MapToDto(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            DocumentType = document.DocumentType,
            DocumentTypeName = document.DocumentType.ToString(),
            TenantId = document.TenantId,
            TenantName = document.Tenant?.FullName,
            PropertyId = document.PropertyId,
            PropertyName = document.Property?.Name,
            UnitId = document.UnitId,
            UnitNumber = document.Unit?.UnitNumber,
            FileName = document.FileName,
            FileUrl = document.FileUrl,
            FileSize = document.FileSize,
            FileSizeFormatted = FormatFileSize(document.FileSize),
            ContentType = document.ContentType,
            UploadedByUserId = document.UploadedByUserId,
            UploadedByName = document.UploadedBy?.FullName ?? "Unknown",
            UploadedAt = document.UploadedAt,
            Description = document.Description,
            IsVerified = document.IsVerified,
            VerifiedByUserId = document.VerifiedByUserId,
            VerifiedByName = document.VerifiedBy?.FullName,
            VerifiedAt = document.VerifiedAt
        };
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
