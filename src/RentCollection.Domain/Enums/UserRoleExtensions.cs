namespace RentCollection.Domain.Enums;

/// <summary>
/// Extension methods and constants for UserRole enum
/// </summary>
public static class UserRoleExtensions
{
    // String constants for role names (used in claims and comparisons)
    public const string PlatformAdmin = "PlatformAdmin";
    public const string Landlord = "Landlord";
    public const string Caretaker = "Caretaker";
    public const string Manager = "Manager";
    public const string Accountant = "Accountant";
    public const string Tenant = "Tenant";

    public static readonly string[] All = { PlatformAdmin, Landlord, Caretaker, Manager, Accountant, Tenant };

    /// <summary>
    /// Convert UserRole enum to string
    /// </summary>
    public static string ToRoleString(this UserRole role)
    {
        return role switch
        {
            UserRole.PlatformAdmin => PlatformAdmin,
            UserRole.Landlord => Landlord,
            UserRole.Caretaker => Caretaker,
            UserRole.Manager => Manager,
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
            PlatformAdmin => UserRole.PlatformAdmin,
            Landlord => UserRole.Landlord,
            Caretaker => UserRole.Caretaker,
            Manager => UserRole.Manager,
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
            PlatformAdmin => UserRole.PlatformAdmin,
            Landlord => UserRole.Landlord,
            Caretaker => UserRole.Caretaker,
            Manager => UserRole.Manager,
            Accountant => UserRole.Accountant,
            Tenant => UserRole.Tenant,
            _ => default
        };

        return role is PlatformAdmin or Landlord or Caretaker or Manager or Accountant or Tenant;
    }
}
