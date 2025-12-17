using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities
{
    public class RentReminder
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int LandlordId { get; set; }
        public int PropertyId { get; set; }
        public int UnitId { get; set; }

        // Reminder details
        public ReminderType ReminderType { get; set; }
        public ReminderChannel Channel { get; set; }
        public ReminderStatus Status { get; set; }

        // Scheduling
        public DateTime ScheduledDate { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime RentDueDate { get; set; }
        public decimal RentAmount { get; set; }

        // Message details
        public string MessageTemplate { get; set; } = string.Empty;
        public string MessageSent { get; set; } = string.Empty;

        // Delivery tracking
        public string? SmsMessageId { get; set; }
        public string? EmailMessageId { get; set; }
        public string? FailureReason { get; set; }
        public int RetryCount { get; set; } = 0;
        public DateTime? LastRetryDate { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Tenant Tenant { get; set; } = null!;
        public User Landlord { get; set; } = null!;
        public Property Property { get; set; } = null!;
        public Unit Unit { get; set; } = null!;
    }
}
