namespace RentCollection.Domain.Enums
{
    public enum ReminderStatus
    {
        Scheduled = 1,
        Sent = 2,
        Failed = 3,
        Cancelled = 4,
        Skipped = 5  // Payment made before reminder sent
    }
}
