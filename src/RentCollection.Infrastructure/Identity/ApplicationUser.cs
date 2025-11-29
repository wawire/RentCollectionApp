using Microsoft.AspNetCore.Identity;

namespace RentCollection.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = UserRoles.Viewer;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string PropertyManager = "PropertyManager";
    public const string Viewer = "Viewer";

    public static readonly string[] All = { Admin, PropertyManager, Viewer };
}
