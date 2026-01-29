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
    /// Organization/Company ID for multi-tenancy
    /// </summary>
    public int OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    /// <summary>
    /// For Landlords and Caretakers - links to specific properties they manage
    /// Null for PlatformAdmin and Accountant (they see all)
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
    public int LoginAttempts { get; set; }
    public DateTime? LoginLockoutUntil { get; set; }
    public DateTime? LastFailedLoginAt { get; set; }

    /// <summary>
    /// Account verification status
    /// </summary>
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public VerificationChannel? VerificationChannel { get; set; }

    /// <summary>
    /// OTP verification details
    /// </summary>
    public string? OtpHash { get; set; }
    public DateTime? OtpExpiresAt { get; set; }
    public int OtpAttempts { get; set; }
    public DateTime? OtpLastSentAt { get; set; }
    public DateTime? OtpLockoutUntil { get; set; }

    /// <summary>
    /// Force password change on first login or after invite
    /// </summary>
    public bool MustChangePassword { get; set; } = false;

    /// <summary>
    /// Two-factor authentication enabled status
    /// </summary>
    public bool TwoFactorEnabled { get; set; } = false;

    /// <summary>
    /// Two-factor authentication secret key (for TOTP)
    /// Encrypted/hashed before storage
    /// </summary>
    public string? TwoFactorSecret { get; set; }

    /// <summary>
    /// For Landlord role - properties they own
    /// </summary>
    public ICollection<Property> OwnedProperties { get; set; } = new List<Property>();

    /// <summary>
    /// For Landlord role - payment accounts they own
    /// </summary>
    public ICollection<LandlordPaymentAccount> PaymentAccounts { get; set; } = new List<LandlordPaymentAccount>();

    /// <summary>
    /// Scoped property assignments for Manager/Accountant/Caretaker
    /// </summary>
    public ICollection<UserPropertyAssignment> PropertyAssignments { get; set; } = new List<UserPropertyAssignment>();

    /// <summary>
    /// Full name for display
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
}

