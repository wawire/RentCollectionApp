using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
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

    // M-Pesa API URLs
    private const string SandboxBaseUrl = "https://sandbox.safaricom.co.ke";
    private const string ProductionBaseUrl = "https://api.safaricom.co.ke";

    public MPesaService(
        ApplicationDbContext context,
        ILogger<MPesaService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
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
                CallBackURL = "https://yourdomain.com/api/mpesa/callback", // TODO: Update with actual callback URL
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
            // TODO: Implement C2B callback handler
            // This would:
            // 1. Validate the callback data
            // 2. Extract transaction details (TransCode, Amount, AccountNumber, PhoneNumber)
            // 3. Match to unit using AccountNumber
            // 4. Auto-create payment record with Pending status
            // 5. Notify landlord

            _logger.LogInformation("Received M-Pesa C2B callback: {CallbackData}", JsonSerializer.Serialize(callbackData));

            return ServiceResult<bool>.Success(true, "C2B callback processed");
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
        // TODO: Get from configuration (sandbox vs production)
        return SandboxBaseUrl;
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
