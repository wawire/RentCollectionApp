using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Auth;

/// <summary>
/// User information DTO
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public VerificationChannel? VerificationChannel { get; set; }
    public bool MustChangePassword { get; set; }
    public int? PropertyId { get; set; }
    public string? PropertyName { get; set; }
    public int? TenantId { get; set; }
    public int OrganizationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
