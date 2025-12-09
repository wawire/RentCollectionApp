using Microsoft.AspNetCore.Http;

namespace RentCollection.Application.DTOs.Documents
{
    public class UploadDocumentDto
    {
        public IFormFile File { get; set; } = null!;
        public string DocumentType { get; set; } = string.Empty;
        public int? TenantId { get; set; }
        public string? Description { get; set; }
    }
}
