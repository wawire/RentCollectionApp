namespace RentCollection.Domain.Enums
{
    /// <summary>
    /// Status of a maintenance request
    /// </summary>
    public enum MaintenanceRequestStatus
    {
        Pending = 0,
        Assigned = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4
    }
}
