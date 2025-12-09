namespace RentCollection.Application.Helpers;

/// <summary>
/// Helper methods for calculating rent payment due dates
/// </summary>
public static class PaymentDueDateHelper
{
    /// <summary>
    /// Calculate the due date for a specific month's rent based on the tenant's rent due day
    /// </summary>
    /// <param name="year">Year for the rent payment</param>
    /// <param name="month">Month for the rent payment</param>
    /// <param name="rentDueDay">Day of month when rent is due (1-31)</param>
    /// <returns>Due date for the payment</returns>
    public static DateTime CalculateDueDate(int year, int month, int rentDueDay)
    {
        // Ensure rent due day is valid (1-31)
        if (rentDueDay < 1 || rentDueDay > 31)
        {
            throw new ArgumentException("Rent due day must be between 1 and 31", nameof(rentDueDay));
        }

        // Get the last day of the month
        var lastDayOfMonth = DateTime.DaysInMonth(year, month);

        // If rent due day is greater than days in month, use last day of month
        // Example: If rent due day is 31 but it's February, use Feb 28/29
        var actualDueDay = Math.Min(rentDueDay, lastDayOfMonth);

        return new DateTime(year, month, actualDueDay);
    }

    /// <summary>
    /// Calculate the due date for the current month's rent
    /// </summary>
    /// <param name="rentDueDay">Day of month when rent is due (1-31)</param>
    /// <returns>Due date for the current month's payment</returns>
    public static DateTime CalculateCurrentMonthDueDate(int rentDueDay)
    {
        var now = DateTime.UtcNow;
        return CalculateDueDate(now.Year, now.Month, rentDueDay);
    }

    /// <summary>
    /// Calculate the due date for next month's rent
    /// </summary>
    /// <param name="rentDueDay">Day of month when rent is due (1-31)</param>
    /// <returns>Due date for next month's payment</returns>
    public static DateTime CalculateNextMonthDueDate(int rentDueDay)
    {
        var now = DateTime.UtcNow;
        var nextMonth = now.AddMonths(1);
        return CalculateDueDate(nextMonth.Year, nextMonth.Month, rentDueDay);
    }

    /// <summary>
    /// Calculate period start and end dates for a monthly rent payment
    /// </summary>
    /// <param name="dueDate">The due date of the payment</param>
    /// <returns>Tuple of (PeriodStart, PeriodEnd)</returns>
    public static (DateTime PeriodStart, DateTime PeriodEnd) CalculatePaymentPeriod(DateTime dueDate)
    {
        // Payment period is typically the month the payment is due for
        // Period start is the 1st of the month
        var periodStart = new DateTime(dueDate.Year, dueDate.Month, 1);

        // Period end is the last day of the month
        var lastDay = DateTime.DaysInMonth(dueDate.Year, dueDate.Month);
        var periodEnd = new DateTime(dueDate.Year, dueDate.Month, lastDay);

        return (periodStart, periodEnd);
    }

    /// <summary>
    /// Determine if a payment is currently overdue based on due date and status
    /// </summary>
    /// <param name="dueDate">Payment due date</param>
    /// <param name="status">Payment status</param>
    /// <returns>True if payment is pending and past due date</returns>
    public static bool IsPaymentOverdue(DateTime dueDate, Domain.Enums.PaymentStatus status)
    {
        return status == Domain.Enums.PaymentStatus.Pending && DateTime.UtcNow.Date > dueDate.Date;
    }

    /// <summary>
    /// Calculate days overdue for a pending payment
    /// </summary>
    /// <param name="dueDate">Payment due date</param>
    /// <param name="status">Payment status</param>
    /// <returns>Number of days overdue (0 if not overdue)</returns>
    public static int CalculateDaysOverdue(DateTime dueDate, Domain.Enums.PaymentStatus status)
    {
        if (!IsPaymentOverdue(dueDate, status))
        {
            return 0;
        }

        return (DateTime.UtcNow.Date - dueDate.Date).Days;
    }
}
