namespace RentCollection.Application.DTOs.TenantPortal;

/// <summary>
/// Comprehensive lease information for tenant portal
/// </summary>
public class TenantLeaseInfoDto
{
    /// <summary>
    /// Tenant details
    /// </summary>
    public TenantDetailsDto Tenant { get; set; } = null!;

    /// <summary>
    /// Property details
    /// </summary>
    public PropertyDetailsDto Property { get; set; } = null!;

    /// <summary>
    /// Unit details
    /// </summary>
    public UnitDetailsDto Unit { get; set; } = null!;

    /// <summary>
    /// Lease agreement details
    /// </summary>
    public LeaseDetailsDto Lease { get; set; } = null!;

    /// <summary>
    /// Payment account information
    /// </summary>
    public PaymentAccountInfoDto PaymentAccount { get; set; } = null!;
}

public class TenantDetailsDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? IdNumber { get; set; }
}

public class PropertyDetailsDto
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Description { get; set; }
    public List<string> Amenities { get; set; } = new();
}

public class UnitDetailsDto
{
    public string UnitNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal SquareFeet { get; set; }
    public string? Description { get; set; }
}

public class LeaseDetailsDto
{
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal? SecurityDeposit { get; set; }
    public int RentDueDay { get; set; }
    public int LateFeeGracePeriodDays { get; set; }
    public decimal LateFeePercentage { get; set; }
    public decimal? LateFeeFixedAmount { get; set; }
    public string LateFeePolicy { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int? DaysUntilExpiry { get; set; }
}

public class PaymentAccountInfoDto
{
    public string AccountType { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? MPesaPaybill { get; set; }
    public string? PaymentAccountNumber { get; set; }
    public string Instructions { get; set; } = string.Empty;
}
