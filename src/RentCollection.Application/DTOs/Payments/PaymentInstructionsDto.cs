using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Payments;

/// <summary>
/// Payment instructions for a tenant to make rent payment
/// </summary>
public class PaymentInstructionsDto
{
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public int UnitId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public decimal MonthlyRent { get; set; }

    // Landlord details
    public string LandlordName { get; set; } = string.Empty;
    public string? LandlordPhone { get; set; }

    // Payment account details
    public int LandlordAccountId { get; set; }
    public PaymentAccountType AccountType { get; set; }
    public string AccountName { get; set; } = string.Empty;

    // M-Pesa Paybill details (if applicable)
    public string? PaybillNumber { get; set; }
    public string? PaybillName { get; set; }
    public string? AccountNumber { get; set; }  // Unit's payment account number

    // M-Pesa Till Number (if applicable)
    public string? TillNumber { get; set; }

    // M-Pesa Phone (if applicable)
    public string? MPesaPhoneNumber { get; set; }

    // Bank details (if applicable)
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
    public string? BankBranch { get; set; }

    // Payment reference for bank transfers
    public string? ReferenceCode { get; set; }

    // Custom instructions
    public string? PaymentInstructions { get; set; }
}
