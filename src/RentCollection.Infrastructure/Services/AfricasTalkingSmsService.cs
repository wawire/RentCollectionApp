using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Sms;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using System.Text.Json;

namespace RentCollection.Infrastructure.Services;

public class AfricasTalkingSmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AfricasTalkingSmsService> _logger;
    private readonly ApplicationDbContext _context;
    private readonly string _username;
    private readonly string _apiKey;
    private readonly string _senderId;
    private const string SMS_API_URL = "https://api.sandbox.africastalking.com/version1/messaging";

    public AfricasTalkingSmsService(
        IConfiguration configuration,
        ILogger<AfricasTalkingSmsService> logger,
        ApplicationDbContext context)
    {
        _configuration = configuration;
        _logger = logger;
        _context = context;

        _username = _configuration["AfricasTalking:Username"] ?? "sandbox";
        _apiKey = _configuration["AfricasTalking:ApiKey"] ?? throw new InvalidOperationException("Africa's Talking API Key not configured");
        _senderId = _configuration["AfricasTalking:SenderId"] ?? "RENTPAY";
    }

    public async Task<Result> SendSmsAsync(SendSmsDto sendSmsDto)
    {
        try
        {
            _logger.LogInformation("Sending SMS to {PhoneNumber}", sendSmsDto.PhoneNumber);

            var client = new RestClient(SMS_API_URL);
            var request = new RestRequest("", Method.Post);

            request.AddHeader("apiKey", _apiKey);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            request.AddParameter("username", _username);
            request.AddParameter("to", NormalizePhoneNumber(sendSmsDto.PhoneNumber));
            request.AddParameter("message", sendSmsDto.Message);
            request.AddParameter("from", _senderId);

            var response = await client.ExecuteAsync(request);

            // Log SMS to database
            var smsLog = new SmsLog
            {
                PhoneNumber = sendSmsDto.PhoneNumber,
                Message = sendSmsDto.Message,
                Status = response.IsSuccessful ? SmsStatus.Sent : SmsStatus.Failed,
                SentAt = DateTime.UtcNow,
                Response = response.Content,
                TenantId = sendSmsDto.TenantId
            };

            _context.SmsLogs.Add(smsLog);
            await _context.SaveChangesAsync();

            if (response.IsSuccessful)
            {
                _logger.LogInformation("SMS sent successfully to {PhoneNumber}", sendSmsDto.PhoneNumber);
                return Result.Success("SMS sent successfully");
            }
            else
            {
                _logger.LogError("Failed to send SMS: {Error}", response.ErrorMessage);
                return Result.Failure($"Failed to send SMS: {response.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", sendSmsDto.PhoneNumber);

            // Log failed attempt to database
            var smsLog = new SmsLog
            {
                PhoneNumber = sendSmsDto.PhoneNumber,
                Message = sendSmsDto.Message,
                Status = SmsStatus.Failed,
                SentAt = DateTime.UtcNow,
                Response = ex.Message,
                TenantId = sendSmsDto.TenantId
            };

            _context.SmsLogs.Add(smsLog);
            await _context.SaveChangesAsync();

            return Result.Failure($"Error sending SMS: {ex.Message}");
        }
    }

    public async Task<Result> SendRentReminderAsync(int tenantId)
    {
        try
        {
            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                .ThenInclude(u => u.Property)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
            {
                return Result.Failure("Tenant not found");
            }

            if (string.IsNullOrWhiteSpace(tenant.PhoneNumber))
            {
                return Result.Failure("Tenant phone number not available");
            }

            var message = SmsTemplates.GetRentReminderMessage(
                tenant.FullName,
                tenant.Unit.Property.Name,
                tenant.Unit.UnitNumber,
                tenant.MonthlyRent,
                DateTime.UtcNow.AddDays(5) // Due in 5 days
            );

            var sendSmsDto = new SendSmsDto
            {
                PhoneNumber = tenant.PhoneNumber,
                Message = message,
                TenantId = tenantId
            };

            return await SendSmsAsync(sendSmsDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending rent reminder to tenant {TenantId}", tenantId);
            return Result.Failure($"Error sending rent reminder: {ex.Message}");
        }
    }

    public async Task<Result> SendPaymentReceiptAsync(int paymentId)
    {
        try
        {
            var payment = await _context.Payments
                .Include(p => p.Tenant)
                .ThenInclude(t => t.Unit)
                .ThenInclude(u => u.Property)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return Result.Failure("Payment not found");
            }

            if (string.IsNullOrWhiteSpace(payment.Tenant.PhoneNumber))
            {
                return Result.Failure("Tenant phone number not available");
            }

            var message = SmsTemplates.GetPaymentReceiptMessage(
                payment.Tenant.FullName,
                payment.Amount,
                payment.PaymentDate,
                payment.ReferenceNumber ?? payment.Id.ToString(),
                payment.Tenant.Unit.Property.Name,
                payment.Tenant.Unit.UnitNumber
            );

            var sendSmsDto = new SendSmsDto
            {
                PhoneNumber = payment.Tenant.PhoneNumber,
                Message = message,
                TenantId = payment.TenantId
            };

            return await SendSmsAsync(sendSmsDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment receipt for payment {PaymentId}", paymentId);
            return Result.Failure($"Error sending payment receipt: {ex.Message}");
        }
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        // Remove all non-numeric characters
        var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());

        // Add Kenya country code if not present
        if (!cleaned.StartsWith("254") && cleaned.StartsWith("0"))
        {
            cleaned = "254" + cleaned[1..];
        }
        else if (!cleaned.StartsWith("254") && !cleaned.StartsWith("+"))
        {
            cleaned = "254" + cleaned;
        }

        return "+" + cleaned.TrimStart('+');
    }
}
