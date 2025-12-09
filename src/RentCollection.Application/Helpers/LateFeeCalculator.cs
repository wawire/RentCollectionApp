using RentCollection.Domain.Entities;

namespace RentCollection.Application.Helpers;

/// <summary>
/// Helper methods for calculating late payment penalties
/// </summary>
public static class LateFeeCalculator
{
    /// <summary>
    /// Calculate late fee for a payment based on tenant's penalty configuration
    /// </summary>
    /// <param name="tenant">Tenant with late fee configuration</param>
    /// <param name="dueDate">Payment due date</param>
    /// <param name="paymentDate">Actual payment date (or current date if still pending)</param>
    /// <returns>Late fee amount (0 if within grace period or not late)</returns>
    public static decimal CalculateLateFee(Tenant tenant, DateTime dueDate, DateTime paymentDate)
    {
        // Calculate days overdue
        var daysOverdue = (paymentDate.Date - dueDate.Date).Days;

        // If not overdue or within grace period, no penalty
        if (daysOverdue <= tenant.LateFeeGracePeriodDays)
        {
            return 0;
        }

        // Calculate actual days for penalty (subtract grace period)
        var penaltyDays = daysOverdue - tenant.LateFeeGracePeriodDays;

        // If fixed amount is set, use it (takes precedence over percentage)
        if (tenant.LateFeeFixedAmount.HasValue && tenant.LateFeeFixedAmount.Value > 0)
        {
            return tenant.LateFeeFixedAmount.Value;
        }

        // Otherwise, calculate as percentage of monthly rent
        return tenant.MonthlyRent * tenant.LateFeePercentage;
    }

    /// <summary>
    /// Calculate current late fee for a pending payment
    /// </summary>
    /// <param name="tenant">Tenant with late fee configuration</param>
    /// <param name="dueDate">Payment due date</param>
    /// <returns>Current late fee amount (0 if not applicable)</returns>
    public static decimal CalculateCurrentLateFee(Tenant tenant, DateTime dueDate)
    {
        return CalculateLateFee(tenant, dueDate, DateTime.UtcNow);
    }

    /// <summary>
    /// Check if a payment should have a late fee applied
    /// </summary>
    /// <param name="tenant">Tenant with late fee configuration</param>
    /// <param name="dueDate">Payment due date</param>
    /// <param name="paymentDate">Actual payment date</param>
    /// <returns>True if late fee should be applied</returns>
    public static bool ShouldApplyLateFee(Tenant tenant, DateTime dueDate, DateTime paymentDate)
    {
        var daysOverdue = (paymentDate.Date - dueDate.Date).Days;
        return daysOverdue > tenant.LateFeeGracePeriodDays;
    }

    /// <summary>
    /// Get late fee details as a formatted string for display
    /// </summary>
    /// <param name="tenant">Tenant with late fee configuration</param>
    /// <param name="dueDate">Payment due date</param>
    /// <param name="paymentDate">Actual payment date</param>
    /// <returns>Late fee details message</returns>
    public static string GetLateFeeDetails(Tenant tenant, DateTime dueDate, DateTime paymentDate)
    {
        var daysOverdue = (paymentDate.Date - dueDate.Date).Days;

        if (daysOverdue <= 0)
        {
            return "No late fee - payment not overdue";
        }

        if (daysOverdue <= tenant.LateFeeGracePeriodDays)
        {
            var daysRemaining = tenant.LateFeeGracePeriodDays - daysOverdue;
            return $"Within {tenant.LateFeeGracePeriodDays}-day grace period ({daysRemaining} days remaining)";
        }

        var penaltyDays = daysOverdue - tenant.LateFeeGracePeriodDays;
        var lateFee = CalculateLateFee(tenant, dueDate, paymentDate);

        if (tenant.LateFeeFixedAmount.HasValue && tenant.LateFeeFixedAmount.Value > 0)
        {
            return $"Fixed late fee of KES {lateFee:N2} applied ({penaltyDays} days past grace period)";
        }
        else
        {
            var percentage = tenant.LateFeePercentage * 100;
            return $"{percentage}% late fee applied: KES {lateFee:N2} ({penaltyDays} days past grace period)";
        }
    }

    /// <summary>
    /// Calculate late fee description for tenant portal display
    /// </summary>
    /// <param name="tenant">Tenant with late fee configuration</param>
    /// <returns>Late fee policy description</returns>
    public static string GetLateFeePolicy(Tenant tenant)
    {
        var gracePeriodText = tenant.LateFeeGracePeriodDays == 1
            ? "1 day"
            : $"{tenant.LateFeeGracePeriodDays} days";

        if (tenant.LateFeeFixedAmount.HasValue && tenant.LateFeeFixedAmount.Value > 0)
        {
            return $"Late fee: KES {tenant.LateFeeFixedAmount.Value:N2} after {gracePeriodText} grace period";
        }
        else
        {
            var percentage = tenant.LateFeePercentage * 100;
            return $"Late fee: {percentage}% of rent after {gracePeriodText} grace period";
        }
    }
}
