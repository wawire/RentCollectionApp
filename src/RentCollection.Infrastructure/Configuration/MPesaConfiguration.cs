namespace RentCollection.Infrastructure.Configuration;

/// <summary>
/// M-Pesa API configuration settings
/// </summary>
public class MPesaConfiguration
{
    public const string SectionName = "MPesa";

    /// <summary>
    /// Use sandbox environment (true) or production (false)
    /// </summary>
    public bool UseSandbox { get; set; } = true;

    /// <summary>
    /// Base URL for callback endpoints (e.g., "https://yourdomain.com")
    /// </summary>
    public string CallbackBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Timeout for STK Push (in seconds, default 60)
    /// </summary>
    public int StkPushTimeout { get; set; } = 60;

    /// <summary>
    /// Enable request/response logging for debugging
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;
}
