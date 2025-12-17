using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.RentReminders
{
    public class RentReminderDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string TenantPhone { get; set; } = string.Empty;
        public string TenantEmail { get; set; } = string.Empty;
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public int UnitId { get; set; }
        public string UnitNumber { get; set; } = string.Empty;

        public ReminderType ReminderType { get; set; }
        public string ReminderTypeDisplay { get; set; } = string.Empty;
        public ReminderChannel Channel { get; set; }
        public string ChannelDisplay { get; set; } = string.Empty;
        public ReminderStatus Status { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;

        public DateTime ScheduledDate { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime RentDueDate { get; set; }
        public decimal RentAmount { get; set; }

        public string MessageSent { get; set; } = string.Empty;
        public string? FailureReason { get; set; }
        public int RetryCount { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class ReminderStatisticsDto
    {
        public int TotalReminders { get; set; }
        public int ScheduledReminders { get; set; }
        public int SentReminders { get; set; }
        public int FailedReminders { get; set; }
        public decimal SuccessRate { get; set; }

        public Dictionary<string, int> RemindersByType { get; set; } = new();
        public Dictionary<string, int> RemindersByStatus { get; set; } = new();
    }
}
