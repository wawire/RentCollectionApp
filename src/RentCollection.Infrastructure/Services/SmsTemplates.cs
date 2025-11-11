namespace RentCollection.Infrastructure.Services;

/// <summary>
/// Helper class for generating SMS message templates
/// </summary>
public static class SmsTemplates
{
    /// <summary>
    /// Generate rent reminder SMS message
    /// </summary>
    public static string GetRentReminderMessage(
        string tenantName,
        string propertyName,
        string unitNumber,
        decimal rentAmount,
        DateTime dueDate)
    {
        return $"Dear {tenantName},\n\n" +
               $"This is a friendly reminder that your rent of KES {rentAmount:N2} for " +
               $"{propertyName} - Unit {unitNumber} is due on {dueDate:dd MMM yyyy}.\n\n" +
               $"Please make payment at your earliest convenience.\n\n" +
               $"Thank you for your cooperation.\n" +
               $"- Property Management";
    }

    /// <summary>
    /// Generate payment receipt SMS message
    /// </summary>
    public static string GetPaymentReceiptMessage(
        string tenantName,
        decimal amount,
        DateTime paymentDate,
        string referenceNumber,
        string propertyName,
        string unitNumber)
    {
        return $"Dear {tenantName},\n\n" +
               $"Thank you! We have received your payment of KES {amount:N2} " +
               $"for {propertyName} - Unit {unitNumber}.\n\n" +
               $"Payment Date: {paymentDate:dd MMM yyyy}\n" +
               $"Receipt No: {referenceNumber}\n\n" +
               $"Thank you for your prompt payment.\n" +
               $"- Property Management";
    }

    /// <summary>
    /// Generate welcome message for new tenants
    /// </summary>
    public static string GetWelcomeMessage(
        string tenantName,
        string propertyName,
        string unitNumber,
        decimal monthlyRent,
        DateTime moveInDate)
    {
        return $"Dear {tenantName},\n\n" +
               $"Welcome to {propertyName} - Unit {unitNumber}!\n\n" +
               $"Monthly Rent: KES {monthlyRent:N2}\n" +
               $"Move-in Date: {moveInDate:dd MMM yyyy}\n\n" +
               $"We wish you a comfortable stay. For any inquiries, please contact property management.\n\n" +
               $"- Property Management";
    }

    /// <summary>
    /// Generate overdue rent notice
    /// </summary>
    public static string GetOverdueNoticeMessage(
        string tenantName,
        decimal overdueAmount,
        int daysOverdue,
        string propertyName,
        string unitNumber)
    {
        return $"URGENT: Dear {tenantName},\n\n" +
               $"Your rent payment of KES {overdueAmount:N2} for {propertyName} - Unit {unitNumber} " +
               $"is {daysOverdue} day(s) overdue.\n\n" +
               $"Please settle your account immediately to avoid further action.\n\n" +
               $"Contact us for payment arrangements if needed.\n" +
               $"- Property Management";
    }

    /// <summary>
    /// Generate lease renewal reminder
    /// </summary>
    public static string GetLeaseRenewalReminderMessage(
        string tenantName,
        DateTime leaseEndDate,
        string propertyName,
        string unitNumber)
    {
        var daysUntilExpiry = (leaseEndDate - DateTime.UtcNow).Days;

        return $"Dear {tenantName},\n\n" +
               $"Your lease for {propertyName} - Unit {unitNumber} expires in {daysUntilExpiry} days " +
               $"on {leaseEndDate:dd MMM yyyy}.\n\n" +
               $"Please contact us to discuss lease renewal options.\n\n" +
               $"- Property Management";
    }

    /// <summary>
    /// Generate maintenance notification
    /// </summary>
    public static string GetMaintenanceNotificationMessage(
        string tenantName,
        string maintenanceType,
        DateTime scheduledDate,
        string propertyName,
        string unitNumber)
    {
        return $"Dear {tenantName},\n\n" +
               $"Scheduled {maintenanceType} maintenance for {propertyName} - Unit {unitNumber} " +
               $"on {scheduledDate:dd MMM yyyy}.\n\n" +
               $"Please ensure access is available. Thank you for your cooperation.\n\n" +
               $"- Property Management";
    }
}
