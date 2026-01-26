using Microsoft.Extensions.Logging;
using RentCollection.Application.DTOs.RentReminders;
using RentCollection.Application.DTOs.Sms;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Services
{
    public class RentReminderService : IRentReminderService
    {
        private readonly IRentReminderRepository _reminderRepository;
        private readonly IReminderSettingsRepository _settingsRepository;
        private readonly ITenantReminderPreferenceRepository _preferenceRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ISmsService _smsService;
        private readonly MessageTemplateService _templateService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<RentReminderService> _logger;

        public RentReminderService(
            IRentReminderRepository reminderRepository,
            IReminderSettingsRepository settingsRepository,
            ITenantReminderPreferenceRepository preferenceRepository,
            ITenantRepository tenantRepository,
            IPaymentRepository paymentRepository,
            ISmsService smsService,
            MessageTemplateService templateService,
            ICurrentUserService currentUserService,
            ILogger<RentReminderService> logger)
        {
            _reminderRepository = reminderRepository;
            _settingsRepository = settingsRepository;
            _preferenceRepository = preferenceRepository;
            _tenantRepository = tenantRepository;
            _paymentRepository = paymentRepository;
            _smsService = smsService;
            _templateService = templateService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        #region Settings Management

        public async Task<ReminderSettingsDto> GetReminderSettingsAsync(int landlordId)
        {
            var settings = await _settingsRepository.GetByLandlordIdAsync(landlordId);

            if (settings == null)
            {
                return await GetOrCreateDefaultSettingsAsync(landlordId);
            }

            return MapToDto(settings);
        }

        public async Task<ReminderSettingsDto> GetOrCreateDefaultSettingsAsync(int landlordId)
        {
            var existingSettings = await _settingsRepository.GetByLandlordIdAsync(landlordId);

            if (existingSettings != null)
            {
                return MapToDto(existingSettings);
            }

            // Create default settings
            var newSettings = new ReminderSettings
            {
                LandlordId = landlordId,
                IsEnabled = true,
                DefaultChannel = ReminderChannel.SMS,
                SevenDaysBeforeEnabled = true,
                ThreeDaysBeforeEnabled = true,
                OneDayBeforeEnabled = true,
                OnDueDateEnabled = true,
                OneDayOverdueEnabled = true,
                ThreeDaysOverdueEnabled = false,
                SevenDaysOverdueEnabled = false,
                SevenDaysBeforeTemplate = _templateService.GetDefaultTemplate("7DaysBefore"),
                ThreeDaysBeforeTemplate = _templateService.GetDefaultTemplate("3DaysBefore"),
                OneDayBeforeTemplate = _templateService.GetDefaultTemplate("1DayBefore"),
                OnDueDateTemplate = _templateService.GetDefaultTemplate("OnDueDate"),
                OneDayOverdueTemplate = _templateService.GetDefaultTemplate("1DayOverdue"),
                ThreeDaysOverdueTemplate = _templateService.GetDefaultTemplate("3DaysOverdue"),
                SevenDaysOverdueTemplate = _templateService.GetDefaultTemplate("7DaysOverdue"),
                QuietHoursStart = new TimeSpan(22, 0, 0),
                QuietHoursEnd = new TimeSpan(8, 0, 0)
            };

            await _settingsRepository.AddAsync(newSettings);

            _logger.LogInformation("Created default reminder settings for landlord {LandlordId}", landlordId);

            return MapToDto(newSettings);
        }

        public async Task<ReminderSettingsDto> UpdateReminderSettingsAsync(int landlordId, UpdateReminderSettingsDto dto)
        {
            var settings = await _settingsRepository.GetByLandlordIdAsync(landlordId);

            if (settings == null)
            {
                settings = new ReminderSettings { LandlordId = landlordId };
                await _settingsRepository.AddAsync(settings);
            }

            // Update settings
            settings.IsEnabled = dto.IsEnabled;
            settings.DefaultChannel = dto.DefaultChannel;
            settings.SevenDaysBeforeEnabled = dto.SevenDaysBeforeEnabled;
            settings.ThreeDaysBeforeEnabled = dto.ThreeDaysBeforeEnabled;
            settings.OneDayBeforeEnabled = dto.OneDayBeforeEnabled;
            settings.OnDueDateEnabled = dto.OnDueDateEnabled;
            settings.OneDayOverdueEnabled = dto.OneDayOverdueEnabled;
            settings.ThreeDaysOverdueEnabled = dto.ThreeDaysOverdueEnabled;
            settings.SevenDaysOverdueEnabled = dto.SevenDaysOverdueEnabled;

            // Update templates if provided
            if (!string.IsNullOrWhiteSpace(dto.SevenDaysBeforeTemplate))
                settings.SevenDaysBeforeTemplate = dto.SevenDaysBeforeTemplate;
            if (!string.IsNullOrWhiteSpace(dto.ThreeDaysBeforeTemplate))
                settings.ThreeDaysBeforeTemplate = dto.ThreeDaysBeforeTemplate;
            if (!string.IsNullOrWhiteSpace(dto.OneDayBeforeTemplate))
                settings.OneDayBeforeTemplate = dto.OneDayBeforeTemplate;
            if (!string.IsNullOrWhiteSpace(dto.OnDueDateTemplate))
                settings.OnDueDateTemplate = dto.OnDueDateTemplate;
            if (!string.IsNullOrWhiteSpace(dto.OneDayOverdueTemplate))
                settings.OneDayOverdueTemplate = dto.OneDayOverdueTemplate;
            if (!string.IsNullOrWhiteSpace(dto.ThreeDaysOverdueTemplate))
                settings.ThreeDaysOverdueTemplate = dto.ThreeDaysOverdueTemplate;
            if (!string.IsNullOrWhiteSpace(dto.SevenDaysOverdueTemplate))
                settings.SevenDaysOverdueTemplate = dto.SevenDaysOverdueTemplate;

            settings.QuietHoursStart = dto.QuietHoursStart;
            settings.QuietHoursEnd = dto.QuietHoursEnd;
            settings.UpdatedAt = DateTime.UtcNow;

            await _settingsRepository.UpdateAsync(settings);

            _logger.LogInformation("Updated reminder settings for landlord {LandlordId}", landlordId);

            return MapToDto(settings);
        }

        #endregion

        #region Reminder Scheduling

        public async Task ScheduleRemindersForAllTenantsAsync()
        {
            _logger.LogInformation("Starting reminder scheduling for all tenants");

            // Get all active tenants with full details
            var tenants = await _tenantRepository.GetActiveTenantsWithFullDetailsAsync();

            _logger.LogInformation("Found {Count} active tenants to process", tenants.Count());

            int scheduledCount = 0;
            foreach (var tenant in tenants)
            {
                try
                {
                    await ScheduleRemindersForTenantAsync(tenant.Id);
                    scheduledCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error scheduling reminders for tenant {TenantId}", tenant.Id);
                }
            }

            _logger.LogInformation("Scheduled reminders for {Count}/{Total} tenants", scheduledCount, tenants.Count());
        }

        public async Task ScheduleRemindersForTenantAsync(int tenantId)
        {
            var tenant = await _tenantRepository.GetTenantWithDetailsAsync(tenantId);

            if (tenant == null)
            {
                _logger.LogWarning("Tenant {TenantId} not found", tenantId);
                return;
            }

            if (tenant.Status != TenantStatus.Active)
            {
                _logger.LogDebug("Tenant {TenantId} is not active, skipping reminders", tenantId);
                return;
            }

            // Get landlord settings
            var landlordId = tenant.Unit?.Property?.LandlordId ?? 0;
            if (landlordId == 0)
            {
                _logger.LogWarning("Cannot determine landlord ID for tenant {TenantId}", tenantId);
                return;
            }

            var settings = await GetOrCreateDefaultSettingsAsync(landlordId);

            if (!settings.IsEnabled)
            {
                _logger.LogDebug("Reminders disabled for landlord {LandlordId}, skipping", landlordId);
                return;
            }

            // Get tenant preferences
            var preferences = await _preferenceRepository.GetByTenantIdAsync(tenantId);

            if (preferences != null && !preferences.RemindersEnabled)
            {
                _logger.LogDebug("Reminders disabled for tenant {TenantId}, skipping", tenantId);
                return;
            }

            // Calculate next rent due date
            var nextDueDate = CalculateNextRentDueDate(tenant);
            var today = DateTime.Today;

            // Cancel any existing scheduled reminders for this rent cycle
            var existingReminders = await _reminderRepository.GetScheduledRemindersByTenantIdAsync(tenantId, nextDueDate);

            foreach (var existing in existingReminders)
            {
                existing.Status = ReminderStatus.Cancelled;
                await _reminderRepository.UpdateAsync(existing);
            }

            // Schedule reminders based on settings
            var remindersToSchedule = new List<(ReminderType Type, int DaysOffset, bool Enabled, string? Template)>
            {
                (ReminderType.SevenDaysBefore, -7, settings.SevenDaysBeforeEnabled, settings.SevenDaysBeforeTemplate),
                (ReminderType.ThreeDaysBefore, -3, settings.ThreeDaysBeforeEnabled, settings.ThreeDaysBeforeTemplate),
                (ReminderType.OneDayBefore, -1, settings.OneDayBeforeEnabled, settings.OneDayBeforeTemplate),
                (ReminderType.OnDueDate, 0, settings.OnDueDateEnabled, settings.OnDueDateTemplate),
                (ReminderType.OneDayOverdue, 1, settings.OneDayOverdueEnabled, settings.OneDayOverdueTemplate),
                (ReminderType.ThreeDaysOverdue, 3, settings.ThreeDaysOverdueEnabled, settings.ThreeDaysOverdueTemplate),
                (ReminderType.SevenDaysOverdue, 7, settings.SevenDaysOverdueEnabled, settings.SevenDaysOverdueTemplate)
            };

            foreach (var (type, daysOffset, enabled, template) in remindersToSchedule)
            {
                if (!enabled) continue;

                var scheduledDate = nextDueDate.AddDays(daysOffset);

                // Only schedule if date is in the future or today
                if (scheduledDate < today) continue;

                // Check if payment already made for this cycle
                var paymentExists = await _paymentRepository.HasConfirmedPaymentAsync(
                    tenantId, nextDueDate.Month, nextDueDate.Year);

                if (paymentExists)
                {
                    _logger.LogDebug("Payment already made for tenant {TenantId} for {Month}/{Year}, skipping reminders",
                        tenantId, nextDueDate.Month, nextDueDate.Year);
                    continue;
                }

                var reminder = new RentReminder
                {
                    TenantId = tenant.Id,
                    LandlordId = landlordId,
                    PropertyId = tenant.Unit?.PropertyId ?? 0,
                    UnitId = tenant.UnitId,
                    ReminderType = type,
                    Channel = preferences?.PreferredChannel ?? settings.DefaultChannel,
                    Status = ReminderStatus.Scheduled,
                    ScheduledDate = scheduledDate,
                    RentDueDate = nextDueDate,
                    RentAmount = tenant.MonthlyRent,
                    MessageTemplate = template ?? _templateService.GetDefaultTemplate(type.ToString())
                };

                await _reminderRepository.AddAsync(reminder);
            }

            _logger.LogInformation("Scheduled reminders for tenant {TenantId} with due date {DueDate}",
                tenantId, nextDueDate);
        }

        #endregion

        #region Reminder Sending

        public async Task SendReminderNowAsync(int reminderId)
        {
            var reminder = await _reminderRepository.GetReminderWithDetailsAsync(reminderId);

            if (reminder == null)
            {
                throw new InvalidOperationException($"Reminder {reminderId} not found");
            }

            await SendReminderAsync(reminder);
        }

        private async Task SendReminderAsync(RentReminder reminder)
        {
            try
            {
                // Check if payment was made (skip reminder if paid)
                var paymentExists = await _paymentRepository.HasConfirmedPaymentAsync(
                    reminder.TenantId, reminder.RentDueDate.Month, reminder.RentDueDate.Year);

                if (paymentExists)
                {
                    reminder.Status = ReminderStatus.Skipped;
                    reminder.UpdatedAt = DateTime.UtcNow;
                    await _reminderRepository.UpdateAsync(reminder);

                    _logger.LogInformation("Reminder {ReminderId} skipped - payment already made", reminder.Id);
                    return;
                }

                // Build message from template
                var variables = _templateService.BuildVariables(
                    tenantName: reminder.Tenant.FullName,
                    tenantPhone: reminder.Tenant.PhoneNumber,
                    landlordName: reminder.Landlord.FullName,
                    landlordPhone: reminder.Landlord.PhoneNumber ?? string.Empty,
                    propertyName: reminder.Property.Name,
                    unitNumber: reminder.Unit.UnitNumber,
                    rentAmount: reminder.RentAmount,
                    dueDate: reminder.RentDueDate,
                    daysUntilDue: (reminder.RentDueDate - DateTime.Today).Days
                );

                var message = _templateService.RenderTemplate(reminder.MessageTemplate, variables);
                reminder.MessageSent = message;

                // Send SMS (Email support can be added later)
                if (reminder.Channel == ReminderChannel.SMS || reminder.Channel == ReminderChannel.Both)
                {
                    var result = await _smsService.SendSmsAsync(new SendSmsDto
                    {
                        PhoneNumber = reminder.Tenant.PhoneNumber,
                        Message = message,
                        TenantId = reminder.TenantId
                    });

                    if (result.IsSuccess)
                    {
                        reminder.Status = ReminderStatus.Sent;
                        reminder.SentDate = DateTime.UtcNow;
                        _logger.LogInformation("Reminder {ReminderId} sent successfully to tenant {TenantId}",
                            reminder.Id, reminder.TenantId);
                    }
                    else
                    {
                        reminder.Status = ReminderStatus.Failed;
                        reminder.FailureReason = result.ErrorMessage;
                        reminder.RetryCount++;
                        reminder.LastRetryDate = DateTime.UtcNow;
                        _logger.LogError("Failed to send reminder {ReminderId}: {Error}",
                            reminder.Id, result.ErrorMessage);
                    }
                }

                reminder.UpdatedAt = DateTime.UtcNow;
                await _reminderRepository.UpdateAsync(reminder);
            }
            catch (Exception ex)
            {
                reminder.Status = ReminderStatus.Failed;
                reminder.FailureReason = ex.Message;
                reminder.RetryCount++;
                reminder.LastRetryDate = DateTime.UtcNow;
                reminder.UpdatedAt = DateTime.UtcNow;

                await _reminderRepository.UpdateAsync(reminder);

                _logger.LogError(ex, "Error sending reminder {ReminderId}", reminder.Id);
                throw;
            }
        }

        #endregion

        #region Query Methods

        public async Task<List<RentReminderDto>> GetRemindersForLandlordAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var reminders = await _reminderRepository.GetRemindersByLandlordIdAsync(landlordId, startDate, endDate);
            return reminders.Select(MapReminderToDto).ToList();
        }

        public async Task<List<RentReminderDto>> GetRemindersForTenantAsync(int tenantId)
        {
            await EnsureTenantAccessAsync(tenantId);
            var reminders = await _reminderRepository.GetRemindersByTenantIdAsync(tenantId);
            return reminders.Select(MapReminderToDto).ToList();
        }

        public async Task<ReminderStatisticsDto> GetReminderStatisticsAsync(int landlordId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var reminders = (await _reminderRepository.GetRemindersByLandlordIdAsync(landlordId, startDate, endDate)).ToList();

            var totalReminders = reminders.Count;
            var sentReminders = reminders.Count(r => r.Status == ReminderStatus.Sent);
            var scheduledReminders = reminders.Count(r => r.Status == ReminderStatus.Scheduled);
            var failedReminders = reminders.Count(r => r.Status == ReminderStatus.Failed);

            var successRate = totalReminders > 0 ? (decimal)sentReminders / totalReminders * 100 : 0;

            var remindersByType = await _reminderRepository.GetRemindersByTypeAsync(landlordId, startDate, endDate);
            var remindersByStatus = await _reminderRepository.GetRemindersByStatusAsync(landlordId, startDate, endDate);

            return new ReminderStatisticsDto
            {
                TotalReminders = totalReminders,
                ScheduledReminders = scheduledReminders,
                SentReminders = sentReminders,
                FailedReminders = failedReminders,
                SuccessRate = successRate,
                RemindersByType = remindersByType,
                RemindersByStatus = remindersByStatus
            };
        }

        #endregion

        #region Tenant Preferences

        public async Task<bool> UpdateTenantPreferencesAsync(int tenantId, bool remindersEnabled, ReminderChannel preferredChannel)
        {
            await EnsureTenantAccessAsync(tenantId);
            var preference = await _preferenceRepository.GetByTenantIdAsync(tenantId);

            if (preference == null)
            {
                preference = new TenantReminderPreference
                {
                    TenantId = tenantId,
                    RemindersEnabled = remindersEnabled,
                    PreferredChannel = preferredChannel
                };
                await _preferenceRepository.AddAsync(preference);
            }
            else
            {
                preference.RemindersEnabled = remindersEnabled;
                preference.PreferredChannel = preferredChannel;
                preference.UpdatedAt = DateTime.UtcNow;
                await _preferenceRepository.UpdateAsync(preference);
            }

            _logger.LogInformation("Updated reminder preferences for tenant {TenantId}", tenantId);

            return true;
        }

        private async Task EnsureTenantAccessAsync(int tenantId)
        {
            if (_currentUserService.IsPlatformAdmin)
            {
                return;
            }

            if (_currentUserService.IsTenant)
            {
                if (!_currentUserService.TenantId.HasValue || _currentUserService.TenantId.Value != tenantId)
                {
                    throw new UnauthorizedAccessException("You can only access your own reminders");
                }

                return;
            }

            var tenant = await _tenantRepository.GetTenantWithDetailsAsync(tenantId);
            if (tenant == null)
            {
                throw new InvalidOperationException($"Tenant with ID {tenantId} not found");
            }

            if (_currentUserService.IsLandlord)
            {
                var landlordId = _currentUserService.UserIdInt;
                if (!landlordId.HasValue || tenant.Unit?.Property?.LandlordId != landlordId.Value)
                {
                    throw new UnauthorizedAccessException("You do not have permission to access this tenant");
                }

                return;
            }

            if (_currentUserService.IsCaretaker)
            {
                var propertyId = _currentUserService.PropertyId;
                if (!propertyId.HasValue || tenant.Unit?.PropertyId != propertyId.Value)
                {
                    throw new UnauthorizedAccessException("You do not have permission to access this tenant");
                }

                return;
            }

            throw new UnauthorizedAccessException("You do not have permission to access tenant reminders");
        }

        public async Task CancelReminderAsync(int reminderId)
        {
            var reminder = await _reminderRepository.GetByIdAsync(reminderId);

            if (reminder == null)
            {
                throw new InvalidOperationException($"Reminder {reminderId} not found");
            }

            reminder.Status = ReminderStatus.Cancelled;
            reminder.UpdatedAt = DateTime.UtcNow;

            await _reminderRepository.UpdateAsync(reminder);

            _logger.LogInformation("Cancelled reminder {ReminderId}", reminderId);
        }

        public async Task<List<RentReminderDto>> GetScheduledRemindersAsync()
        {
            var reminders = await _reminderRepository.GetScheduledRemindersAsync();
            return reminders.Select(MapReminderToDto).ToList();
        }

        #endregion

        #region Helper Methods

        private DateTime CalculateNextRentDueDate(Tenant tenant)
        {
            var today = DateTime.Today;
            var rentDueDay = tenant.RentDueDay;

            // If today is before the due day this month, due date is this month
            if (today.Day < rentDueDay)
            {
                return new DateTime(today.Year, today.Month, Math.Min(rentDueDay, DateTime.DaysInMonth(today.Year, today.Month)));
            }

            // Otherwise, due date is next month
            var nextMonth = today.AddMonths(1);
            return new DateTime(nextMonth.Year, nextMonth.Month, Math.Min(rentDueDay, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month)));
        }

        private ReminderSettingsDto MapToDto(ReminderSettings settings)
        {
            return new ReminderSettingsDto
            {
                Id = settings.Id,
                LandlordId = settings.LandlordId,
                IsEnabled = settings.IsEnabled,
                DefaultChannel = settings.DefaultChannel,
                SevenDaysBeforeEnabled = settings.SevenDaysBeforeEnabled,
                ThreeDaysBeforeEnabled = settings.ThreeDaysBeforeEnabled,
                OneDayBeforeEnabled = settings.OneDayBeforeEnabled,
                OnDueDateEnabled = settings.OnDueDateEnabled,
                OneDayOverdueEnabled = settings.OneDayOverdueEnabled,
                ThreeDaysOverdueEnabled = settings.ThreeDaysOverdueEnabled,
                SevenDaysOverdueEnabled = settings.SevenDaysOverdueEnabled,
                SevenDaysBeforeTemplate = settings.SevenDaysBeforeTemplate,
                ThreeDaysBeforeTemplate = settings.ThreeDaysBeforeTemplate,
                OneDayBeforeTemplate = settings.OneDayBeforeTemplate,
                OnDueDateTemplate = settings.OnDueDateTemplate,
                OneDayOverdueTemplate = settings.OneDayOverdueTemplate,
                ThreeDaysOverdueTemplate = settings.ThreeDaysOverdueTemplate,
                SevenDaysOverdueTemplate = settings.SevenDaysOverdueTemplate,
                QuietHoursStart = settings.QuietHoursStart,
                QuietHoursEnd = settings.QuietHoursEnd
            };
        }

        private RentReminderDto MapReminderToDto(RentReminder reminder)
        {
            return new RentReminderDto
            {
                Id = reminder.Id,
                TenantId = reminder.TenantId,
                TenantName = reminder.Tenant.FullName,
                TenantPhone = reminder.Tenant.PhoneNumber,
                TenantEmail = reminder.Tenant.Email,
                PropertyId = reminder.PropertyId,
                PropertyName = reminder.Property.Name,
                UnitId = reminder.UnitId,
                UnitNumber = reminder.Unit.UnitNumber,
                ReminderType = reminder.ReminderType,
                ReminderTypeDisplay = reminder.ReminderType.ToString(),
                Channel = reminder.Channel,
                ChannelDisplay = reminder.Channel.ToString(),
                Status = reminder.Status,
                StatusDisplay = reminder.Status.ToString(),
                ScheduledDate = reminder.ScheduledDate,
                SentDate = reminder.SentDate,
                RentDueDate = reminder.RentDueDate,
                RentAmount = reminder.RentAmount,
                MessageSent = reminder.MessageSent,
                FailureReason = reminder.FailureReason,
                RetryCount = reminder.RetryCount,
                CreatedAt = reminder.CreatedAt
            };
        }

        #endregion
    }
}

