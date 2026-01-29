using RentCollection.Application.Services.Interfaces;

namespace RentCollection.IntegrationTests.Infrastructure;

public class TestEmailService : IEmailService
{
    private readonly TestOtpStore _otpStore;

    public TestEmailService(TestOtpStore otpStore)
    {
        _otpStore = otpStore;
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName)
    {
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(string toEmail, string subject, string body)
    {
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string toEmail, string userName, string temporaryPassword)
    {
        return Task.CompletedTask;
    }

    public Task SendEmailVerificationAsync(string toEmail, string verificationToken, string userName)
    {
        return Task.CompletedTask;
    }

    public Task SendVerificationOtpEmailAsync(string toEmail, string userName, string otpCode)
    {
        _otpStore.SetEmailCode(toEmail, otpCode);
        return Task.CompletedTask;
    }

    public Task SendRentReminderEmailAsync(string toEmail, string tenantName, string propertyName, string unitNumber, decimal rentAmount, DateTime dueDate)
    {
        return Task.CompletedTask;
    }

    public Task SendOverdueNoticeEmailAsync(string toEmail, string tenantName, string propertyName, string unitNumber, decimal overdueAmount, int daysOverdue)
    {
        return Task.CompletedTask;
    }

    public Task SendPaymentReceiptEmailAsync(string toEmail, string tenantName, string propertyName, string unitNumber, decimal amount, DateTime paymentDate, string referenceNumber)
    {
        return Task.CompletedTask;
    }
}
