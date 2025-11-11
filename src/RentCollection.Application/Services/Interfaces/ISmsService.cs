using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Sms;

namespace RentCollection.Application.Services.Interfaces;

public interface ISmsService
{
    Task<Result> SendSmsAsync(SendSmsDto sendSmsDto);
    Task<Result> SendRentReminderAsync(int tenantId);
    Task<Result> SendPaymentReceiptAsync(int paymentId);
}
