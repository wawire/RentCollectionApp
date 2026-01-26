using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.TenantPortal;
using RentCollection.Application.Helpers;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Services;

public class TenantPortalService : ITenantPortalService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TenantPortalService> _logger;

    public TenantPortalService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<TenantPortalService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<TenantDashboardDto>> GetDashboardAsync(int tenantId)
    {
        try
        {
            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                    .ThenInclude(u => u.Property)
                .Include(t => t.Payments)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
            {
                return Result<TenantDashboardDto>.Failure($"Tenant with ID {tenantId} not found");
            }

            var dashboard = await BuildDashboardAsync(tenant);

            return Result<TenantDashboardDto>.Success(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard for tenant {TenantId}", tenantId);
            return Result<TenantDashboardDto>.Failure("An error occurred while loading dashboard");
        }
    }

    public async Task<Result<TenantLeaseInfoDto>> GetLeaseInfoAsync(int tenantId)
    {
        try
        {
            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                    .ThenInclude(u => u.Property)
                        .ThenInclude(p => p.PropertyAmenities)
                            .ThenInclude(pa => pa.Amenity)
                .Include(t => t.Unit)
                    .ThenInclude(u => u.Property)
                        .ThenInclude(p => p.PaymentAccounts.Where(pa => pa.IsActive))
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
            {
                return Result<TenantLeaseInfoDto>.Failure($"Tenant with ID {tenantId} not found");
            }

            var leaseInfo = BuildLeaseInfo(tenant);

            return Result<TenantLeaseInfoDto>.Success(leaseInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lease info for tenant {TenantId}", tenantId);
            return Result<TenantLeaseInfoDto>.Failure("An error occurred while loading lease information");
        }
    }

    public async Task<Result<TenantDashboardDto>> GetMyDashboardAsync()
    {
        if (!_currentUserService.IsTenant || !_currentUserService.TenantId.HasValue)
        {
            return Result<TenantDashboardDto>.Failure("You must be a tenant to view this dashboard");
        }

        return await GetDashboardAsync(_currentUserService.TenantId.Value);
    }

    public async Task<Result<TenantLeaseInfoDto>> GetMyLeaseInfoAsync()
    {
        if (!_currentUserService.IsTenant || !_currentUserService.TenantId.HasValue)
        {
            return Result<TenantLeaseInfoDto>.Failure("You must be a tenant to view lease information");
        }

        return await GetLeaseInfoAsync(_currentUserService.TenantId.Value);
    }

    // Helper methods

    private async Task<TenantDashboardDto> BuildDashboardAsync(Domain.Entities.Tenant tenant)
    {
        var today = DateTime.UtcNow.Date;

        // Payments history
        var allPayments = tenant.Payments.ToList();
        var confirmedPayments = allPayments.Where(p => p.Status == PaymentStatus.Completed).ToList();

        // Use invoices as the source of truth for balance and overdue status
        var invoices = await _context.Invoices
            .Include(i => i.Allocations)
            .Include(i => i.LineItems)
            .Where(i => i.TenantId == tenant.Id && i.Status != InvoiceStatus.Void)
            .ToListAsync();

        var invoiceSummaries = invoices
            .Select(i =>
            {
                var allocated = i.Allocations.Sum(a => a.Amount);
                var balance = InvoiceStatusCalculator.CalculateBalance(i, allocated);
                var status = InvoiceStatusCalculator.CalculateStatus(i, allocated, DateTime.UtcNow);
                return new { Invoice = i, Balance = balance, Status = status };
            })
            .ToList();

        var overdueInvoices = invoiceSummaries
            .Where(i => i.Balance > 0 && i.Invoice.DueDate.Date < today)
            .OrderBy(i => i.Invoice.DueDate)
            .ToList();

        var hasOverdue = overdueInvoices.Any();
        var overdueAmount = overdueInvoices.Sum(i => i.Balance);
        var daysOverdue = hasOverdue ? (today - overdueInvoices.First().Invoice.DueDate.Date).Days : 0;

        // Calculate next payment
        var nextDueDate = PaymentDueDateHelper.CalculateNextMonthDueDate(tenant.RentDueDay);
        var daysUntilDue = (nextDueDate.Date - today).Days;

        // Current balance (all outstanding invoices)
        var currentBalance = invoiceSummaries.Sum(i => i.Balance);

        // Recent payments (last 5 confirmed)
        var recentPayments = confirmedPayments
            .OrderByDescending(p => p.PaymentDate)
            .Take(5)
            .Select(p => new RecentPaymentDto
            {
                Id = p.Id,
                Amount = p.Amount,
                LateFeeAmount = p.LateFeeAmount,
                TotalAmount = p.TotalAmount,
                PaymentDate = p.PaymentDate,
                DueDate = p.DueDate,
                PaymentMethod = p.PaymentMethod.ToString(),
                Status = p.Status.ToString(),
                TransactionReference = p.TransactionReference,
                IsLate = p.IsLate,
                DaysOverdue = p.DaysOverdue
            })
            .ToList();

        // Pending invoices
        var pendingPaymentsDtos = invoiceSummaries
            .Where(i => i.Balance > 0)
            .OrderBy(i => i.Invoice.DueDate)
            .Select(i => new RecentPaymentDto
            {
                Id = i.Invoice.Id,
                Amount = i.Invoice.Amount + i.Invoice.OpeningBalance,
                LateFeeAmount = 0,
                TotalAmount = i.Balance,
                PaymentDate = i.Invoice.CreatedAt,
                DueDate = i.Invoice.DueDate,
                PaymentMethod = "Invoice",
                Status = i.Status.ToString(),
                TransactionReference = null,
                IsLate = i.Invoice.DueDate.Date < today,
                DaysOverdue = i.Invoice.DueDate.Date < today ? (today - i.Invoice.DueDate.Date).Days : 0
            })
            .ToList();

        // Document count
        var documentCount = await _context.Documents
            .CountAsync(d => d.TenantId == tenant.Id);

        // Lease expiry
        int? daysUntilLeaseExpiry = null;
        if (tenant.LeaseEndDate.HasValue)
        {
            daysUntilLeaseExpiry = (tenant.LeaseEndDate.Value.Date - today).Days;
        }

        return new TenantDashboardDto
        {
            TenantInfo = new TenantInfoDto
            {
                Id = tenant.Id,
                FullName = tenant.FullName,
                Email = tenant.Email,
                PhoneNumber = tenant.PhoneNumber,
                PropertyName = tenant.Unit?.Property?.Name ?? "",
                UnitNumber = tenant.Unit?.UnitNumber ?? "",
                MonthlyRent = tenant.MonthlyRent,
                LeaseStartDate = tenant.LeaseStartDate,
                LeaseEndDate = tenant.LeaseEndDate
            },
            CurrentBalance = currentBalance,
            NextPaymentDueDate = nextDueDate,
            NextPaymentAmount = tenant.MonthlyRent,
            DaysUntilDue = daysUntilDue,
            HasOverduePayments = hasOverdue,
            OverdueAmount = overdueAmount,
            DaysOverdue = daysOverdue,
            TotalPaymentsMade = confirmedPayments.Count,
            TotalAmountPaid = confirmedPayments.Sum(p => p.Amount),
            RecentPayments = recentPayments,
            PendingPayments = pendingPaymentsDtos,
            DocumentCount = documentCount,
            LeaseExpiryDate = tenant.LeaseEndDate,
            DaysUntilLeaseExpiry = daysUntilLeaseExpiry,
            LateFeePolicy = LateFeeCalculator.GetLateFeePolicy(tenant)
        };
    }

    private TenantLeaseInfoDto BuildLeaseInfo(Domain.Entities.Tenant tenant)
    {
        var property = tenant.Unit?.Property;
        var unit = tenant.Unit;

        // Get active payment account
        var paymentAccount = property?.PaymentAccounts.FirstOrDefault(pa => pa.IsActive);

        // Property amenities
        var amenities = property?.PropertyAmenities
            .Select(pa => pa.Amenity.Name)
            .ToList() ?? new List<string>();

        // Calculate days until expiry
        int? daysUntilExpiry = null;
        if (tenant.LeaseEndDate.HasValue)
        {
            daysUntilExpiry = (tenant.LeaseEndDate.Value.Date - DateTime.UtcNow.Date).Days;
        }

        return new TenantLeaseInfoDto
        {
            Tenant = new TenantDetailsDto
            {
                FullName = tenant.FullName,
                Email = tenant.Email,
                PhoneNumber = tenant.PhoneNumber,
                IdNumber = tenant.IdNumber
            },
            Property = new PropertyDetailsDto
            {
                Name = property?.Name ?? "",
                Address = property?.Location,
                City = property?.Location,
                Description = property?.Description,
                Amenities = amenities
            },
            Unit = new UnitDetailsDto
            {
                UnitNumber = unit?.UnitNumber ?? "",
                Type = unit?.RentalType.ToString() ?? "",
                Bedrooms = unit?.Bedrooms ?? 0,
                Bathrooms = unit?.Bathrooms ?? 0,
                SquareFeet = unit?.SquareFeet ?? 0,
                Description = unit?.Description
            },
            Lease = new LeaseDetailsDto
            {
                StartDate = tenant.LeaseStartDate,
                EndDate = tenant.LeaseEndDate,
                MonthlyRent = tenant.MonthlyRent,
                SecurityDeposit = tenant.SecurityDeposit,
                RentDueDay = tenant.RentDueDay,
                LateFeeGracePeriodDays = tenant.LateFeeGracePeriodDays,
                LateFeePercentage = tenant.LateFeePercentage,
                LateFeeFixedAmount = tenant.LateFeeFixedAmount,
                LateFeePolicy = LateFeeCalculator.GetLateFeePolicy(tenant),
                IsActive = tenant.IsActive,
                DaysUntilExpiry = daysUntilExpiry
            },
            PaymentAccount = new PaymentAccountInfoDto
            {
                AccountType = paymentAccount?.AccountType.ToString() ?? "",
                AccountName = paymentAccount?.AccountName ?? "",
                AccountNumber = paymentAccount?.AccountType switch
                {
                    PaymentAccountType.BankAccount => paymentAccount.BankAccountNumber,
                    PaymentAccountType.MPesaPaybill => paymentAccount.PaybillNumber,
                    PaymentAccountType.MPesaTillNumber => paymentAccount.TillNumber,
                    PaymentAccountType.MPesaPhone => paymentAccount.MPesaPhoneNumber,
                    _ => null
                },
                BankName = paymentAccount?.BankName,
                MPesaPaybill = paymentAccount?.MPesaShortCode,
                PaymentAccountNumber = unit?.PaymentAccountNumber,
                Instructions = BuildPaymentInstructions(paymentAccount, unit)
            }
        };
    }

    private string BuildPaymentInstructions(Domain.Entities.LandlordPaymentAccount? paymentAccount, Domain.Entities.Unit? unit)
    {
        if (paymentAccount == null)
        {
            return "Please contact your landlord for payment instructions.";
        }

        var accountNumber = unit?.PaymentAccountNumber ?? unit?.UnitNumber ?? "your unit number";

        return paymentAccount.AccountType switch
        {
            PaymentAccountType.BankAccount =>
                $"Bank: {paymentAccount.BankName}\n" +
                $"Account Name: {paymentAccount.AccountName}\n" +
                $"Account Number: {paymentAccount.BankAccountNumber}\n" +
                $"Reference: {accountNumber}",

            PaymentAccountType.MPesaPaybill =>
                $"M-Pesa Paybill: {paymentAccount.MPesaShortCode}\n" +
                $"Account Number: {accountNumber}\n" +
                $"Amount: Your monthly rent",

            PaymentAccountType.MPesaTillNumber =>
                $"M-Pesa Till Number: {paymentAccount.MPesaShortCode}\n" +
                $"Amount: Your monthly rent",

            _ => "Please contact your landlord for payment instructions."
        };
    }
}
