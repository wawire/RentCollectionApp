namespace RentCollection.Application.DTOs.Reports
{
    public class ProfitLossReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Period { get; set; } = string.Empty;

        // Income
        public decimal TotalRentCollected { get; set; }
        public decimal TotalRentExpected { get; set; }
        public decimal CollectionRate { get; set; }
        public decimal SecurityDepositsReceived { get; set; }
        public decimal LateFees { get; set; }
        public decimal TotalIncome { get; set; }

        // Expenses
        public decimal MaintenanceExpenses { get; set; }
        public decimal UtilitiesExpenses { get; set; }
        public decimal PropertyManagementFees { get; set; }
        public decimal TaxesAndInsurance { get; set; }
        public decimal OtherExpenses { get; set; }
        public decimal TotalExpenses { get; set; }

        // Summary
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }

        // Breakdown by property
        public List<PropertyProfitLossDto> PropertiesBreakdown { get; set; } = new();
    }

    public class PropertyProfitLossDto
    {
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public decimal RentCollected { get; set; }
        public decimal RentExpected { get; set; }
        public decimal CollectionRate { get; set; }
        public decimal LateFees { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal Expenses { get; set; }
        public decimal NetProfit { get; set; }
        public int TotalUnits { get; set; }
        public int OccupiedUnits { get; set; }
        public decimal OccupancyRate { get; set; }
    }
}
