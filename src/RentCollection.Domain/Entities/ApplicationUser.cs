using Microsoft.AspNetCore.Identity;

namespace RentCollection.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation property - if this is a tenant user
    public int? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
