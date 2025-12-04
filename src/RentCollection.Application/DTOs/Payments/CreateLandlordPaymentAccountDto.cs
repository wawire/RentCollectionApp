using System.ComponentModel.DataAnnotations;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Payments;

/// <summary>
/// DTO for creating a new landlord payment account
/// </summary>
public class CreateLandlordPaymentAccountDto
{
    public int? PropertyId { get; set; }

    [Required(ErrorMessage = "Account name is required")]
    [StringLength(100)]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    public PaymentAccountType AccountType { get; set; }

    // M-Pesa Paybill Details
    public string? PaybillNumber { get; set; }
    public string? PaybillName { get; set; }

    // M-Pesa Till Number
    public string? TillNumber { get; set; }

    // M-Pesa Phone Number
    [Phone]
    public string? MPesaPhoneNumber { get; set; }

    // Bank Details
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
    public string? BankBranch { get; set; }
    public string? SwiftCode { get; set; }

    // M-Pesa API Credentials (for STK Push and webhooks)
    public string? MPesaConsumerKey { get; set; }
    public string? MPesaConsumerSecret { get; set; }
    public string? MPesaShortCode { get; set; }
    public string? MPesaPasskey { get; set; }

    // Settings
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool AutoReconciliation { get; set; } = false;

    public string? PaymentInstructions { get; set; }
}
