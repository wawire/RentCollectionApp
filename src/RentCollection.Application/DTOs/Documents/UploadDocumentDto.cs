using System.ComponentModel.DataAnnotations;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Documents;

/// <summary>
/// DTO for uploading a new document
/// </summary>
public class UploadDocumentDto
{
    [Required]
    public DocumentType DocumentType { get; set; }

    public int? TenantId { get; set; }
    public int? PropertyId { get; set; }
    public int? UnitId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
