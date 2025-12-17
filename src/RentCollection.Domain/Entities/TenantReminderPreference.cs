using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities
{
    public class TenantReminderPreference
    {
        public int Id { get; set; }
        public int TenantId { get; set; }

        // Opt-in/opt-out
        public bool RemindersEnabled { get; set; } = true;
        public ReminderChannel PreferredChannel { get; set; } = ReminderChannel.Both;

        // Specific reminder preferences
        public bool OptOutSevenDaysBefore { get; set; } = false;
        public bool OptOutThreeDaysBefore { get; set; } = false;
        public bool OptOutOneDayBefore { get; set; } = false;
        public bool OptOutOnDueDate { get; set; } = false;
        public bool OptOutOverdueReminders { get; set; } = false;

        // Contact details override (if different from tenant profile)
        public string? AlternatePhoneNumber { get; set; }
        public string? AlternateEmail { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Tenant Tenant { get; set; } = null!;
    }
}
