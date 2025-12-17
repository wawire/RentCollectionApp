using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Interfaces;

namespace RentCollection.Application.Services
{
    /// <summary>
    /// Background service that runs daily to schedule and send rent reminders
    /// </summary>
    public class RentReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RentReminderBackgroundService> _logger;
        private readonly TimeSpan _schedulingInterval = TimeSpan.FromHours(1); // Check every hour
        private readonly TimeSpan _sendingInterval = TimeSpan.FromMinutes(5); // Send reminders every 5 minutes

        public RentReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<RentReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Rent Reminder Background Service started at {Time}", DateTimeOffset.Now);

            // Wait for initial delay (start at next hour boundary)
            var initialDelay = GetDelayUntilNextHour();
            await Task.Delay(initialDelay, stoppingToken);

            var lastSchedulingRun = DateTime.MinValue;
            var lastSendingRun = DateTime.MinValue;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;

                    // Run scheduling task once per hour
                    if (now - lastSchedulingRun >= _schedulingInterval)
                    {
                        await ScheduleRemindersAsync(stoppingToken);
                        lastSchedulingRun = now;
                    }

                    // Run sending task every 5 minutes
                    if (now - lastSendingRun >= _sendingInterval)
                    {
                        await ProcessScheduledRemindersAsync(stoppingToken);
                        lastSendingRun = now;
                    }

                    // Wait before next check (1 minute)
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Rent Reminder Background Service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Rent Reminder Background Service main loop");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Rent Reminder Background Service stopped at {Time}", DateTimeOffset.Now);
        }

        /// <summary>
        /// Schedule reminders for all tenants (runs once per hour)
        /// </summary>
        private async Task ScheduleRemindersAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var reminderService = scope.ServiceProvider.GetRequiredService<IRentReminderService>();

            try
            {
                _logger.LogInformation("Starting reminder scheduling at {Time}", DateTime.Now);

                await reminderService.ScheduleRemindersForAllTenantsAsync();

                _logger.LogInformation("Completed reminder scheduling at {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling reminders");
            }
        }

        /// <summary>
        /// Process and send scheduled reminders that are due (runs every 5 minutes)
        /// </summary>
        private async Task ProcessScheduledRemindersAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var reminderService = scope.ServiceProvider.GetRequiredService<IRentReminderService>();

            try
            {
                _logger.LogInformation("Starting to process scheduled reminders at {Time}", DateTime.Now);

                // Get all scheduled reminders that are due
                var reminders = await reminderService.GetScheduledRemindersAsync();

                if (reminders.Any())
                {
                    _logger.LogInformation("Found {Count} reminders to process", reminders.Count);

                    foreach (var reminder in reminders)
                    {
                        if (stoppingToken.IsCancellationRequested)
                            break;

                        try
                        {
                            await reminderService.SendReminderNowAsync(reminder.Id);
                            _logger.LogInformation("Sent reminder {ReminderId} to tenant {TenantName}",
                                reminder.Id, reminder.TenantName);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error sending reminder {ReminderId}", reminder.Id);
                        }

                        // Small delay between reminders to avoid overwhelming SMS/Email services
                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    }

                    _logger.LogInformation("Completed processing {Count} reminders", reminders.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled reminders");
            }
        }

        /// <summary>
        /// Calculate delay until next hour boundary
        /// </summary>
        private TimeSpan GetDelayUntilNextHour()
        {
            var now = DateTime.UtcNow;
            var nextHour = now.Date.AddHours(now.Hour + 1);
            return nextHour - now;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Rent Reminder Background Service is stopping");
            return base.StopAsync(cancellationToken);
        }
    }
}
