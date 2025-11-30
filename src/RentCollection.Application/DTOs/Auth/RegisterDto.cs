using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Auth;

/// <summary>
/// User registration DTO
/// </summary>
public class RegisterDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Kenyan phone number format: 0712345678 or +254712345678
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    /// <summary>
    /// For Landlords and Caretakers - assign to specific property
    /// </summary>
    public int? PropertyId { get; set; }

    /// <summary>
    /// For Tenant role - link to existing tenant record
    /// </summary>
    public int? TenantId { get; set; }
}
