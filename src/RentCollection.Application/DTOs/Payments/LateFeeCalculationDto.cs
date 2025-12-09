namespace RentCollection.Application.DTOs.Payments
{
    public class LateFeeCalculationDto
    {
        public decimal Amount { get; set; }
        public decimal LateFeeAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
        public DateTime DueDate { get; set; }
    }
}
