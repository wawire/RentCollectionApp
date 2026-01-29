using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Services;

public class UtilityBillingService : IUtilityBillingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UtilityBillingService> _logger;

    public UtilityBillingService(ApplicationDbContext context, ILogger<UtilityBillingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<InvoiceLineItem>> BuildLineItemsForTenantAsync(
        Tenant tenant,
        DateTime periodStart,
        DateTime periodEnd)
    {
        var configs = await _context.UtilityConfigs
            .Include(c => c.UtilityType)
            .Where(c =>
                c.IsActive &&
                c.PropertyId == tenant.Unit.PropertyId &&
                (c.UnitId == null || c.UnitId == tenant.UnitId) &&
                c.EffectiveFrom <= periodEnd &&
                (c.EffectiveTo == null || c.EffectiveTo >= periodStart))
            .ToListAsync();

        var lineItems = new List<InvoiceLineItem>();
        if (configs.Count == 0)
        {
            return lineItems;
        }

        var activeTenantCount = 0;
        var sharedConfigs = configs.Any(c => c.BillingMode == UtilityBillingMode.Shared);
        if (sharedConfigs)
        {
            activeTenantCount = await _context.Tenants
                .Where(t => t.Unit.PropertyId == tenant.Unit.PropertyId && t.Status == TenantStatus.Active)
                .CountAsync();
        }

        foreach (var config in configs)
        {
            var utility = config.UtilityType;
            if (utility == null)
            {
                continue;
            }

            switch (config.BillingMode)
            {
                case UtilityBillingMode.Fixed:
                    if (!config.FixedAmount.HasValue || config.FixedAmount.Value <= 0)
                    {
                        continue;
                    }

                    lineItems.Add(new InvoiceLineItem
                    {
                        LineItemType = InvoiceLineItemType.Utility,
                        UtilityTypeId = utility.Id,
                        UtilityType = utility,
                        UtilityConfigId = config.Id,
                        UtilityConfig = config,
                        Description = $"{utility.Name} (Fixed)",
                        Quantity = 1,
                        Rate = config.FixedAmount.Value,
                        Amount = config.FixedAmount.Value,
                        UnitOfMeasure = utility.UnitOfMeasure
                    });
                    break;

                case UtilityBillingMode.Shared:
                    if (!config.SharedAmount.HasValue || config.SharedAmount.Value <= 0 || activeTenantCount <= 0)
                    {
                        continue;
                    }

                    var splitAmount = decimal.Round(config.SharedAmount.Value / activeTenantCount, 2);
                    lineItems.Add(new InvoiceLineItem
                    {
                        LineItemType = InvoiceLineItemType.Utility,
                        UtilityTypeId = utility.Id,
                        UtilityType = utility,
                        UtilityConfigId = config.Id,
                        UtilityConfig = config,
                        Description = $"{utility.Name} (Shared)",
                        Quantity = 1,
                        Rate = splitAmount,
                        Amount = splitAmount,
                        UnitOfMeasure = utility.UnitOfMeasure
                    });
                    break;

                case UtilityBillingMode.Metered:
                    if (!config.Rate.HasValue || config.Rate.Value <= 0)
                    {
                        continue;
                    }

                    var currentReading = await _context.MeterReadings
                        .Where(r => r.UtilityConfigId == config.Id &&
                                    r.UnitId == tenant.UnitId &&
                                    r.ReadingDate <= periodEnd)
                        .OrderByDescending(r => r.ReadingDate)
                        .FirstOrDefaultAsync();

                    if (currentReading == null || currentReading.ReadingDate < periodStart)
                    {
                        continue;
                    }

                    var previousReading = await _context.MeterReadings
                        .Where(r => r.UtilityConfigId == config.Id &&
                                    r.UnitId == tenant.UnitId &&
                                    r.ReadingDate < currentReading.ReadingDate)
                        .OrderByDescending(r => r.ReadingDate)
                        .FirstOrDefaultAsync();

                    if (previousReading == null)
                    {
                        continue;
                    }

                    var consumption = currentReading.ReadingValue - previousReading.ReadingValue;
                    if (consumption <= 0)
                    {
                        continue;
                    }

                    var amount = decimal.Round(consumption * config.Rate.Value, 2);
                    lineItems.Add(new InvoiceLineItem
                    {
                        LineItemType = InvoiceLineItemType.Utility,
                        UtilityTypeId = utility.Id,
                        UtilityType = utility,
                        UtilityConfigId = config.Id,
                        UtilityConfig = config,
                        Description = $"{utility.Name} (Metered)",
                        Quantity = consumption,
                        Rate = config.Rate.Value,
                        Amount = amount,
                        UnitOfMeasure = utility.UnitOfMeasure
                    });
                    break;
            }
        }

        if (lineItems.Count == 0)
        {
            _logger.LogInformation("No utility line items generated for tenant {TenantId}", tenant.Id);
        }

        return lineItems;
    }
}
