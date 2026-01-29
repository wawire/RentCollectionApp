using System.Text.RegularExpressions;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Sms;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.IntegrationTests.Infrastructure;

public class TestSmsService : ISmsService
{
    private static readonly Regex CodeRegex = new Regex(@"\b\d{6}\b", RegexOptions.Compiled);
    private readonly TestOtpStore _otpStore;

    public TestSmsService(TestOtpStore otpStore)
    {
        _otpStore = otpStore;
    }

    public Task<Result> SendSmsAsync(SendSmsDto sendSmsDto)
    {
        var match = CodeRegex.Match(sendSmsDto.Message ?? string.Empty);
        if (match.Success && !string.IsNullOrWhiteSpace(sendSmsDto.PhoneNumber))
        {
            _otpStore.SetPhoneCode(sendSmsDto.PhoneNumber, match.Value);
        }

        return Task.FromResult(Result.Success());
    }

    public Task<Result> SendRentReminderAsync(int tenantId)
    {
        return Task.FromResult(Result.Success());
    }

    public Task<Result> SendPaymentReceiptAsync(int paymentId)
    {
        return Task.FromResult(Result.Success());
    }
}
