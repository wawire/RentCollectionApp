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
