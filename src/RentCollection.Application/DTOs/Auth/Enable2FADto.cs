namespace RentCollection.Application.DTOs.Auth;

/// <summary>
/// DTO for enabling two-factor authentication
/// </summary>
public class Enable2FADto
{
    /// <summary>
    /// The 6-digit code from the authenticator app to verify setup
    /// </summary>
    public string VerificationCode { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO when setting up 2FA
/// Contains QR code data and secret for authenticator apps
/// </summary>
public class Setup2FAResponseDto
{
    /// <summary>
    /// Secret key to be entered manually in authenticator app (if QR code fails)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// QR code URI that can be used to generate QR code
    /// Format: otpauth://totp/{Issuer}:{AccountName}?secret={Secret}&issuer={Issuer}
    /// </summary>
    public string QrCodeUri { get; set; } = string.Empty;

    /// <summary>
    /// Issuer name (app name)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Account name (user's email)
    /// </summary>
    public string AccountName { get; set; } = string.Empty;
}

/// <summary>
/// DTO for verifying 2FA code during login
/// </summary>
public class Verify2FACodeDto
{
    /// <summary>
    /// User's email or phone
    /// </summary>
    public string EmailOrPhone { get; set; } = string.Empty;

    /// <summary>
    /// The 6-digit code from the authenticator app
    /// </summary>
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// DTO for disabling two-factor authentication
/// </summary>
public class Disable2FADto
{
    /// <summary>
    /// Current password for security verification
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
