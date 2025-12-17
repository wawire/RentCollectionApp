using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities
{
    public class ReminderSettings
    {
        public int Id { get; set; }
        public int LandlordId { get; set; }

        // Global settings
        public bool IsEnabled { get; set; } = true;
        public ReminderChannel DefaultChannel { get; set; } = ReminderChannel.Both;

        // Reminder schedule (enabled/disabled per reminder type)
        public bool SevenDaysBeforeEnabled { get; set; } = true;
        public bool ThreeDaysBeforeEnabled { get; set; } = true;
        public bool OneDayBeforeEnabled { get; set; } = true;
        public bool OnDueDateEnabled { get; set; } = true;
        public bool OneDayOverdueEnabled { get; set; } = true;
        public bool ThreeDaysOverdueEnabled { get; set; } = false;
        public bool SevenDaysOverdueEnabled { get; set; } = false;

        // Customizable message templates
        public string? SevenDaysBeforeTemplate { get; set; }
        public string? ThreeDaysBeforeTemplate { get; set; }
        public string? OneDayBeforeTemplate { get; set; }
        public string? OnDueDateTemplate { get; set; }
        public string? OneDayOverdueTemplate { get; set; }
        public string? ThreeDaysOverdueTemplate { get; set; }
        public string? SevenDaysOverdueTemplate { get; set; }

        // Quiet hours (don't send during these times)
        public TimeSpan? QuietHoursStart { get; set; } = new TimeSpan(22, 0, 0); // 10 PM
        public TimeSpan? QuietHoursEnd { get; set; } = new TimeSpan(8, 0, 0);   // 8 AM

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public User Landlord { get; set; } = null!;
    }
}
