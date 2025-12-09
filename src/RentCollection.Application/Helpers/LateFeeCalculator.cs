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
