using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Models;
using RentCollection.Application.Helpers;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Services;

public class PaymentAllocationService : IPaymentAllocationService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PaymentAllocationService> _logger;

    public PaymentAllocationService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<PaymentAllocationService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> AllocatePaymentAsync(int paymentId, int? invoiceId = null, decimal? amount = null)
    {
        try
        {
            var payment = await _context.Payments
                .Include(p => p.Allocations)
                .Include(p => p.Unit)
                    .ThenInclude(u => u.Property)
                .Include(p => p.Tenant)
                    .ThenInclude(t => t.Unit)
                        .ThenInclude(u => u.Property)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return Result.Failure($"Payment with ID {paymentId} not found");
            }

            if (payment.Status != PaymentStatus.Completed)
            {
                return Result.Failure("Only completed payments can be allocated");
            }

            if (!await CanAccessPaymentAsync(payment))
            {
                return Result.Failure("You do not have permission to allocate this payment");
            }

            var remaining = GetRemainingAmount(payment);
            if (remaining <= 0)
            {
                return Result.Success("Payment has no remaining balance to allocate");
            }

            if (invoiceId.HasValue)
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Allocations)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId.Value);

                if (invoice == null)
                {
                    return Result.Failure($"Invoice with ID {invoiceId.Value} not found");
                }

                if (invoice.TenantId != payment.TenantId)
                {
                    return Result.Failure("Invoice tenant does not match payment tenant");
                }

                var amountToAllocate = amount ?? remaining;
                return await AllocateToInvoiceAsync(payment, invoice, amountToAllocate);
            }

            return await AllocateToOutstandingInvoicesAsync(payment, remaining);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error allocating payment {PaymentId}", paymentId);
            return Result.Failure("An error occurred while allocating the payment");
        }
    }

    public async Task<Result> AllocatePaymentToOutstandingInvoicesAsync(int paymentId)
    {
        try
        {
            var payment = await _context.Payments
                .Include(p => p.Allocations)
                .Include(p => p.Unit)
                    .ThenInclude(u => u.Property)
                .Include(p => p.Tenant)
                    .ThenInclude(t => t.Unit)
                        .ThenInclude(u => u.Property)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return Result.Failure($"Payment with ID {paymentId} not found");
            }

            if (payment.Status != PaymentStatus.Completed)
            {
                return Result.Failure("Only completed payments can be allocated");
            }

            var remaining = GetRemainingAmount(payment);
            if (remaining <= 0)
            {
                return Result.Success("Payment has no remaining balance to allocate");
            }

            return await AllocateToOutstandingInvoicesAsync(payment, remaining);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error allocating payment {PaymentId} to outstanding invoices", paymentId);
            return Result.Failure("An error occurred while allocating the payment");
        }
    }

    private async Task<Result> AllocateToOutstandingInvoicesAsync(Payment payment, decimal remaining)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Allocations)
            .Where(i => i.TenantId == payment.TenantId &&
                        i.Status != InvoiceStatus.Void &&
                        i.Balance > 0)
            .OrderBy(i => i.DueDate)
            .ThenBy(i => i.PeriodStart)
            .ToListAsync();

        foreach (var invoice in invoices)
        {
            if (remaining <= 0)
            {
                break;
            }

            var invoiceBalance = GetInvoiceBalance(invoice);
            if (invoiceBalance <= 0)
            {
                continue;
            }

            var allocationAmount = Math.Min(remaining, invoiceBalance);
            await AllocateToInvoiceAsync(payment, invoice, allocationAmount, saveChanges: false);
            remaining -= allocationAmount;
        }

        payment.UnallocatedAmount = remaining;
        await _context.SaveChangesAsync();

        return Result.Success("Payment allocated successfully");
    }

    private async Task<Result> AllocateToInvoiceAsync(Payment payment, Invoice invoice, decimal amountToAllocate, bool saveChanges = true)
    {
        if (amountToAllocate <= 0)
        {
            return Result.Failure("Allocation amount must be greater than zero");
        }

        if (GetInvoiceBalance(invoice) <= 0)
        {
            return Result.Failure("Invoice has no outstanding balance");
        }

        var allocationAmount = Math.Min(amountToAllocate, GetInvoiceBalance(invoice));

        var allocation = new PaymentAllocation
        {
            PaymentId = payment.Id,
            InvoiceId = invoice.Id,
            Amount = allocationAmount,
            CreatedAt = DateTime.UtcNow
        };

        _context.PaymentAllocations.Add(allocation);

        var allocatedTotal = invoice.Allocations.Sum(a => a.Amount) + allocationAmount;
        InvoiceStatusCalculator.Apply(invoice, allocatedTotal, DateTime.UtcNow);

        var paymentAllocated = payment.Allocations.Sum(a => a.Amount) + allocationAmount;
        payment.UnallocatedAmount = Math.Max(0, payment.Amount - paymentAllocated);

        if (saveChanges)
        {
            await _context.SaveChangesAsync();
        }

        return Result.Success("Payment allocated successfully");
    }

    private static decimal GetRemainingAmount(Payment payment)
    {
        var allocated = payment.Allocations.Sum(a => a.Amount);
        return Math.Max(0, payment.Amount - allocated);
    }

    private static decimal GetInvoiceBalance(Invoice invoice)
    {
        var allocated = invoice.Allocations.Sum(a => a.Amount);
        return InvoiceStatusCalculator.CalculateBalance(invoice, allocated);
    }

    public async Task<Result> ReverseAllocationsAsync(int paymentId, string? reason = null)
    {
        try
        {
            var payment = await _context.Payments
                .Include(p => p.Allocations)
                .Include(p => p.Unit)
                    .ThenInclude(u => u.Property)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return Result.Failure($"Payment with ID {paymentId} not found");
            }

            if (!await CanAccessPaymentAsync(payment))
            {
                return Result.Failure("You do not have permission to reverse this payment allocation");
            }

            if (payment.Allocations.Count == 0)
            {
                return Result.Success("No allocations to reverse");
            }

            var invoiceIds = payment.Allocations.Select(a => a.InvoiceId).Distinct().ToList();
            var remainingAllocations = await _context.PaymentAllocations
                .Where(a => invoiceIds.Contains(a.InvoiceId) && a.PaymentId != payment.Id)
                .GroupBy(a => a.InvoiceId)
                .Select(g => new { InvoiceId = g.Key, Total = g.Sum(x => x.Amount) })
                .ToDictionaryAsync(x => x.InvoiceId, x => x.Total);

            _context.PaymentAllocations.RemoveRange(payment.Allocations);
            payment.UnallocatedAmount = payment.Amount;
            payment.UpdatedAt = DateTime.UtcNow;

            var invoices = await _context.Invoices
                .Where(i => invoiceIds.Contains(i.Id))
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var invoice in invoices)
            {
                remainingAllocations.TryGetValue(invoice.Id, out var allocatedTotal);
                InvoiceStatusCalculator.Apply(invoice, allocatedTotal, now);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reversed allocations for payment {PaymentId}. Reason: {Reason}", paymentId, reason);
            return Result.Success("Payment allocations reversed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reversing allocations for payment {PaymentId}", paymentId);
            return Result.Failure("An error occurred while reversing payment allocations");
        }
    }

    private async Task<bool> CanAccessPaymentAsync(Payment payment)
    {
        if (_currentUserService.IsPlatformAdmin)
        {
            return true;
        }

        if (!_currentUserService.OrganizationId.HasValue ||
            payment.Unit?.Property?.OrganizationId != _currentUserService.OrganizationId.Value)
        {
            return false;
        }

        if (_currentUserService.IsLandlord)
        {
            return payment.Unit.Property.LandlordId == _currentUserService.UserIdInt;
        }

        if (_currentUserService.IsCaretaker)
        {
            return payment.Unit.PropertyId == _currentUserService.PropertyId;
        }

        if (_currentUserService.IsManager || _currentUserService.IsCaretaker || _currentUserService.IsAccountant)
        {
            var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
            return assignedPropertyIds.Contains(payment.Unit.PropertyId);
        }

        return false;
    }
}

