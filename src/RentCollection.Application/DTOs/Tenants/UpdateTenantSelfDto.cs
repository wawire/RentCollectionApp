using System.ComponentModel.DataAnnotations;

namespace RentCollection.Application.DTOs.Tenants;

/// <summary>
/// DTO for tenant self-update (limited fields tenants can modify)
/// </summary>
public class UpdateTenantSelfDto
{
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}
