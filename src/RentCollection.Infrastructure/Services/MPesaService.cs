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
using System.Text.RegularExpressions;

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
    private readonly ICurrentUserService _currentUserService;

    // M-Pesa API URLs
    private const string SandboxBaseUrl = "https://sandbox.safaricom.co.ke";
    private const string ProductionBaseUrl = "https://api.safaricom.co.ke";

    public MPesaService(
        ApplicationDbContext context,
        ILogger<MPesaService> logger,
        IHttpClientFactory httpClientFactory,
        IOptions<MPesaConfiguration> mpesaConfig,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _mpesaConfig = mpesaConfig.Value;
        _currentUserService = currentUserService;
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
                    TransactionType = MPesaTransactionType.C2B,
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

    public async Task<ServiceResult<StkPushQueryResponseDto>> QueryStkPushStatusAsync(string checkoutRequestId)
    {
        try
        {
            _logger.LogInformation("Querying STK Push status for CheckoutRequestID: {CheckoutRequestId}", checkoutRequestId);

            var transaction = await _context.MPesaTransactions
                .Include(t => t.Tenant!)
                    .ThenInclude(t => t.Unit!)
                        .ThenInclude(u => u.Property!)
                            .ThenInclude(p => p.PaymentAccounts)
                .FirstOrDefaultAsync(t => t.CheckoutRequestID == checkoutRequestId);

            if (transaction == null)
            {
                return ServiceResult<StkPushQueryResponseDto>.Failure("Transaction not found");
            }

            if (transaction.Tenant == null || transaction.Tenant.Unit == null || transaction.Tenant.Unit.Property == null)
            {
                return ServiceResult<StkPushQueryResponseDto>.Failure("Tenant/property not found for STK query");
            }

            if (_currentUserService.IsAuthenticated && !_currentUserService.IsPlatformAdmin)
            {
                if (!_currentUserService.OrganizationId.HasValue ||
                    transaction.Tenant.Unit.Property.OrganizationId != _currentUserService.OrganizationId.Value)
                {
                    return ServiceResult<StkPushQueryResponseDto>.Failure("You do not have permission to access this transaction");
                }

                if (_currentUserService.IsTenant)
                {
                    if (!_currentUserService.TenantId.HasValue ||
                        transaction.TenantId != _currentUserService.TenantId.Value)
                    {
                        return ServiceResult<StkPushQueryResponseDto>.Failure("You do not have permission to access this transaction");
                    }
                }
                else if (_currentUserService.IsLandlord)
                {
                    if (!_currentUserService.UserIdInt.HasValue ||
                        transaction.Tenant.Unit.Property.LandlordId != _currentUserService.UserIdInt.Value)
                    {
                        return ServiceResult<StkPushQueryResponseDto>.Failure("You do not have permission to access this transaction");
                    }
                }
                else if (_currentUserService.IsManager || _currentUserService.IsAccountant || _currentUserService.IsCaretaker)
                {
                    var assignedPropertyIds = await _currentUserService.GetAssignedPropertyIdsAsync();
                    if (assignedPropertyIds.Count == 0 ||
                        !assignedPropertyIds.Contains(transaction.Tenant.Unit.PropertyId))
                    {
                        return ServiceResult<StkPushQueryResponseDto>.Failure("You do not have permission to access this transaction");
                    }
                }
                else
                {
                    return ServiceResult<StkPushQueryResponseDto>.Failure("You do not have permission to access this transaction");
                }
            }

            var paymentAccount = transaction.Tenant.Unit.Property.PaymentAccounts?
                .FirstOrDefault(pa => pa.AccountType == PaymentAccountType.MPesaPaybill && pa.IsActive);

            if (paymentAccount == null ||
                string.IsNullOrEmpty(paymentAccount.MPesaShortCode) ||
                string.IsNullOrEmpty(paymentAccount.MPesaPasskey))
            {
                return ServiceResult<StkPushQueryResponseDto>.Failure("M-Pesa account configuration not found for STK query");
            }

            var accessTokenResult = await GetAccessTokenAsync(paymentAccount.Id);
            if (!accessTokenResult.IsSuccess)
            {
                return ServiceResult<StkPushQueryResponseDto>.Failure(accessTokenResult.ErrorMessage);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var password = GeneratePassword(paymentAccount.MPesaShortCode, paymentAccount.MPesaPasskey, timestamp);

            var queryRequest = new
            {
                BusinessShortCode = paymentAccount.MPesaShortCode,
                Password = password,
                Timestamp = timestamp,
                CheckoutRequestID = checkoutRequestId
            };

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenResult.Data);

            var requestContent = new StringContent(
                JsonSerializer.Serialize(queryRequest),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync($"{GetBaseUrl()}/mpesa/stkpushquery/v1/query", requestContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("STK Push query failed: {Response}", responseContent);
                return ServiceResult<StkPushQueryResponseDto>.Failure("Failed to query STK Push status");
            }

            var queryResponse = JsonSerializer.Deserialize<StkPushQueryResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (queryResponse == null)
            {
                return ServiceResult<StkPushQueryResponseDto>.Failure("Invalid STK Push query response");
            }

            return ServiceResult<StkPushQueryResponseDto>.Success(queryResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying STK Push status");
            return ServiceResult<StkPushQueryResponseDto>.Failure("An error occurred while querying STK Push status");
        }
    }

    public async Task<ServiceResult<bool>> HandleC2BCallbackAsync(MPesaC2BCallbackDto callbackData, string? correlationId = null)
    {
        try
        {
            _logger.LogInformation("Received M-Pesa C2B callback: {CallbackData}", JsonSerializer.Serialize(callbackData));

            // Validate required fields
            if (string.IsNullOrWhiteSpace(callbackData.TransID) ||
                string.IsNullOrWhiteSpace(callbackData.BillRefNumber) ||
                callbackData.TransAmount <= 0)
            {
                await CreateUnmatchedPaymentAsync(callbackData, "Missing required fields or invalid amount", correlationId);
                return ServiceResult<bool>.Success(true, "Callback quarantined");
            }

            if (!IsValidAccountReference(callbackData.BillRefNumber))
            {
                await CreateUnmatchedPaymentAsync(callbackData, "Invalid account reference format", correlationId);
                return ServiceResult<bool>.Success(true, "Callback quarantined");
            }

            // Check for duplicate transaction
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.TransactionReference == callbackData.TransID);

            if (existingPayment != null)
            {
                _logger.LogWarning("Duplicate M-Pesa transaction received: {TransID}", callbackData.TransID);
                return ServiceResult<bool>.Success(true, "Transaction already processed");
            }

            var existingUnmatched = await _context.UnmatchedPayments
                .FirstOrDefaultAsync(p => p.TransactionReference == callbackData.TransID);

            if (existingUnmatched != null)
            {
                _logger.LogWarning("Duplicate unmatched M-Pesa transaction received: {TransID}", callbackData.TransID);
                return ServiceResult<bool>.Success(true, "Transaction already quarantined");
            }

            // Find unit by payment account number (BillRefNumber)
            // The BillRefNumber should match the unit's PaymentAccountNumber (e.g., "A101", "B205")
            var unit = await _context.Units
                .Include(u => u.Property)
                    .ThenInclude(p => p.PaymentAccounts)
                .Include(u => u.Tenants.Where(t => t.IsActive))
                .FirstOrDefaultAsync(u => u.PaymentAccountNumber == callbackData.BillRefNumber);

            if (unit == null)
            {
                _logger.LogWarning("No unit found for BillRefNumber: {BillRefNumber}", callbackData.BillRefNumber);

                // Try to find unit by unit number as fallback
                unit = await _context.Units
                    .Include(u => u.Property)
                        .ThenInclude(p => p.PaymentAccounts)
                    .Include(u => u.Tenants.Where(t => t.IsActive))
                    .FirstOrDefaultAsync(u => u.UnitNumber == callbackData.BillRefNumber);

                if (unit == null)
                {
                    await CreateUnmatchedPaymentAsync(callbackData, "Unit not found for account reference", correlationId);
                    return ServiceResult<bool>.Success(true, "Callback quarantined");
                }
            }

            // Get active tenant for the unit
            var tenant = unit.Tenants.FirstOrDefault(t => t.IsActive);
            if (tenant == null)
            {
                await CreateUnmatchedPaymentAsync(callbackData, "No active tenant found for unit", correlationId);
                return ServiceResult<bool>.Success(true, "Callback quarantined");
            }

            // Get landlord payment account
            var paymentAccount = unit.Property.PaymentAccounts
                .FirstOrDefault(pa => pa.AccountType == PaymentAccountType.MPesaPaybill &&
                                     pa.MPesaShortCode == callbackData.BusinessShortCode);

            if (paymentAccount == null)
            {
                await CreateUnmatchedPaymentAsync(callbackData, "Payment account not found for shortcode", correlationId);
                return ServiceResult<bool>.Success(true, "Callback quarantined");
            }

            // Parse transaction time
            DateTime transactionDate;
            try
            {
                // M-Pesa format: "20240315120530" (YYYYMMDDHHmmss)
                transactionDate = DateTime.ParseExact(
                    callbackData.TransTime,
                    "yyyyMMddHHmmss",
                    System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                transactionDate = DateTime.UtcNow;
                _logger.LogWarning("Failed to parse transaction time: {TransTime}, using current time", callbackData.TransTime);
            }

            // Calculate due date for the current payment period
            var dueDate = Application.Helpers.PaymentDueDateHelper.CalculateNextMonthDueDate(tenant.RentDueDay);
            var (periodStart, periodEnd) = Application.Helpers.PaymentDueDateHelper.GetPaymentPeriod(dueDate);

            // Create payment record
            var payment = new Payment
            {
                TenantId = tenant.Id,
                UnitId = unit.Id,
                LandlordAccountId = paymentAccount.Id,
                Amount = callbackData.TransAmount,
                PaymentDate = transactionDate,
                DueDate = dueDate,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                PaymentMethod = PaymentMethod.MPesa,
                Status = PaymentStatus.Pending, // Pending until landlord confirms
                UnallocatedAmount = callbackData.TransAmount,
                TransactionReference = callbackData.TransID,
                PaybillAccountNumber = callbackData.BillRefNumber,
                MPesaPhoneNumber = callbackData.MSISDN,
                Notes = $"M-Pesa C2B auto-recorded payment from {callbackData.FirstName} {callbackData.LastName}".Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Auto-created payment record from M-Pesa C2B: PaymentId={PaymentId}, TransID={TransID}, Tenant={TenantId}, Amount={Amount}",
                payment.Id, callbackData.TransID, tenant.Id, callbackData.TransAmount);

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

    private static bool IsValidAccountReference(string accountReference)
    {
        return Regex.IsMatch(accountReference, "^[A-Za-z0-9-]+$");
    }

    private async Task CreateUnmatchedPaymentAsync(MPesaC2BCallbackDto callbackData, string reason, string? correlationId)
    {
        var payload = JsonSerializer.Serialize(callbackData);
        var transactionReference = string.IsNullOrWhiteSpace(callbackData.TransID)
            ? $"UNKNOWN-{Guid.NewGuid():N}"
            : callbackData.TransID;

        int? landlordId = null;
        int? propertyId = null;
        if (!string.IsNullOrWhiteSpace(callbackData.BusinessShortCode))
        {
            var paymentAccount = await _context.LandlordPaymentAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(pa => pa.MPesaShortCode == callbackData.BusinessShortCode);

            if (paymentAccount != null)
            {
                landlordId = paymentAccount.LandlordId;
                propertyId = paymentAccount.PropertyId;
            }
        }

        var unmatched = new UnmatchedPayment
        {
            TransactionReference = transactionReference,
            Amount = callbackData.TransAmount,
            AccountReference = callbackData.BillRefNumber ?? string.Empty,
            PhoneNumber = callbackData.MSISDN,
            BusinessShortCode = callbackData.BusinessShortCode,
            CorrelationId = correlationId,
            RawPayload = payload,
            Reason = reason,
            LandlordId = landlordId,
            PropertyId = propertyId
        };

        _context.UnmatchedPayments.Add(unmatched);
        await _context.SaveChangesAsync();

        _logger.LogWarning("C2B payment quarantined: TransID={TransID}, Reason={Reason}", transactionReference, reason);
    }

    public async Task<ServiceResult<B2CResponseDto>> InitiateB2CAsync(int moveOutInspectionId, InitiateB2CDto dto)
    {
        try
        {
            // Get inspection with related entities
            var inspection = await _context.MoveOutInspections
                .Include(i => i.Tenant)
                .Include(i => i.Unit)
                    .ThenInclude(u => u.Property)
                        .ThenInclude(p => p.PaymentAccounts)
                .FirstOrDefaultAsync(i => i.Id == moveOutInspectionId);

            if (inspection == null)
            {
                return ServiceResult<B2CResponseDto>.Failure($"Inspection with ID {moveOutInspectionId} not found");
            }

            // Get payment account with M-Pesa credentials
            var paymentAccount = inspection.Unit.Property.PaymentAccounts
                .FirstOrDefault(pa => pa.AccountType == PaymentAccountType.MPesaPaybill && pa.IsActive);

            if (paymentAccount == null)
            {
                return ServiceResult<B2CResponseDto>.Failure("No M-Pesa Paybill account configured for this property");
            }

            if (string.IsNullOrEmpty(paymentAccount.MPesaConsumerKey) ||
                string.IsNullOrEmpty(paymentAccount.MPesaConsumerSecret))
            {
                return ServiceResult<B2CResponseDto>.Failure("M-Pesa API credentials not configured");
            }

            // Validate refund amount
            if (inspection.RefundAmount <= 0)
            {
                return ServiceResult<B2CResponseDto>.Failure("No refund amount to process");
            }

            if (dto.Amount != inspection.RefundAmount)
            {
                return ServiceResult<B2CResponseDto>.Failure($"Amount mismatch: expected {inspection.RefundAmount}, got {dto.Amount}");
            }

            // Get access token
            var accessTokenResult = await GetAccessTokenAsync(paymentAccount.Id);
            if (!accessTokenResult.IsSuccess)
            {
                return ServiceResult<B2CResponseDto>.Failure(accessTokenResult.ErrorMessage);
            }

            // Format phone number
            var phoneNumber = FormatPhoneNumber(dto.PhoneNumber);

            // Generate unique originator conversation ID
            var originatorConversationID = $"RC-B2C-{moveOutInspectionId}-{DateTime.UtcNow:yyyyMMddHHmmss}";

            // Prepare B2C request
            var b2cRequest = new
            {
                InitiatorName = paymentAccount.MPesaInitiatorName ?? "apiuser",
                SecurityCredential = paymentAccount.MPesaSecurityCredential ?? "",
                CommandID = dto.CommandID,
                Amount = (int)dto.Amount,
                PartyA = paymentAccount.MPesaShortCode,
                PartyB = phoneNumber,
                Remarks = dto.Remarks,
                QueueTimeOutURL = $"{_mpesaConfig.CallbackBaseUrl}/api/mpesa/b2c/timeout",
                ResultURL = $"{_mpesaConfig.CallbackBaseUrl}/api/mpesa/b2c/result",
                Occasion = dto.Occasion,
                OriginatorConversationID = originatorConversationID
            };

            var requestJson = JsonSerializer.Serialize(b2cRequest);

            // Create M-Pesa transaction record (Pending)
            var mpesaTransaction = new MPesaTransaction
            {
                TransactionType = MPesaTransactionType.B2C,
                OriginatorConversationID = originatorConversationID,
                PhoneNumber = phoneNumber,
                Amount = dto.Amount,
                AccountReference = $"Refund-{moveOutInspectionId}",
                TransactionDesc = dto.Remarks,
                TenantId = inspection.TenantId,
                MoveOutInspectionId = moveOutInspectionId,
                Status = MPesaTransactionStatus.Pending,
                CommandID = dto.CommandID,
                InitiatorName = paymentAccount.MPesaInitiatorName,
                RequestJson = requestJson,
                CreatedAt = DateTime.UtcNow
            };

            _context.MPesaTransactions.Add(mpesaTransaction);
            await _context.SaveChangesAsync();

            // Make B2C API request
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenResult.Data);

            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{GetBaseUrl()}/mpesa/b2c/v1/paymentrequest", content);

            var responseJson = await response.Content.ReadAsStringAsync();
            mpesaTransaction.ResponseJson = responseJson;

            if (!response.IsSuccessStatusCode)
            {
                mpesaTransaction.Status = MPesaTransactionStatus.Failed;
                mpesaTransaction.ResultDesc = $"HTTP {response.StatusCode}: {responseJson}";
                await _context.SaveChangesAsync();

                _logger.LogError("B2C request failed: {Response}", responseJson);
                return ServiceResult<B2CResponseDto>.Failure("Failed to initiate refund disbursement");
            }

            var b2cResponse = JsonSerializer.Deserialize<B2CResponseDto>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (b2cResponse == null)
            {
                mpesaTransaction.Status = MPesaTransactionStatus.Failed;
                mpesaTransaction.ResultDesc = "Invalid response from M-Pesa";
                await _context.SaveChangesAsync();

                return ServiceResult<B2CResponseDto>.Failure("Invalid response from M-Pesa API");
            }

            // Update transaction with response details
            mpesaTransaction.ConversationID = b2cResponse.ConversationID;
            mpesaTransaction.UpdatedAt = DateTime.UtcNow;

            if (!b2cResponse.IsSuccessful)
            {
                mpesaTransaction.Status = MPesaTransactionStatus.Failed;
                mpesaTransaction.ResultDesc = b2cResponse.ResponseDescription;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "B2C refund initiated: InspectionId={InspectionId}, ConversationID={ConversationID}, Amount={Amount}",
                moveOutInspectionId, b2cResponse.ConversationID, dto.Amount);

            return ServiceResult<B2CResponseDto>.Success(b2cResponse, "Refund disbursement initiated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating B2C disbursement for inspection {InspectionId}", moveOutInspectionId);
            return ServiceResult<B2CResponseDto>.Failure("An error occurred while initiating refund");
        }
    }

    public async Task<ServiceResult<bool>> HandleB2CCallbackAsync(B2CCallbackDto callbackData)
    {
        try
        {
            if (callbackData.Result == null)
            {
                _logger.LogWarning("Received B2C callback with null Result");
                return ServiceResult<bool>.Failure("Invalid callback data");
            }

            var result = callbackData.Result;

            // Find transaction by ConversationID
            var transaction = await _context.MPesaTransactions
                .Include(t => t.MoveOutInspection)
                .FirstOrDefaultAsync(t => t.ConversationID == result.ConversationID);

            if (transaction == null)
            {
                _logger.LogWarning("B2C callback received for unknown ConversationID: {ConversationID}", result.ConversationID);
                return ServiceResult<bool>.Failure("Transaction not found");
            }

            // Update transaction with callback data
            transaction.CallbackJson = JsonSerializer.Serialize(callbackData);
            transaction.CallbackReceivedAt = DateTime.UtcNow;
            transaction.ResultCode = result.ResultCode;
            transaction.ResultDesc = result.ResultDesc;

            // Extract transaction details from ResultParameters
            if (result.ResultParameters?.ResultParameter != null)
            {
                foreach (var param in result.ResultParameters.ResultParameter)
                {
                    if (param.Key == "TransactionID" && param.Value != null)
                    {
                        transaction.MPesaReceiptNumber = param.Value.ToString();
                    }
                    else if (param.Key == "TransactionCompletedDateTime" && param.Value != null)
                    {
                        if (DateTime.TryParse(param.Value.ToString(), out var transDate))
                        {
                            transaction.TransactionDate = transDate;
                        }
                    }
                }
            }

            // Update status based on result code
            if (result.ResultCode == 0)
            {
                transaction.Status = MPesaTransactionStatus.Completed;

                // Update inspection refund status
                if (transaction.MoveOutInspection != null)
                {
                    transaction.MoveOutInspection.RefundProcessed = true;
                    transaction.MoveOutInspection.RefundDate = DateTime.UtcNow;
                    transaction.MoveOutInspection.RefundReference = transaction.MPesaReceiptNumber;
                    transaction.MoveOutInspection.RefundMethod = "MPesa";
                    transaction.MoveOutInspection.Status = MoveOutInspectionStatus.RefundProcessed;
                }

                _logger.LogInformation(
                    "B2C refund completed: TransactionID={TransactionID}, InspectionId={InspectionId}, Amount={Amount}",
                    transaction.MPesaReceiptNumber, transaction.MoveOutInspectionId, transaction.Amount);
            }
            else
            {
                transaction.Status = MPesaTransactionStatus.Failed;
                _logger.LogWarning(
                    "B2C refund failed: ConversationID={ConversationID}, ResultCode={ResultCode}, ResultDesc={ResultDesc}",
                    result.ConversationID, result.ResultCode, result.ResultDesc);
            }

            transaction.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Success(true, "B2C callback processed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling B2C callback");
            return ServiceResult<bool>.Failure("An error occurred while processing callback");
        }
    }
}

