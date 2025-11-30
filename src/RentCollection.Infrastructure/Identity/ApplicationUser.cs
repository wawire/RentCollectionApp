using Microsoft.AspNetCore.Identity;

namespace RentCollection.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = UserRoles.Caretaker;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // For data isolation - Caretakers and Accountants belong to a Landlord
    public string? LandlordId { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}

public static class UserRoles
{
    public const string SystemAdmin = "SystemAdmin";
    public const string Landlord = "Landlord";
    public const string Caretaker = "Caretaker";
    public const string Accountant = "Accountant";
    public const string Tenant = "Tenant";

    public static readonly string[] All = { SystemAdmin, Landlord, Caretaker, Accountant, Tenant };
}
