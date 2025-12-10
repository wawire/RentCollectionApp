using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentCollection.Application.Common;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Configuration;
using RentCollection.Infrastructure.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RentCollection.Infrastructure.Services;

/// <summary>
/// Service for M-Pesa Daraja API integration
/// Documentation: https://developer.safaricom.co.ke/
/// </summary>
public class MPesaService : IMPesaService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MPesaService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MPesaConfiguration _mpesaConfig;

    // M-Pesa API URLs
    private const string SandboxBaseUrl = "https://sandbox.safaricom.co.ke";
    private const string ProductionBaseUrl = "https://api.safaricom.co.ke";

    public MPesaService(
        ApplicationDbContext context,
        ILogger<MPesaService> logger,
        IHttpClientFactory httpClientFactory,
        IOptions<MPesaConfiguration> mpesaConfig)
    {
        _context = context;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _mpesaConfig = mpesaConfig.Value;
    }

    public async Task<ServiceResult<StkPushCallbackDto>> InitiateStkPushAsync(int tenantId, InitiateStkPushDto dto)
    {
        try
        {
            // Get tenant with unit and payment account details
            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                    .ThenInclude(u => u.Property)
                        .ThenInclude(p => p.PaymentAccounts)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
            {
                return ServiceResult<StkPushCallbackDto>.Failure($"Tenant with ID {tenantId} not found");
            }

            if (tenant.Unit == null)
            {
                return ServiceResult<StkPushCallbackDto>.Failure("Tenant does not have an assigned unit");
            }

            // Get payment account with M-Pesa credentials
            var paymentAccount = tenant.Unit.Property.PaymentAccounts
                .FirstOrDefault(pa => pa.AccountType == PaymentAccountType.MPesaPaybill && pa.IsActive);

            if (paymentAccount == null)
            {
                return ServiceResult<StkPushCallbackDto>.Failure("No M-Pesa Paybill account configured for this property");
            }

            if (string.IsNullOrEmpty(paymentAccount.MPesaConsumerKey) ||
                string.IsNullOrEmpty(paymentAccount.MPesaConsumerSecret))
            {
                return ServiceResult<StkPushCallbackDto>.Failure("M-Pesa API credentials not configured");
            }

            // Get access token
            var accessTokenResult = await GetAccessTokenAsync(paymentAccount.Id);
            if (!accessTokenResult.IsSuccess)
            {
                return ServiceResult<StkPushCallbackDto>.Failure(accessTokenResult.ErrorMessage);
            }

            // Format phone number (remove leading 0, add 254)
            var phoneNumber = FormatPhoneNumber(dto.PhoneNumber);

            // Generate password and timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var password = GeneratePassword(paymentAccount.MPesaShortCode!, paymentAccount.MPesaPasskey!, timestamp);

            // Prepare STK Push request
            var stkPushRequest = new
            {
                BusinessShortCode = paymentAccount.MPesaShortCode,
                Password = password,
                Timestamp = timestamp,
                TransactionType = "CustomerPayBillOnline",
                Amount = dto.Amount.ToString("0"),
                PartyA = phoneNumber,
                PartyB = paymentAccount.MPesaShortCode,
                PhoneNumber = phoneNumber,
                CallBackURL = $"{_mpesaConfig.CallbackBaseUrl}/api/mpesa/stkpush/callback",
                AccountReference = tenant.Unit.PaymentAccountNumber ?? tenant.Unit.UnitNumber,
                TransactionDesc = $"Rent payment for {tenant.Unit.Property.Name} - {tenant.Unit.UnitNumber}"
            };

            // Make STK Push request
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenResult.Data);

            var requestContent = new StringContent(
                JsonSerializer.Serialize(stkPushRequest),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                $"{GetBaseUrl()}/mpesa/stkpush/v1/processrequest",
                requestContent);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("STK Push failed: {Response}", responseContent);
                return ServiceResult<StkPushCallbackDto>.Failure("Failed to initiate STK Push. Please try again.");
            }

            var stkPushResponse = JsonSerializer.Deserialize<StkPushCallbackDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Track the STK Push transaction
            if (stkPushResponse != null)
            {
                var mpesaTransaction = new MPesaTransaction
                {
                    MerchantRequestID = stkPushResponse.MerchantRequestID ?? string.Empty,
                    CheckoutRequestID = stkPushResponse.CheckoutRequestID ?? string.Empty,
                    PhoneNumber = phoneNumber,
                    Amount = dto.Amount,
                    AccountReference = tenant.Unit.PaymentAccountNumber ?? tenant.Unit.UnitNumber,
                    TransactionDesc = $"Rent payment for {tenant.Unit.Property.Name} - {tenant.Unit.UnitNumber}",
                    TenantId = tenantId,
                    Status = MPesaTransactionStatus.Pending,
                    ResultCode = 0,
                    ResultDesc = "STK Push initiated",
                    RequestJson = _mpesaConfig.EnableDetailedLogging ? JsonSerializer.Serialize(stkPushRequest) : null,
                    ResponseJson = _mpesaConfig.EnableDetailedLogging ? responseContent : null,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MPesaTransactions.Add(mpesaTransaction);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("STK Push initiated for tenant {TenantId}, phone {Phone}", tenantId, phoneNumber);

            return ServiceResult<StkPushCallbackDto>.Success(stkPushResponse!, "STK Push sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating STK Push for tenant {TenantId}", tenantId);
            return ServiceResult<StkPushCallbackDto>.Failure("An error occurred while initiating STK Push");
        }
    }

    public async Task<ServiceResult<StkPushCallbackDto>> QueryStkPushStatusAsync(string checkoutRequestId)
    {
        try
        {
            // TODO: Implement STK Push query
            // This would use the Query Request endpoint to check payment status

            _logger.LogInformation("Querying STK Push status for CheckoutRequestID: {CheckoutRequestId}", checkoutRequestId);

            // Placeholder response
            return ServiceResult<StkPushCallbackDto>.Failure("STK Push query not yet implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying STK Push status");
            return ServiceResult<StkPushCallbackDto>.Failure("An error occurred while querying STK Push status");
        }
    }

    public async Task<ServiceResult<bool>> HandleC2BCallbackAsync(object callbackData)
    {
        try
        {
            _logger.LogInformation("Received M-Pesa C2B callback: {CallbackData}", JsonSerializer.Serialize(callbackData));

            // Deserialize callback data
            var json = JsonSerializer.Serialize(callbackData);
            var callback = JsonSerializer.Deserialize<MPesaC2BCallbackDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (callback == null)
            {
                _logger.LogError("Failed to deserialize M-Pesa C2B callback data");
                return ServiceResult<bool>.Failure("Invalid callback data");
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(callback.TransID) ||
                string.IsNullOrWhiteSpace(callback.BillRefNumber) ||
                callback.TransAmount <= 0)
            {
                _logger.LogError("M-Pesa callback missing required fields: {Callback}", json);
                return ServiceResult<bool>.Failure("Missing required fields in callback");
            }

            // Check for duplicate transaction
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.TransactionReference == callback.TransID);

            if (existingPayment != null)
            {
                _logger.LogWarning("Duplicate M-Pesa transaction received: {TransID}", callback.TransID);
                return ServiceResult<bool>.Success(true, "Transaction already processed");
            }

            // Find unit by payment account number (BillRefNumber)
            // The BillRefNumber should match the unit's PaymentAccountNumber (e.g., "A101", "B205")
            var unit = await _context.Units
                .Include(u => u.Property)
                    .ThenInclude(p => p.PaymentAccounts)
                .Include(u => u.Tenants.Where(t => t.IsActive))
                .FirstOrDefaultAsync(u => u.PaymentAccountNumber == callback.BillRefNumber);

            if (unit == null)
            {
                _logger.LogWarning("No unit found for BillRefNumber: {BillRefNumber}", callback.BillRefNumber);

                // Try to find unit by unit number as fallback
                unit = await _context.Units
                    .Include(u => u.Property)
                        .ThenInclude(p => p.PaymentAccounts)
                    .Include(u => u.Tenants.Where(t => t.IsActive))
                    .FirstOrDefaultAsync(u => u.UnitNumber == callback.BillRefNumber);

                if (unit == null)
                {
                    _logger.LogError("Unit not found for account number: {BillRefNumber}", callback.BillRefNumber);
                    return ServiceResult<bool>.Failure($"No unit found for account number: {callback.BillRefNumber}");
                }
            }

            // Get active tenant for the unit
            var tenant = unit.Tenants.FirstOrDefault(t => t.IsActive);
            if (tenant == null)
            {
                _logger.LogError("No active tenant found for unit: {UnitId}", unit.Id);
                return ServiceResult<bool>.Failure($"No active tenant found for unit {unit.UnitNumber}");
            }

            // Get landlord payment account
            var paymentAccount = unit.Property.PaymentAccounts
                .FirstOrDefault(pa => pa.AccountType == PaymentAccountType.MPesaPaybill &&
                                     pa.MPesaShortCode == callback.BusinessShortCode);

            if (paymentAccount == null)
            {
                _logger.LogError("No M-Pesa payment account found for shortcode: {ShortCode}", callback.BusinessShortCode);
                return ServiceResult<bool>.Failure("Payment account not found");
            }

            // Parse transaction time
            DateTime transactionDate;
            try
            {
                // M-Pesa format: "20240315120530" (YYYYMMDDHHmmss)
                transactionDate = DateTime.ParseExact(
                    callback.TransTime,
                    "yyyyMMddHHmmss",
                    System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                transactionDate = DateTime.UtcNow;
                _logger.LogWarning("Failed to parse transaction time: {TransTime}, using current time", callback.TransTime);
            }

            // Calculate due date for the current payment period
            var dueDate = Application.Helpers.PaymentDueDateHelper.CalculateCurrentMonthDueDate(tenant.RentDueDay);
            var (periodStart, periodEnd) = Application.Helpers.PaymentDueDateHelper.CalculatePaymentPeriod(dueDate);

            // Create payment record
            var payment = new Payment
            {
                TenantId = tenant.Id,
                UnitId = unit.Id,
                LandlordAccountId = paymentAccount.Id,
                Amount = callback.TransAmount,
                PaymentDate = transactionDate,
                DueDate = dueDate,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                PaymentMethod = PaymentMethod.MPesa,
                Status = PaymentStatus.Pending, // Pending until landlord confirms
                TransactionReference = callback.TransID,
                PaybillAccountNumber = callback.BillRefNumber,
                MPesaPhoneNumber = callback.MSISDN,
                Notes = $"M-Pesa C2B auto-recorded payment from {callback.FirstName} {callback.LastName}".Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Auto-created payment record from M-Pesa C2B: PaymentId={PaymentId}, TransID={TransID}, Tenant={TenantId}, Amount={Amount}",
                payment.Id, callback.TransID, tenant.Id, callback.TransAmount);

            return ServiceResult<bool>.Success(true, "Payment recorded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling M-Pesa C2B callback");
            return ServiceResult<bool>.Failure("An error occurred while processing the callback");
        }
    }

    public async Task<ServiceResult<string>> GetAccessTokenAsync(int landlordAccountId)
    {
        try
        {
            var account = await _context.LandlordPaymentAccounts
                .FirstOrDefaultAsync(a => a.Id == landlordAccountId);

            if (account == null)
            {
                return ServiceResult<string>.Failure("Payment account not found");
            }

            if (string.IsNullOrEmpty(account.MPesaConsumerKey) || string.IsNullOrEmpty(account.MPesaConsumerSecret))
            {
                return ServiceResult<string>.Failure("M-Pesa API credentials not configured");
            }

            // Create authentication header
            var authBytes = Encoding.ASCII.GetBytes($"{account.MPesaConsumerKey}:{account.MPesaConsumerSecret}");
            var authBase64 = Convert.ToBase64String(authBytes);

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authBase64);

            var response = await client.GetAsync($"{GetBaseUrl()}/oauth/v1/generate?grant_type=client_credentials");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get M-Pesa access token: {Error}", errorContent);
                return ServiceResult<string>.Failure("Failed to authenticate with M-Pesa API");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);

            if (tokenResponse != null && tokenResponse.TryGetValue("access_token", out var accessToken))
            {
                return ServiceResult<string>.Success(accessToken.ToString()!);
            }

            return ServiceResult<string>.Failure("Failed to retrieve access token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting M-Pesa access token");
            return ServiceResult<string>.Failure("An error occurred while getting access token");
        }
    }

    // Helper methods
    private string GetBaseUrl()
    {
        return _mpesaConfig.UseSandbox ? SandboxBaseUrl : ProductionBaseUrl;
    }

    private string FormatPhoneNumber(string phoneNumber)
    {
        // Remove spaces, hyphens, etc.
        phoneNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());

        // Add 254 if starts with 0
        if (phoneNumber.StartsWith("0"))
        {
            phoneNumber = "254" + phoneNumber.Substring(1);
        }

        // Add 254 if not present
        if (!phoneNumber.StartsWith("254"))
        {
            phoneNumber = "254" + phoneNumber;
        }

        return phoneNumber;
    }

    private string GeneratePassword(string shortCode, string passkey, string timestamp)
    {
        var passwordBytes = Encoding.ASCII.GetBytes($"{shortCode}{passkey}{timestamp}");
        return Convert.ToBase64String(passwordBytes);
    }
}
