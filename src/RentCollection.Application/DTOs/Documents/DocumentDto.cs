namespace RentCollection.Application.DTOs.Documents
{
    public class DocumentDto
    {
        public int Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public string? TenantName { get; set; }
        public int? PropertyId { get; set; }
        public string? PropertyName { get; set; }
        public int? UnitId { get; set; }
        public string? UnitNumber { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileSizeFormatted { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int UploadedByUserId { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public bool IsVerified { get; set; }
        public int? VerifiedByUserId { get; set; }
        public string? VerifiedByName { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
