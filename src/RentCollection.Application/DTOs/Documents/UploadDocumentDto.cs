using Microsoft.AspNetCore.Http;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Documents
{
    public class UploadDocumentDto
    {
        public IFormFile File { get; set; } = null!;
        public DocumentType DocumentType { get; set; }
        public int? TenantId { get; set; }
        public int? PropertyId { get; set; }
        public int? UnitId { get; set; }
        public string? Description { get; set; }
    }
}
