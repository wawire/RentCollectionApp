namespace RentCollection.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a tenant in the system
/// </summary>
public enum TenantStatus
{
    /// <summary>
    /// Tenant has applied but not yet approved by landlord
    /// </summary>
    Prospective = 0,

    /// <summary>
    /// Tenant has been approved and is actively renting
    /// </summary>
    Active = 1,

    /// <summary>
    /// Tenant lease has ended or been terminated
    /// </summary>
    Inactive = 2
}
