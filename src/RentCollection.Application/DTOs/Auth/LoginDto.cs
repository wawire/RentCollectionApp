namespace RentCollection.Application.DTOs.Auth;

/// <summary>
/// Login request DTO
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Email or phone number (Kenyan format: 0712345678 or +254712345678)
    /// </summary>
    public string EmailOrPhone { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
