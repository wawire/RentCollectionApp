namespace RentCollection.Domain.Enums;

/// <summary>
/// User account status
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// Account is invited but not yet fully onboarded
    /// </summary>
    Invited = 0,

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
