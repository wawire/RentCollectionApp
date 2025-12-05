using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Payments;

/// <summary>
/// DTO for landlord payment account information
/// </summary>
public class LandlordPaymentAccountDto
{
    public int Id { get; set; }
    public int LandlordId { get; set; }
    public int? PropertyId { get; set; }
    public string? PropertyName { get; set; }

    public string AccountName { get; set; } = string.Empty;
    public PaymentAccountType AccountType { get; set; }

    // M-Pesa Paybill
    public string? PaybillNumber { get; set; }
    public string? PaybillName { get; set; }

    // M-Pesa Till Number
    public string? TillNumber { get; set; }

    // M-Pesa Phone
    public string? MPesaPhoneNumber { get; set; }

    // Bank Details
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
    public string? BankBranch { get; set; }
    public string? SwiftCode { get; set; }

    // Settings
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public bool AutoReconciliation { get; set; }

    public string? PaymentInstructions { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
