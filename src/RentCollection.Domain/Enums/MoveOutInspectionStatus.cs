namespace RentCollection.Domain.Enums
{
    public enum MoveOutInspectionStatus
    {
        Scheduled = 1,      // Inspection scheduled but not done
        InProgress = 2,     // Currently being inspected
        Completed = 3,      // Inspection done, calculating deductions
        Reviewed = 4,       // Landlord reviewed and approved deductions
        DisputeRaised = 5,  // Tenant disputed deductions
        Settled = 6,        // Final settlement completed
        RefundProcessed = 7 // Refund sent to tenant
    }

    public enum InspectionCategory
    {
        Walls = 1,
        Floors = 2,
        Ceiling = 3,
        Doors = 4,
        Windows = 5,
        Kitchen = 6,
        Bathroom = 7,
        Fixtures = 8,      // Light fixtures, fans, etc.
        Appliances = 9,
        Plumbing = 10,
        Electrical = 11,
        Locks = 12,
        Keys = 13,
        Yard = 14,
        Other = 99
    }

    public enum PhotoType
    {
        MoveIn = 1,         // Photo taken when tenant moved in
        MoveOut = 2,        // Photo taken during move-out inspection
        Damage = 3,         // Close-up of specific damage
        Before = 4,         // Before repair
        After = 5           // After repair
    }
}
