namespace RentCollection.Domain.Enums;

/// <summary>
/// User roles specific to Kenyan property management context
/// </summary>
public enum UserRole
{
    /// <summary>
    /// System administrator with full access
    /// </summary>
    SystemAdmin = 1,

    /// <summary>
    /// Property owner with full access to their properties
    /// </summary>
    Landlord = 2,

    /// <summary>
    /// On-ground property manager handling day-to-day operations (Bwana/Bi Caretaker)
    /// </summary>
    Caretaker = 3,

    /// <summary>
    /// Financial accountant/bookkeeper with read-only financial access
    /// </summary>
    Accountant = 4,

    /// <summary>
    /// Property tenant with self-service portal access
    /// </summary>
    Tenant = 5
}
