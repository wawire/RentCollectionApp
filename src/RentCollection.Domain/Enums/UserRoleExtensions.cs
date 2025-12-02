namespace RentCollection.Domain.Enums;

/// <summary>
/// Extension methods and constants for UserRole enum
/// </summary>
public static class UserRoleExtensions
{
    // String constants for role names (used in claims and comparisons)
    public const string SystemAdmin = "SystemAdmin";
    public const string Landlord = "Landlord";
    public const string Caretaker = "Caretaker";
    public const string Accountant = "Accountant";
    public const string Tenant = "Tenant";

    public static readonly string[] All = { SystemAdmin, Landlord, Caretaker, Accountant, Tenant };

    /// <summary>
    /// Convert UserRole enum to string
    /// </summary>
    public static string ToRoleString(this UserRole role)
    {
        return role switch
        {
            UserRole.SystemAdmin => SystemAdmin,
            UserRole.Landlord => Landlord,
            UserRole.Caretaker => Caretaker,
            UserRole.Accountant => Accountant,
            UserRole.Tenant => Tenant,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };
    }

    /// <summary>
    /// Parse string to UserRole enum
    /// </summary>
    public static UserRole ParseRole(string role)
    {
        return role switch
        {
            SystemAdmin => UserRole.SystemAdmin,
            Landlord => UserRole.Landlord,
            Caretaker => UserRole.Caretaker,
            Accountant => UserRole.Accountant,
            Tenant => UserRole.Tenant,
            _ => throw new ArgumentException($"Invalid role: {role}", nameof(role))
        };
    }

    /// <summary>
    /// Try parse string to UserRole enum
    /// </summary>
    public static bool TryParseRole(string role, out UserRole userRole)
    {
        userRole = role switch
        {
            SystemAdmin => UserRole.SystemAdmin,
            Landlord => UserRole.Landlord,
            Caretaker => UserRole.Caretaker,
            Accountant => UserRole.Accountant,
            Tenant => UserRole.Tenant,
            _ => default
        };

        return role is SystemAdmin or Landlord or Caretaker or Accountant or Tenant;
    }
}
