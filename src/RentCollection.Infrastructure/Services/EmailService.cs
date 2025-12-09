using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName)
    {
        var resetUrl = $"{_configuration["App:BaseUrl"]}/reset-password?token={resetToken}";

        var subject = "Password Reset Request - RentCollection";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Password Reset Request</h1>
        </div>
        <div class=""content"">
            <p>Hello {userName},</p>
            <p>We received a request to reset your password for your RentCollection account.</p>
            <p>Click the button below to reset your password:</p>
            <p style=""text-align: center;"">
                <a href=""{resetUrl}"" class=""button"">Reset Password</a>
            </p>
            <p>Or copy and paste this link into your browser:</p>
            <p style=""word-break: break-all; color: #666;"">{resetUrl}</p>
            <p><strong>This link will expire in 1 hour.</strong></p>
            <p>If you didn't request a password reset, please ignore this email. Your password will remain unchanged.</p>
        </div>
        <div class=""footer"">
            <p>&copy; {DateTime.UtcNow.Year} RentCollection. All rights reserved.</p>
            <p>This is an automated email. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName, string temporaryPassword)
    {
        var loginUrl = $"{_configuration["App:BaseUrl"]}/login";

        var subject = "Welcome to RentCollection";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .credentials {{ background-color: #fff; padding: 15px; border-left: 4px solid #4CAF50; margin: 20px 0; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Welcome to RentCollection!</h1>
        </div>
        <div class=""content"">
            <p>Hello {userName},</p>
            <p>Your account has been created successfully. Here are your login credentials:</p>
            <div class=""credentials"">
                <p><strong>Email:</strong> {toEmail}</p>
                <p><strong>Temporary Password:</strong> {temporaryPassword}</p>
            </div>
            <p><strong>Important:</strong> For security reasons, please change your password after your first login.</p>
            <p style=""text-align: center;"">
                <a href=""{loginUrl}"" class=""button"">Login Now</a>
            </p>
        </div>
        <div class=""footer"">
            <p>&copy; {DateTime.UtcNow.Year} RentCollection. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendEmailVerificationAsync(string toEmail, string verificationToken, string userName)
    {
        var verifyUrl = $"{_configuration["App:BaseUrl"]}/verify-email?token={verificationToken}";

        var subject = "Verify Your Email - RentCollection";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Verify Your Email</h1>
        </div>
        <div class=""content"">
            <p>Hello {userName},</p>
            <p>Thank you for registering with RentCollection. Please verify your email address by clicking the button below:</p>
            <p style=""text-align: center;"">
                <a href=""{verifyUrl}"" class=""button"">Verify Email</a>
            </p>
            <p>Or copy and paste this link into your browser:</p>
            <p style=""word-break: break-all; color: #666;"">{verifyUrl}</p>
            <p><strong>This link will expire in 24 hours.</strong></p>
        </div>
        <div class=""footer"">
            <p>&copy; {DateTime.UtcNow.Year} RentCollection. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendRentReminderEmailAsync(string toEmail, string tenantName, string propertyName, string unitNumber, decimal rentAmount, DateTime dueDate)
    {
        var daysUntilDue = (dueDate - DateTime.UtcNow).Days;
        var urgency = daysUntilDue <= 2 ? "URGENT: " : "";

        var subject = $"{urgency}Rent Payment Reminder - Due {dueDate:dd MMM yyyy}";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .info-box {{ background-color: #fff; padding: 15px; border-left: 4px solid #2196F3; margin: 20px 0; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
        .highlight {{ color: #2196F3; font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Rent Payment Reminder</h1>
        </div>
        <div class=""content"">
            <p>Dear {tenantName},</p>
            <p>This is a friendly reminder that your rent payment is due soon.</p>
            <div class=""info-box"">
                <p><strong>Property:</strong> {propertyName}</p>
                <p><strong>Unit:</strong> {unitNumber}</p>
                <p><strong>Amount Due:</strong> <span class=""highlight"">KES {rentAmount:N2}</span></p>
                <p><strong>Due Date:</strong> <span class=""highlight"">{dueDate:dd MMMM yyyy}</span></p>
                <p><strong>Days Until Due:</strong> {daysUntilDue} day(s)</p>
            </div>
            <p>Please ensure your payment is made on or before the due date to avoid any late fees.</p>
            <p>If you have already made the payment, please disregard this reminder.</p>
            <p>Thank you for your prompt attention to this matter.</p>
        </div>
        <div class=""footer"">
            <p>&copy; {DateTime.UtcNow.Year} RentCollection. All rights reserved.</p>
            <p>Property Management Services</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendOverdueNoticeEmailAsync(string toEmail, string tenantName, string propertyName, string unitNumber, decimal overdueAmount, int daysOverdue)
    {
        var subject = $"URGENT: Overdue Rent Payment Notice - {daysOverdue} Day(s) Overdue";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f44336; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .urgent-box {{ background-color: #ffebee; padding: 15px; border-left: 4px solid #f44336; margin: 20px 0; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
        .highlight {{ color: #f44336; font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>⚠️ OVERDUE PAYMENT NOTICE</h1>
        </div>
        <div class=""content"">
            <p>Dear {tenantName},</p>
            <p><strong>This is an urgent notice regarding your overdue rent payment.</strong></p>
            <div class=""urgent-box"">
                <p><strong>Property:</strong> {propertyName}</p>
                <p><strong>Unit:</strong> {unitNumber}</p>
                <p><strong>Overdue Amount:</strong> <span class=""highlight"">KES {overdueAmount:N2}</span></p>
                <p><strong>Days Overdue:</strong> <span class=""highlight"">{daysOverdue} day(s)</span></p>
            </div>
            <p>Your rent payment is now overdue. Please settle your account immediately to avoid:</p>
            <ul>
                <li>Additional late payment fees</li>
                <li>Potential legal action</li>
                <li>Negative impact on your rental history</li>
            </ul>
            <p>If you are experiencing financial difficulties, please contact us immediately to discuss payment arrangements.</p>
            <p><strong>This matter requires your immediate attention.</strong></p>
        </div>
        <div class=""footer"">
            <p>&copy; {DateTime.UtcNow.Year} RentCollection. All rights reserved.</p>
            <p>Property Management Services</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendPaymentReceiptEmailAsync(string toEmail, string tenantName, string propertyName, string unitNumber, decimal amount, DateTime paymentDate, string referenceNumber)
    {
        var subject = $"Payment Receipt - {referenceNumber}";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .receipt-box {{ background-color: #fff; padding: 20px; border: 2px solid #4CAF50; margin: 20px 0; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
        .success {{ color: #4CAF50; font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>✓ Payment Received</h1>
        </div>
        <div class=""content"">
            <p>Dear {tenantName},</p>
            <p>Thank you! We have successfully received your rent payment.</p>
            <div class=""receipt-box"">
                <h2 style=""color: #4CAF50; text-align: center;"">PAYMENT RECEIPT</h2>
                <hr>
                <p><strong>Property:</strong> {propertyName}</p>
                <p><strong>Unit:</strong> {unitNumber}</p>
                <p><strong>Amount Paid:</strong> <span class=""success"">KES {amount:N2}</span></p>
                <p><strong>Payment Date:</strong> {paymentDate:dd MMMM yyyy}</p>
                <p><strong>Receipt Number:</strong> {referenceNumber}</p>
                <hr>
                <p style=""text-align: center; font-size: 14px; color: #666;"">Please keep this receipt for your records</p>
            </div>
            <p>We appreciate your prompt payment and thank you for being a valued tenant.</p>
            <p>If you have any questions about this payment, please don't hesitate to contact us.</p>
        </div>
        <div class=""footer"">
            <p>&copy; {DateTime.UtcNow.Year} RentCollection. All rights reserved.</p>
            <p>Property Management Services</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _configuration["Email:FromName"] ?? "RentCollection",
                _configuration["Email:FromAddress"] ?? "noreply@rentcollection.com"));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            // Connect to SMTP server
            await client.ConnectAsync(
                _configuration["Email:SmtpHost"] ?? "smtp.gmail.com",
                int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                SecureSocketOptions.StartTls);

            // Authenticate
            await client.AuthenticateAsync(
                _configuration["Email:Username"],
                _configuration["Email:Password"]);

            // Send email
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {ToEmail} with subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {ToEmail} with subject: {Subject}", toEmail, subject);
            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
    }
}
