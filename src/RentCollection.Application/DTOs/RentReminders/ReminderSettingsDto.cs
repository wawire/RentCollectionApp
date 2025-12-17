using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.RentReminders
{
    public class ReminderSettingsDto
    {
        public int Id { get; set; }
        public int LandlordId { get; set; }
        public bool IsEnabled { get; set; }
        public ReminderChannel DefaultChannel { get; set; }

        // Reminder schedule
        public bool SevenDaysBeforeEnabled { get; set; }
        public bool ThreeDaysBeforeEnabled { get; set; }
        public bool OneDayBeforeEnabled { get; set; }
        public bool OnDueDateEnabled { get; set; }
        public bool OneDayOverdueEnabled { get; set; }
        public bool ThreeDaysOverdueEnabled { get; set; }
        public bool SevenDaysOverdueEnabled { get; set; }

        // Message templates
        public string? SevenDaysBeforeTemplate { get; set; }
        public string? ThreeDaysBeforeTemplate { get; set; }
        public string? OneDayBeforeTemplate { get; set; }
        public string? OnDueDateTemplate { get; set; }
        public string? OneDayOverdueTemplate { get; set; }
        public string? ThreeDaysOverdueTemplate { get; set; }
        public string? SevenDaysOverdueTemplate { get; set; }

        // Quiet hours
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
    }

    public class UpdateReminderSettingsDto
    {
        public bool IsEnabled { get; set; }
        public ReminderChannel DefaultChannel { get; set; }

        public bool SevenDaysBeforeEnabled { get; set; }
        public bool ThreeDaysBeforeEnabled { get; set; }
        public bool OneDayBeforeEnabled { get; set; }
        public bool OnDueDateEnabled { get; set; }
        public bool OneDayOverdueEnabled { get; set; }
        public bool ThreeDaysOverdueEnabled { get; set; }
        public bool SevenDaysOverdueEnabled { get; set; }

        public string? SevenDaysBeforeTemplate { get; set; }
        public string? ThreeDaysBeforeTemplate { get; set; }
        public string? OneDayBeforeTemplate { get; set; }
        public string? OnDueDateTemplate { get; set; }
        public string? OneDayOverdueTemplate { get; set; }
        public string? ThreeDaysOverdueTemplate { get; set; }
        public string? SevenDaysOverdueTemplate { get; set; }

        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
    }
}
