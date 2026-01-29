using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.Application.Services;

public class MpesaStkReconciliationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MpesaStkReconciliationBackgroundService> _logger;
    private readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(10);
    private readonly TimeSpan _minAge = TimeSpan.FromMinutes(2);
    private const int BatchSize = 50;

    public MpesaStkReconciliationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<MpesaStkReconciliationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("M-Pesa STK reconciliation service started at {Time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var transactionService = scope.ServiceProvider.GetRequiredService<IMPesaTransactionService>();

                var cutoff = DateTime.UtcNow.Subtract(_minAge);
                var checkoutIds = await transactionService.GetPendingStkPushCheckoutRequestIdsAsync(cutoff, BatchSize);

                if (checkoutIds.Any())
                {
                    _logger.LogInformation("Reconciling {Count} pending STK transactions", checkoutIds.Count);

                    foreach (var checkoutId in checkoutIds)
                    {
                        if (stoppingToken.IsCancellationRequested)
                        {
                            break;
                        }

                        try
                        {
                            await transactionService.QueryAndUpdateStkPushStatusAsync(checkoutId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to reconcile STK transaction {CheckoutRequestId}", checkoutId);
                        }

                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in STK reconciliation loop");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }

        _logger.LogInformation("M-Pesa STK reconciliation service stopped at {Time}", DateTimeOffset.Now);
    }
}
