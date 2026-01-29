using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Services;

public class CsvExportService : IExportService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;

    public CsvExportService(ApplicationDbContext context, ICurrentUserService currentUserService, IAuditLogService auditLogService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditLogService = auditLogService;
    }

    public async Task<string> ExportPaymentsAsync(int? propertyId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Payments
            .Include(p => p.Tenant)
                .ThenInclude(t => t.Unit)
                    .ThenInclude(u => u.Property)
            .AsQueryable();

        query = await ApplyPaymentScopeAsync(query);

        if (propertyId.HasValue)
        {
            query = query.Where(p => p.Unit.PropertyId == propertyId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate <= endDate.Value);
        }

        var payments = await query.OrderByDescending(p => p.PaymentDate).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("PaymentId,Property,Unit,Tenant,Amount,PaymentDate,Method,Status,TransactionRef");

        foreach (var payment in payments)
        {
            sb.AppendLine(string.Join(",",
                payment.Id,
                Escape(payment.Unit.Property.Name),
                Escape(payment.Unit.UnitNumber),
                Escape(payment.Tenant.FullName),
                payment.Amount.ToString("F2", CultureInfo.InvariantCulture),
                payment.PaymentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                payment.PaymentMethod,
                payment.Status,
                Escape(payment.TransactionReference ?? string.Empty)));
        }

        await _auditLogService.LogActionAsync(
            "Export",
            "Payments",
            0,
            $"Exported payments CSV for propertyId={propertyId?.ToString() ?? "all"}");

        return sb.ToString();
    }

    public async Task<string> ExportInvoicesAsync(int? propertyId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Invoices
            .Include(i => i.Tenant)
            .Include(i => i.Unit)
            .Include(i => i.Property)
            .Include(i => i.LineItems)
                .ThenInclude(li => li.UtilityType)
            .AsQueryable();

        query = await ApplyInvoiceScopeAsync(query);

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

        var invoices = await query.OrderByDescending(i => i.PeriodStart).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("InvoiceId,Property,Unit,Tenant,PeriodStart,PeriodEnd,DueDate,Amount,OpeningBalance,Balance,Status,LineItemType,UtilityName,Quantity,Rate,LineAmount");

        foreach (var invoice in invoices)
        {
            if (invoice.LineItems.Count == 0)
            {
                sb.AppendLine(string.Join(",",
                    invoice.Id,
                    Escape(invoice.Property.Name),
                    Escape(invoice.Unit.UnitNumber),
                    Escape(invoice.Tenant.FullName),
                    invoice.PeriodStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    invoice.PeriodEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    invoice.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    invoice.Amount.ToString("F2", CultureInfo.InvariantCulture),
                    invoice.OpeningBalance.ToString("F2", CultureInfo.InvariantCulture),
                    invoice.Balance.ToString("F2", CultureInfo.InvariantCulture),
                    invoice.Status,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty));
                continue;
            }

            foreach (var lineItem in invoice.LineItems)
            {
                sb.AppendLine(string.Join(",",
                    invoice.Id,
                    Escape(invoice.Property.Name),
                    Escape(invoice.Unit.UnitNumber),
                    Escape(invoice.Tenant.FullName),
                    invoice.PeriodStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    invoice.PeriodEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    invoice.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    invoice.Amount.ToString("F2", CultureInfo.InvariantCulture),
                    invoice.OpeningBalance.ToString("F2", CultureInfo.InvariantCulture),
                    invoice.Balance.ToString("F2", CultureInfo.InvariantCulture),
                    invoice.Status,
                    lineItem.LineItemType,
                    Escape(lineItem.UtilityType?.Name ?? string.Empty),
                    lineItem.Quantity.ToString("F2", CultureInfo.InvariantCulture),
                    lineItem.Rate.ToString("F2", CultureInfo.InvariantCulture),
                    lineItem.Amount.ToString("F2", CultureInfo.InvariantCulture)));
            }
        }

        await _auditLogService.LogActionAsync(
            "Export",
            "Invoices",
            0,
            $"Exported invoices CSV for propertyId={propertyId?.ToString() ?? "all"}");

        return sb.ToString();
    }

    public async Task<string> ExportExpensesAsync(int? propertyId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Expenses
            .Include(e => e.Property)
            .AsQueryable();

        query = await ApplyExpenseScopeAsync(query);

        if (propertyId.HasValue)
        {
            query = query.Where(e => e.PropertyId == propertyId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate <= endDate.Value);
        }

        var expenses = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("ExpenseId,Property,Category,Amount,ExpenseDate,Vendor,Description");

        foreach (var expense in expenses)
        {
            sb.AppendLine(string.Join(",",
                expense.Id,
                Escape(expense.Property.Name),
                expense.Category,
                expense.Amount.ToString("F2", CultureInfo.InvariantCulture),
                expense.ExpenseDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Escape(expense.Vendor),
                Escape(expense.Description)));
        }

        await _auditLogService.LogActionAsync(
            "Export",
            "Expenses",
            0,
            $"Exported expenses CSV for propertyId={propertyId?.ToString() ?? "all"}");

        return sb.ToString();
    }

    public async Task<string> ExportArrearsAsync(int? propertyId)
    {
        var query = _context.Invoices
            .Include(i => i.Tenant)
            .Include(i => i.Unit)
            .Include(i => i.Property)
            .Where(i => i.Balance > 0 && i.Status != InvoiceStatus.Void);

        query = await ApplyInvoiceScopeAsync(query);

        if (propertyId.HasValue)
        {
            query = query.Where(i => i.PropertyId == propertyId.Value);
        }

        var invoices = await query.OrderBy(i => i.DueDate).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("InvoiceId,Property,Unit,Tenant,DueDate,Balance,Status");

        foreach (var invoice in invoices)
        {
            sb.AppendLine(string.Join(",",
                invoice.Id,
                Escape(invoice.Property.Name),
                Escape(invoice.Unit.UnitNumber),
                Escape(invoice.Tenant.FullName),
                invoice.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                invoice.Balance.ToString("F2", CultureInfo.InvariantCulture),
                invoice.Status));
        }

        await _auditLogService.LogActionAsync(
            "Export",
            "Arrears",
            0,
            $"Exported arrears CSV for propertyId={propertyId?.ToString() ?? "all"}");

        return sb.ToString();
    }

    private async Task<IQueryable<Payment>> ApplyPaymentScopeAsync(IQueryable<Payment> query)
    {
        if (_currentUserService.IsPlatformAdmin)
        {
            return query;
        }

        if (!_currentUserService.OrganizationId.HasValue)
        {
            return query.Where(_ => false);
        }

        query = query.Where(p => p.Unit.Property.OrganizationId == _currentUserService.OrganizationId.Value);

        if (_currentUserService.IsLandlord && _currentUserService.UserIdInt.HasValue)
        {
            return query.Where(p => p.Unit.Property.LandlordId == _currentUserService.UserIdInt.Value);
        }

        if ((_currentUserService.IsCaretaker || _currentUserService.IsManager || _currentUserService.IsAccountant))
        {
            var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
            return assignedPropertyIds.Count == 0
                ? query.Where(_ => false)
                : query.Where(p => assignedPropertyIds.Contains(p.Unit.PropertyId));
        }

        if (_currentUserService.IsTenant && _currentUserService.TenantId.HasValue)
        {
            return query.Where(p => p.TenantId == _currentUserService.TenantId.Value);
        }

        return query.Where(_ => false);
    }

    private async Task<IQueryable<Invoice>> ApplyInvoiceScopeAsync(IQueryable<Invoice> query)
    {
        if (_currentUserService.IsPlatformAdmin)
        {
            return query;
        }

        if (!_currentUserService.OrganizationId.HasValue)
        {
            return query.Where(_ => false);
        }

        query = query.Where(i => i.Property.OrganizationId == _currentUserService.OrganizationId.Value);

        if (_currentUserService.IsLandlord && _currentUserService.UserIdInt.HasValue)
        {
            return query.Where(i => i.LandlordId == _currentUserService.UserIdInt.Value);
        }

        if ((_currentUserService.IsCaretaker || _currentUserService.IsManager || _currentUserService.IsAccountant))
        {
            var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
            return assignedPropertyIds.Count == 0
                ? query.Where(_ => false)
                : query.Where(i => assignedPropertyIds.Contains(i.PropertyId));
        }

        if (_currentUserService.IsTenant && _currentUserService.TenantId.HasValue)
        {
            return query.Where(i => i.TenantId == _currentUserService.TenantId.Value);
        }

        return query.Where(_ => false);
    }

    private async Task<IQueryable<Expense>> ApplyExpenseScopeAsync(IQueryable<Expense> query)
    {
        if (_currentUserService.IsPlatformAdmin)
        {
            return query;
        }

        if (!_currentUserService.OrganizationId.HasValue)
        {
            return query.Where(_ => false);
        }

        query = query.Where(e => e.Property.OrganizationId == _currentUserService.OrganizationId.Value);

        if (_currentUserService.IsLandlord && _currentUserService.UserIdInt.HasValue)
        {
            return query.Where(e => e.Property.LandlordId == _currentUserService.UserIdInt.Value);
        }

        if ((_currentUserService.IsCaretaker || _currentUserService.IsManager || _currentUserService.IsAccountant))
        {
            var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
            return assignedPropertyIds.Count == 0
                ? query.Where(_ => false)
                : query.Where(e => assignedPropertyIds.Contains(e.PropertyId));
        }

        return query.Where(_ => false);
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}

