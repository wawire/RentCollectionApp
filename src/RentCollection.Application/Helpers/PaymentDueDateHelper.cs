namespace RentCollection.Application.Helpers
{
    public static class PaymentDueDateHelper
    {
        public static DateTime CalculateNextMonthDueDate(int rentDueDay)
        {
            var today = DateTime.UtcNow.Date;
            var currentMonth = today.Month;
            var currentYear = today.Year;

            // Get the due date for current month
            var currentMonthDueDate = new DateTime(currentYear, currentMonth, Math.Min(rentDueDay, DateTime.DaysInMonth(currentYear, currentMonth)));

            // If we're past the due date for current month, get next month's due date
            if (today > currentMonthDueDate)
            {
                var nextMonth = currentMonth == 12 ? 1 : currentMonth + 1;
                var nextYear = currentMonth == 12 ? currentYear + 1 : currentYear;
                return new DateTime(nextYear, nextMonth, Math.Min(rentDueDay, DateTime.DaysInMonth(nextYear, nextMonth)));
            }

            return currentMonthDueDate;
        }

        public static DateTime CalculateDueDateForMonth(int year, int month, int rentDueDay)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var day = Math.Min(rentDueDay, daysInMonth);
            return new DateTime(year, month, day);
        }

        public static (DateTime periodStart, DateTime periodEnd) GetPaymentPeriod(DateTime dueDate)
        {
            // Payment period is typically the month for which rent is due
            var periodStart = new DateTime(dueDate.Year, dueDate.Month, 1);
            var periodEnd = periodStart.AddMonths(1).AddDays(-1);
            return (periodStart, periodEnd);
        }
    }
}
