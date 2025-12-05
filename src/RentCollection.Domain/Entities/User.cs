using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

/// <summary>
/// User entity for authentication and authorization
/// </summary>
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;

    /// <summary>
    /// Organization/Company ID for multi-tenancy (future use)
    /// </summary>
    public int? OrganizationId { get; set; }

    /// <summary>
    /// For Landlords and Caretakers - links to specific properties they manage
    /// Null for SystemAdmin and Accountant (they see all)
    /// </summary>
    public int? PropertyId { get; set; }
    public Property? Property { get; set; }

    /// <summary>
    /// For Tenant role - links to their tenant record
    /// </summary>
    public int? TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// For Landlord role - properties they own
    /// </summary>
    public ICollection<Property> OwnedProperties { get; set; } = new List<Property>();

    /// <summary>
    /// For Landlord role - payment accounts they own
    /// </summary>
    public ICollection<LandlordPaymentAccount> PaymentAccounts { get; set; } = new List<LandlordPaymentAccount>();

    /// <summary>
    /// Full name for display
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
}
