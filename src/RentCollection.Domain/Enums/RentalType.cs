namespace RentCollection.Domain.Enums;

/// <summary>
/// Defines the type of rental arrangement for a unit
/// </summary>
public enum RentalType
{
    /// <summary>
    /// Monthly rental with flexible terms
    /// </summary>
    Rent = 1,

    /// <summary>
    /// Long-term lease (typically 6-12 months commitment)
    /// </summary>
    Lease = 2,

    /// <summary>
    /// Available for both rent and lease arrangements
    /// </summary>
    Both = 3
}
