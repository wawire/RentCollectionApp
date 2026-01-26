using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.Application.Services;

public class InvoiceGenerationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InvoiceGenerationBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(12);

    public InvoiceGenerationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<InvoiceGenerationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Invoice generation service started at {Time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();

                var today = DateTime.UtcNow.Date;
                await invoiceService.GenerateMonthlyInvoicesAsync(today.Year, today.Month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running invoice generation");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Invoice generation service stopped at {Time}", DateTimeOffset.Now);
    }
}
