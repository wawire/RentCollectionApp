namespace RentCollection.Application.DTOs.Payments
{
    public class LateFeeCalculationDto
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public decimal LateFeeAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CurrentDate { get; set; }
        public int GracePeriodDays { get; set; }
        public int PenaltyDays { get; set; }
        public bool IsWithinGracePeriod { get; set; }
        public string LateFeePolicy { get; set; } = string.Empty;
        public string LateFeeDetails { get; set; } = string.Empty;
    }
}
