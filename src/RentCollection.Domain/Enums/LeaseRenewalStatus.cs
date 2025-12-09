namespace RentCollection.Domain.Enums
{
    /// <summary>
    /// Status of a lease renewal request
    /// </summary>
    public enum LeaseRenewalStatus
    {
        Pending = 0,
        TenantAccepted = 1,
        TenantRejected = 2,
        LandlordApproved = 3,
        LandlordRejected = 4,
        Completed = 5,
        Cancelled = 6
    }
}
