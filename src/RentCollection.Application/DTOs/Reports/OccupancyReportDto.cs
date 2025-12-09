namespace RentCollection.Application.DTOs.Reports
{
    public class OccupancyReportDto
    {
        public DateTime ReportDate { get; set; }
        public int TotalProperties { get; set; }
        public int TotalUnits { get; set; }
        public int OccupiedUnits { get; set; }
        public int VacantUnits { get; set; }
        public decimal OverallOccupancyRate { get; set; }
        public decimal PotentialMonthlyRevenue { get; set; }
        public decimal ActualMonthlyRevenue { get; set; }
        public decimal VacancyLoss { get; set; }
        public List<PropertyOccupancyDto> PropertiesBreakdown { get; set; } = new();
    }

    public class PropertyOccupancyDto
    {
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public int TotalUnits { get; set; }
        public int OccupiedUnits { get; set; }
        public int VacantUnits { get; set; }
        public decimal OccupancyRate { get; set; }
        public decimal PotentialMonthlyRevenue { get; set; }
        public decimal ActualMonthlyRevenue { get; set; }
        public decimal VacancyLoss { get; set; }
        public List<VacantUnitDto> VacantUnitsList { get; set; } = new();
    }

    public class VacantUnitDto
    {
        public int UnitId { get; set; }
        public string UnitNumber { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public decimal RentAmount { get; set; }
        public int DaysVacant { get; set; }
        public DateTime? LastOccupiedDate { get; set; }
    }
}
