using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Helpers
{
    public static class LateFeeCalculator
    {
        public static decimal CalculateLateFee(
            decimal rentAmount,
            DateTime dueDate,
            DateTime currentDate,
            int gracePeriodDays,
            LateFeeType lateFeeType,
            decimal? lateFeePercentage = null,
            decimal? lateFeeAmount = null)
        {
            // If payment is not overdue (within grace period), no late fee
            var daysOverdue = (currentDate.Date - dueDate.Date).Days;
            if (daysOverdue <= gracePeriodDays)
            {
                return 0;
            }

            // Calculate late fee based on type
            if (lateFeeType == LateFeeType.Percentage && lateFeePercentage.HasValue)
            {
                return rentAmount * (lateFeePercentage.Value / 100);
            }
            else if (lateFeeType == LateFeeType.FixedAmount && lateFeeAmount.HasValue)
            {
                return lateFeeAmount.Value;
            }

            return 0;
        }

        public static decimal CalculateCurrentLateFee(Tenant tenant, DateTime dueDate)
        {
            var currentDate = DateTime.UtcNow;
            var lateFeeType = tenant.LateFeeFixedAmount.HasValue
                ? LateFeeType.FixedAmount
                : LateFeeType.Percentage;

            return CalculateLateFee(
                tenant.MonthlyRent,
                dueDate,
                currentDate,
                tenant.LateFeeGracePeriodDays,
                lateFeeType,
                tenant.LateFeePercentage,
                tenant.LateFeeFixedAmount);
        }

        public static string GetLateFeePolicy(Tenant tenant)
        {
            if (tenant.LateFeeFixedAmount.HasValue)
            {
                return $"Fixed amount of KES {tenant.LateFeeFixedAmount.Value:N2}";
            }
            return $"{tenant.LateFeePercentage * 100}% of monthly rent";
        }

        public static string GetLateFeeDetails(Tenant tenant, DateTime dueDate, DateTime currentDate)
        {
            var daysOverdue = (currentDate.Date - dueDate.Date).Days;
            var gracePeriod = tenant.LateFeeGracePeriodDays;

            if (daysOverdue <= gracePeriod)
            {
                return $"Payment is within grace period ({gracePeriod} days). No late fee applicable.";
            }

            var penaltyDays = daysOverdue - gracePeriod;
            var lateFee = CalculateCurrentLateFee(tenant, dueDate);
            return $"Payment is {daysOverdue} days overdue ({penaltyDays} days past grace period). Late fee: KES {lateFee:N2}";
        }

        public static bool IsOverdue(DateTime dueDate, DateTime currentDate, int gracePeriodDays)
        {
            var daysOverdue = (currentDate.Date - dueDate.Date).Days;
            return daysOverdue > gracePeriodDays;
        }

        public static int GetDaysOverdue(DateTime dueDate, DateTime currentDate)
        {
            var days = (currentDate.Date - dueDate.Date).Days;
            return days > 0 ? days : 0;
        }
    }
}
