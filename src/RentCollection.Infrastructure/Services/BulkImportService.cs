using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common;
using RentCollection.Application.DTOs.BulkImport;
using RentCollection.Application.DTOs.Tenants;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using System.Globalization;
using System.Text;

namespace RentCollection.Infrastructure.Services
{
    public class BulkImportService : IBulkImportService
    {
        private readonly IUnitRepository _unitRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<BulkImportService> _logger;

        public BulkImportService(
            IUnitRepository unitRepository,
            ITenantRepository tenantRepository,
            IPaymentRepository paymentRepository,
            ICurrentUserService currentUserService,
            IAuditLogService auditLogService,
            ILogger<BulkImportService> logger)
        {
            _unitRepository = unitRepository;
            _tenantRepository = tenantRepository;
            _paymentRepository = paymentRepository;
            _currentUserService = currentUserService;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<ServiceResult<BulkImportResultDto>> ImportTenantsFromCsvAsync(IFormFile file, int propertyId)
        {
            try
            {
                if (!_currentUserService.IsLandlord && !_currentUserService.IsSystemAdmin)
                {
                    return ServiceResult<BulkImportResultDto>.Failure("Only landlords can import tenants");
                }

                var result = new BulkImportResultDto();
                var rows = await ParseTenantCsvAsync(file);
                result.TotalCount = rows.Count;

                foreach (var row in rows)
                {
                    try
                    {
                        // Find unit by number
                        var units = await _unitRepository.GetByPropertyIdAsync(propertyId);
                        var unit = units.FirstOrDefault(u => u.UnitNumber == row.UnitNumber);

                        if (unit == null)
                        {
                            result.FailureCount++;
                            result.Errors.Add($"Row {rows.IndexOf(row) + 2}: Unit {row.UnitNumber} not found");
                            continue;
                        }

                        // Check if tenant already exists
                        var existingTenant = await _tenantRepository.GetByEmailAsync(row.Email);
                        if (existingTenant != null)
                        {
                            result.FailureCount++;
                            result.Errors.Add($"Row {rows.IndexOf(row) + 2}: Tenant with email {row.Email} already exists");
                            continue;
                        }

                        // Create tenant
                        var tenant = new Tenant
                        {
                            FirstName = row.FirstName,
                            LastName = row.LastName,
                            Email = row.Email,
                            PhoneNumber = row.PhoneNumber,
                            IdNumber = row.IdNumber,
                            UnitId = unit.Id,
                            LeaseStartDate = row.LeaseStartDate,
                            LeaseEndDate = row.LeaseEndDate,
                            MonthlyRent = row.MonthlyRent,
                            SecurityDeposit = row.SecurityDeposit ?? 0,
                            IsActive = true
                        };

                        await _tenantRepository.AddAsync(tenant);

                        result.SuccessCount++;
                        result.SuccessMessages.Add($"Successfully imported {row.FirstName} {row.LastName} ({row.Email})");
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        result.Errors.Add($"Row {rows.IndexOf(row) + 2}: {ex.Message}");
                    }
                }

                await _auditLogService.LogActionAsync(
                    "BulkImport.Tenants",
                    "BulkImport",
                    propertyId,
                    $"Imported {result.SuccessCount}/{result.TotalCount} tenants");

                _logger.LogInformation("Bulk tenant import: {SuccessCount}/{TotalCount} successful",
                    result.SuccessCount, result.TotalCount);

                return ServiceResult<BulkImportResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk tenant import");
                return ServiceResult<BulkImportResultDto>.Failure("An error occurred during import: " + ex.Message);
            }
        }

        public async Task<ServiceResult<BulkImportResultDto>> ImportPaymentsFromCsvAsync(IFormFile file)
        {
            try
            {
                if (!_currentUserService.IsLandlord && !_currentUserService.IsSystemAdmin)
                {
                    return ServiceResult<BulkImportResultDto>.Failure("Only landlords can import payments");
                }

                var result = new BulkImportResultDto();
                var rows = await ParsePaymentCsvAsync(file);
                result.TotalCount = rows.Count;

                foreach (var row in rows)
                {
                    try
                    {
                        // Find tenant by email
                        var tenant = await _tenantRepository.GetByEmailAsync(row.TenantEmail);
                        if (tenant == null)
                        {
                            result.FailureCount++;
                            result.Errors.Add($"Row {rows.IndexOf(row) + 2}: Tenant with email {row.TenantEmail} not found");
                            continue;
                        }

                        // Create payment
                        var payment = new Payment
                        {
                            TenantId = tenant.Id,
                            Amount = row.Amount,
                            PaymentDate = row.PaymentDate,
                            PaymentMethod = Enum.Parse<PaymentMethod>(row.PaymentMethod, ignoreCase: true),
                            TransactionReference = row.ReferenceNumber,
                            Notes = row.Notes
                        };

                        await _paymentRepository.AddAsync(payment);

                        result.SuccessCount++;
                        result.SuccessMessages.Add($"Imported payment of KES {row.Amount} for {row.TenantEmail}");
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        result.Errors.Add($"Row {rows.IndexOf(row) + 2}: {ex.Message}");
                    }
                }

                await _auditLogService.LogActionAsync(
                    "BulkImport.Payments",
                    "BulkImport",
                    0,
                    $"Imported {result.SuccessCount}/{result.TotalCount} payments");

                _logger.LogInformation("Bulk payment import: {SuccessCount}/{TotalCount} successful",
                    result.SuccessCount, result.TotalCount);

                return ServiceResult<BulkImportResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk payment import");
                return ServiceResult<BulkImportResultDto>.Failure("An error occurred during import: " + ex.Message);
            }
        }

        public async Task<BulkImportResultDto> ValidateTenantCsvAsync(IFormFile file, int propertyId)
        {
            var result = new BulkImportResultDto();
            try
            {
                var rows = await ParseTenantCsvAsync(file);
                result.TotalCount = rows.Count;

                foreach (var row in rows)
                {
                    var errors = new List<string>();

                    if (string.IsNullOrWhiteSpace(row.FirstName)) errors.Add("First name is required");
                    if (string.IsNullOrWhiteSpace(row.LastName)) errors.Add("Last name is required");
                    if (string.IsNullOrWhiteSpace(row.Email)) errors.Add("Email is required");
                    if (string.IsNullOrWhiteSpace(row.PhoneNumber)) errors.Add("Phone number is required");
                    if (string.IsNullOrWhiteSpace(row.UnitNumber)) errors.Add("Unit number is required");
                    if (row.MonthlyRent <= 0) errors.Add("Monthly rent must be greater than 0");

                    if (errors.Any())
                    {
                        result.FailureCount++;
                        result.Errors.Add($"Row {rows.IndexOf(row) + 2}: " + string.Join(", ", errors));
                    }
                    else
                    {
                        result.SuccessCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"CSV parsing error: {ex.Message}");
            }

            return result;
        }

        public async Task<BulkImportResultDto> ValidatePaymentCsvAsync(IFormFile file)
        {
            var result = new BulkImportResultDto();
            try
            {
                var rows = await ParsePaymentCsvAsync(file);
                result.TotalCount = rows.Count;

                foreach (var row in rows)
                {
                    var errors = new List<string>();

                    if (string.IsNullOrWhiteSpace(row.TenantEmail)) errors.Add("Tenant email is required");
                    if (row.Amount <= 0) errors.Add("Amount must be greater than 0");
                    if (string.IsNullOrWhiteSpace(row.PaymentMethod)) errors.Add("Payment method is required");

                    if (errors.Any())
                    {
                        result.FailureCount++;
                        result.Errors.Add($"Row {rows.IndexOf(row) + 2}: " + string.Join(", ", errors));
                    }
                    else
                    {
                        result.SuccessCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"CSV parsing error: {ex.Message}");
            }

            return result;
        }

        private async Task<List<TenantImportRow>> ParseTenantCsvAsync(IFormFile file)
        {
            var rows = new List<TenantImportRow>();
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            // Skip header
            await reader.ReadLineAsync();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = line.Split(',');
                if (values.Length < 8) continue;

                var row = new TenantImportRow
                {
                    FirstName = values[0].Trim(),
                    LastName = values[1].Trim(),
                    Email = values[2].Trim(),
                    PhoneNumber = values[3].Trim(),
                    UnitNumber = values[4].Trim(),
                    LeaseStartDate = DateTime.Parse(values[5].Trim()),
                    LeaseEndDate = string.IsNullOrWhiteSpace(values[6]) ? null : DateTime.Parse(values[6].Trim()),
                    MonthlyRent = decimal.Parse(values[7].Trim()),
                    SecurityDeposit = values.Length > 8 && !string.IsNullOrWhiteSpace(values[8]) ? decimal.Parse(values[8].Trim()) : null,
                    IdNumber = values.Length > 9 ? values[9].Trim() : null
                };

                rows.Add(row);
            }

            return rows;
        }

        private async Task<List<PaymentImportRow>> ParsePaymentCsvAsync(IFormFile file)
        {
            var rows = new List<PaymentImportRow>();
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            // Skip header
            await reader.ReadLineAsync();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = line.Split(',');
                if (values.Length < 4) continue;

                var row = new PaymentImportRow
                {
                    TenantEmail = values[0].Trim(),
                    Amount = decimal.Parse(values[1].Trim()),
                    PaymentDate = DateTime.Parse(values[2].Trim()),
                    PaymentMethod = values[3].Trim(),
                    ReferenceNumber = values.Length > 4 ? values[4].Trim() : null,
                    Notes = values.Length > 5 ? values[5].Trim() : null
                };

                rows.Add(row);
            }

            return rows;
        }
    }
}
