using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Auth;

/// <summary>
/// Authentication response with JWT token
/// </summary>
public class AuthResponseDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public VerificationChannel? VerificationChannel { get; set; }
    public bool MustChangePassword { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int OrganizationId { get; set; }
    public OrganizationStatus? OrganizationStatus { get; set; }

    /// <summary>
    /// Property ID for scoped users (Landlord, Caretaker)
    /// </summary>
    public int? PropertyId { get; set; }

    /// <summary>
    /// Tenant ID for tenant users
    /// </summary>
    public int? TenantId { get; set; }
}
