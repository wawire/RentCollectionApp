namespace RentCollection.Application.DTOs.Dashboard;

public class MonthlyReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalRentCollected { get; set; }
    public decimal TotalRentExpected { get; set; }
    public int NumberOfPayments { get; set; }
    public decimal CollectionRate { get; set; }
}
