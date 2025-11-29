namespace RentCollection.Domain.Enums;

/// <summary>
/// User account status
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// Account is active and can log in
    /// </summary>
    Active = 1,

    /// <summary>
    /// Account is temporarily suspended
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// Account is inactive (e.g., employee left)
    /// </summary>
    Inactive = 3
}
