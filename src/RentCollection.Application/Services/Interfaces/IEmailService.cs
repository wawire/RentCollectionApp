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

    /// <summary>
    /// Send rent payment reminder email
    /// </summary>
    /// <param name="toEmail">Tenant email address</param>
    /// <param name="tenantName">Tenant's name</param>
    /// <param name="propertyName">Property name</param>
    /// <param name="unitNumber">Unit number</param>
    /// <param name="rentAmount">Monthly rent amount</param>
    /// <param name="dueDate">Payment due date</param>
    Task SendRentReminderEmailAsync(string toEmail, string tenantName, string propertyName, string unitNumber, decimal rentAmount, DateTime dueDate);

    /// <summary>
    /// Send overdue rent notice email
    /// </summary>
    /// <param name="toEmail">Tenant email address</param>
    /// <param name="tenantName">Tenant's name</param>
    /// <param name="propertyName">Property name</param>
    /// <param name="unitNumber">Unit number</param>
    /// <param name="overdueAmount">Overdue amount</param>
    /// <param name="daysOverdue">Number of days overdue</param>
    Task SendOverdueNoticeEmailAsync(string toEmail, string tenantName, string propertyName, string unitNumber, decimal overdueAmount, int daysOverdue);

    /// <summary>
    /// Send payment receipt confirmation email
    /// </summary>
    /// <param name="toEmail">Tenant email address</param>
    /// <param name="tenantName">Tenant's name</param>
    /// <param name="propertyName">Property name</param>
    /// <param name="unitNumber">Unit number</param>
    /// <param name="amount">Payment amount</param>
    /// <param name="paymentDate">Payment date</param>
    /// <param name="referenceNumber">Receipt/reference number</param>
    Task SendPaymentReceiptEmailAsync(string toEmail, string tenantName, string propertyName, string unitNumber, decimal amount, DateTime paymentDate, string referenceNumber);
}
