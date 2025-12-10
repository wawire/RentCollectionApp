using System.ComponentModel.DataAnnotations;

namespace RentCollection.Application.DTOs.SecurityDeposits;

public class RecordSecurityDepositDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime TransactionDate { get; set; }

    public int? RelatedPaymentId { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
