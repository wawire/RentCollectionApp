namespace RentCollection.Application.DTOs.BulkImport
{
    public class BulkImportResultDto
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int TotalCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> SuccessMessages { get; set; } = new List<string>();
        public bool IsSuccess => FailureCount == 0;
        public string Summary => $"Successfully imported {SuccessCount}/{TotalCount} records. {FailureCount} failed.";
    }

    public class TenantImportRow
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public DateTime LeaseStartDate { get; set; }
        public DateTime? LeaseEndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal? SecurityDeposit { get; set; }
        public string? IdNumber { get; set; }
    }

    public class PaymentImportRow
    {
        public string TenantEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }
}
