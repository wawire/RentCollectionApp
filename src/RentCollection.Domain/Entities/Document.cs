using RentCollection.Domain.Common;
using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

/// <summary>
/// Document entity for storing uploaded files (lease agreements, ID copies, etc.)
/// </summary>
public class Document : BaseEntity
{
    /// <summary>
    /// Type of document
    /// </summary>
    public DocumentType DocumentType { get; set; }

    /// <summary>
    /// Optional: Tenant this document belongs to (for tenant-specific docs)
    /// </summary>
    public int? TenantId { get; set; }

    /// <summary>
    /// Optional: Property this document relates to (for property-specific docs)
    /// </summary>
    public int? PropertyId { get; set; }

    /// <summary>
    /// Optional: Unit this document relates to (for unit-specific docs)
    /// </summary>
    public int? UnitId { get; set; }

    /// <summary>
    /// Optional: Landlord this document relates to (for landlord-level docs)
    /// </summary>
    public int? LandlordId { get; set; }

    /// <summary>
    /// Original filename as uploaded
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// URL/path to the stored file
    /// </summary>
    public string FileUrl { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// MIME type of the file (e.g., "application/pdf", "image/jpeg")
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// User ID of the person who uploaded the document
    /// </summary>
    public int UploadedByUserId { get; set; }

    /// <summary>
    /// Date and time when document was uploaded
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional description or notes about the document
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indicates if document has been verified/approved by landlord
    /// </summary>
    public bool IsVerified { get; set; } = false;

    /// <summary>
    /// User ID of the person who verified the document
    /// </summary>
    public int? VerifiedByUserId { get; set; }

    /// <summary>
    /// Date and time when document was verified
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    // Navigation properties
    public Tenant? Tenant { get; set; }
    public Property? Property { get; set; }
    public Unit? Unit { get; set; }
    public User UploadedBy { get; set; } = null!;
    public User? VerifiedBy { get; set; }
}
