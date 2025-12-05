using RentCollection.Domain.Common;
using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

/// <summary>
/// Represents a payment account where landlords receive rent payments
/// Supports M-Pesa Paybill, Till Number, Bank Accounts, etc.
/// </summary>
public class LandlordPaymentAccount : BaseEntity
{
    // Landlord/Property Association
    public int LandlordId { get; set; }
    public int? PropertyId { get; set; }  // Can be property-specific or general

    // Account Details
    public string AccountName { get; set; } = string.Empty;  // e.g., "Sunrise Apartments Paybill"
    public PaymentAccountType AccountType { get; set; }

    // M-Pesa Paybill Details
    public string? PaybillNumber { get; set; }  // e.g., "123456"
    public string? PaybillName { get; set; }    // Business name registered with Paybill

    // M-Pesa Till Number (Alternative)
    public string? TillNumber { get; set; }

    // M-Pesa Phone Number (For small landlords)
    public string? MPesaPhoneNumber { get; set; }

    // Bank Details (Equity Bank, KCB, etc.)
    public string? BankName { get; set; }  // e.g., "Equity Bank"
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
    public string? BankBranch { get; set; }
    public string? SwiftCode { get; set; }

    // M-Pesa API Integration (For STK Push and Webhooks)
    public string? MPesaConsumerKey { get; set; }
    public string? MPesaConsumerSecret { get; set; }
    public string? MPesaShortCode { get; set; }  // Business shortcode for Daraja API
    public string? MPesaPasskey { get; set; }    // Passkey for STK Push

    // Settings
    public bool IsDefault { get; set; }  // Default account for this landlord/property
    public bool IsActive { get; set; } = true;
    public bool AutoReconciliation { get; set; }  // Enable automatic payment matching via webhooks

    // Instructions for tenants
    public string? PaymentInstructions { get; set; }

    // Navigation properties
    public User Landlord { get; set; } = null!;
    public Property? Property { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
