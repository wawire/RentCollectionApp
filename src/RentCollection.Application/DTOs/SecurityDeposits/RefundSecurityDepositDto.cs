using System.ComponentModel.DataAnnotations;

namespace RentCollection.Application.DTOs.SecurityDeposits;

public class RefundSecurityDepositDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime RefundDate { get; set; }

    [Required]
    [StringLength(500)]
    public string RefundMethod { get; set; } = string.Empty; // e.g., "M-Pesa", "Bank Transfer", "Cash"

    [StringLength(100)]
    public string? TransactionReference { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
