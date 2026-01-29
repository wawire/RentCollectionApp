using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Invoices;
using RentCollection.Application.Helpers;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUtilityBillingService _utilityBillingService;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUtilityBillingService utilityBillingService,
        ILogger<InvoiceService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _utilityBillingService = utilityBillingService;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<InvoiceDto>>> GetInvoicesAsync(int? propertyId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.Invoices
                .Include(i => i.Tenant)
                .Include(i => i.Unit)
                .Include(i => i.Property)
                .Include(i => i.LineItems)
                    .ThenInclude(li => li.UtilityType)
                .AsQueryable();

            query = await ApplyRbacFilterAsync(query);

            if (propertyId.HasValue)
            {
                query = query.Where(i => i.PropertyId == propertyId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(i => i.PeriodStart >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(i => i.PeriodEnd <= endDate.Value);
            }

            var invoices = await query
                .OrderByDescending(i => i.PeriodStart)
                .ToListAsync();

            var dtos = invoices.Select(MapToDto);
            return Result<IEnumerable<InvoiceDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices");
            return Result<IEnumerable<InvoiceDto>>.Failure("An error occurred while retrieving invoices");
        }
    }

    public async Task<Result<IEnumerable<InvoiceDto>>> GetInvoicesByTenantAsync(int tenantId)
    {
        try
        {
            await EnsureTenantAccessAsync(tenantId);

            var invoices = await _context.Invoices
                .Include(i => i.Tenant)
                .Include(i => i.Unit)
                .Include(i => i.Property)
                .Include(i => i.LineItems)
                    .ThenInclude(li => li.UtilityType)
                .Where(i => i.TenantId == tenantId)
                .OrderByDescending(i => i.PeriodStart)
                .ToListAsync();

            return Result<IEnumerable<InvoiceDto>>.Success(invoices.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices for tenant {TenantId}", tenantId);
            return Result<IEnumerable<InvoiceDto>>.Failure("An error occurred while retrieving tenant invoices");
        }
    }

    public async Task<Result<InvoiceDto>> GetInvoiceByIdAsync(int id)
    {
        try
        {
            var invoice = await _context.Invoices
                .Include(i => i.Tenant)
                .Include(i => i.Unit)
                .Include(i => i.Property)
                .Include(i => i.LineItems)
                    .ThenInclude(li => li.UtilityType)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return Result<InvoiceDto>.Failure($"Invoice with ID {id} not found");
            }

            var filtered = await ApplyRbacFilterAsync(_context.Invoices.Where(i => i.Id == id));
            if (!await filtered.AnyAsync())
            {
                return Result<InvoiceDto>.Failure("You do not have permission to view this invoice");
            }

            return Result<InvoiceDto>.Success(MapToDto(invoice));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice {InvoiceId}", id);
            return Result<InvoiceDto>.Failure("An error occurred while retrieving the invoice");
        }
    }

    public async Task<Result<GenerateInvoicesResultDto>> GenerateMonthlyInvoicesAsync(int year, int month)
    {
        try
        {
            var periodStart = new DateTime(year, month, 1);
            var periodEnd = periodStart.AddMonths(1).AddDays(-1);

            var tenants = await _context.Tenants
                .Include(t => t.Unit)
                    .ThenInclude(u => u.Property)
                .Where(t => t.Status == TenantStatus.Active)
                .ToListAsync();

            var generated = 0;
            var skipped = 0;

            foreach (var tenant in tenants)
            {
                if (tenant.Unit == null || tenant.Unit.Property == null || !tenant.Unit.Property.LandlordId.HasValue)
                {
                    _logger.LogWarning(
                        "Skipping invoice generation for tenant {TenantId} due to missing unit/property/landlord",
                        tenant.Id);
                    skipped++;
                    continue;
                }

                var exists = await _context.Invoices.AnyAsync(i =>
                    i.TenantId == tenant.Id &&
                    i.PeriodStart == periodStart &&
                    i.PeriodEnd == periodEnd);

                if (exists)
                {
                    skipped++;
                    continue;
                }

                var previousInvoices = await _context.Invoices
                    .Include(i => i.Allocations)
                    .Where(i => i.TenantId == tenant.Id && i.PeriodEnd < periodStart && i.Status != InvoiceStatus.Void)
                    .ToListAsync();

                var openingBalance = previousInvoices.Sum(i =>
                {
                    var allocated = i.Allocations.Sum(a => a.Amount);
                    return InvoiceStatusCalculator.CalculateBalance(i, allocated);
                });

                var dueDate = PaymentDueDateHelper.CalculateDueDateForMonth(year, month, tenant.RentDueDay);
                var lineItems = new List<InvoiceLineItem>
                {
                    new()
                    {
                        LineItemType = InvoiceLineItemType.Rent,
                        Description = $"Rent - {periodStart:MMMM yyyy}",
                        Quantity = 1,
                        Rate = tenant.MonthlyRent,
                        Amount = tenant.MonthlyRent
                    }
                };

                var utilityLineItems = await _utilityBillingService.BuildLineItemsForTenantAsync(
                    tenant, periodStart, periodEnd);
                if (utilityLineItems.Count > 0)
                {
                    lineItems.AddRange(utilityLineItems);
                }

                var amount = lineItems.Sum(item => item.Amount);
                var total = amount + openingBalance;

                var invoice = new Invoice
                {
                    TenantId = tenant.Id,
                    UnitId = tenant.UnitId,
                    PropertyId = tenant.Unit.PropertyId,
                    LandlordId = tenant.Unit.Property.LandlordId.Value,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    DueDate = dueDate,
                    Amount = amount,
                    OpeningBalance = openingBalance,
                    Balance = total,
                    Status = InvoiceStatus.Issued,
                    LineItems = lineItems
                };

                InvoiceStatusCalculator.Apply(invoice, 0, DateTime.UtcNow);

                _context.Invoices.Add(invoice);
                generated++;
            }

            if (generated > 0)
            {
                await _context.SaveChangesAsync();
            }

            return Result<GenerateInvoicesResultDto>.Success(new GenerateInvoicesResultDto
            {
                GeneratedCount = generated,
                SkippedCount = skipped
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoices for {Year}-{Month}", year, month);
            return Result<GenerateInvoicesResultDto>.Failure("An error occurred while generating invoices");
        }
    }

    private async Task<IQueryable<Invoice>> ApplyRbacFilterAsync(IQueryable<Invoice> query)
    {
        if (_currentUserService.IsPlatformAdmin)
        {
            return query;
        }

        if (!_currentUserService.OrganizationId.HasValue)
        {
            return query.Where(i => false);
        }

        query = query.Where(i => i.Property.OrganizationId == _currentUserService.OrganizationId.Value);

        if (_currentUserService.IsTenant && _currentUserService.TenantId.HasValue)
        {
            return query.Where(i => i.TenantId == _currentUserService.TenantId.Value);
        }

        if (_currentUserService.IsLandlord)
        {
            var landlordId = _currentUserService.UserIdInt;
            if (landlordId.HasValue)
            {
                return query.Where(i => i.LandlordId == landlordId.Value);
            }
        }

        if (_currentUserService.IsCaretaker && _currentUserService.PropertyId.HasValue)
        {
            return query.Where(i => i.PropertyId == _currentUserService.PropertyId.Value);
        }

        if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
        {
            var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
            if (assignedPropertyIds.Count > 0)
            {
                return query.Where(i => assignedPropertyIds.Contains(i.PropertyId));
            }
        }

        return query.Where(i => false);
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
                throw new UnauthorizedAccessException("You can only access your own invoices");
            }

            return;
        }

        var tenant = await _context.Tenants
            .Include(t => t.Unit)
                .ThenInclude(u => u.Property)
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant with ID {tenantId} not found");
        }

        if (!_currentUserService.OrganizationId.HasValue ||
            tenant.Unit?.Property?.OrganizationId != _currentUserService.OrganizationId.Value)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this tenant");
        }

        if (_currentUserService.IsLandlord)
        {
            var landlordId = _currentUserService.UserIdInt;
            if (!landlordId.HasValue || tenant.Unit.Property.LandlordId != landlordId.Value)
            {
                throw new UnauthorizedAccessException("You do not have permission to access this tenant");
            }

            return;
        }

        if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
        {
            var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
            if (!assignedPropertyIds.Contains(tenant.Unit.PropertyId))
            {
                throw new UnauthorizedAccessException("You do not have permission to access this tenant");
            }
        }
    }

    private static InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            TenantId = invoice.TenantId,
            TenantName = invoice.Tenant.FullName,
            UnitId = invoice.UnitId,
            UnitNumber = invoice.Unit.UnitNumber,
            PropertyId = invoice.PropertyId,
            PropertyName = invoice.Property.Name,
            LandlordId = invoice.LandlordId,
            PeriodStart = invoice.PeriodStart,
            PeriodEnd = invoice.PeriodEnd,
            DueDate = invoice.DueDate,
            Amount = invoice.Amount,
            OpeningBalance = invoice.OpeningBalance,
            Balance = invoice.Balance,
            Status = invoice.Status,
            LineItems = invoice.LineItems.Select(item => new InvoiceLineItemDto
            {
                Id = item.Id,
                LineItemType = item.LineItemType,
                Description = item.Description,
                Quantity = item.Quantity,
                Rate = item.Rate,
                Amount = item.Amount,
                UnitOfMeasure = item.UnitOfMeasure,
                UtilityTypeId = item.UtilityTypeId,
                UtilityName = item.UtilityType?.Name
            }).ToList()
        };
    }
}

