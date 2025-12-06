namespace RentCollection.Application.Services.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Send a password reset email to the user
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="resetToken">Password reset token</param>
    /// <param name="userName">User's name for personalization</param>
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName);

    /// <summary>
    /// Send a generic email
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (HTML supported)</param>
    Task SendEmailAsync(string toEmail, string subject, string body);

    /// <summary>
    /// Send a welcome email to new users
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="userName">User's name</param>
    /// <param name="temporaryPassword">Temporary password</param>
    Task SendWelcomeEmailAsync(string toEmail, string userName, string temporaryPassword);

    /// <summary>
    /// Send email verification email
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="verificationToken">Email verification token</param>
    /// <param name="userName">User's name</param>
    Task SendEmailVerificationAsync(string toEmail, string verificationToken, string userName);
}
