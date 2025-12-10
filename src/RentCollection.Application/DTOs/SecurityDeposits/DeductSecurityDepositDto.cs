using System.ComponentModel.DataAnnotations;

namespace RentCollection.Application.DTOs.SecurityDeposits;

public class DeductSecurityDepositDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string Reason { get; set; } = string.Empty;

    public int? RelatedMaintenanceRequestId { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
