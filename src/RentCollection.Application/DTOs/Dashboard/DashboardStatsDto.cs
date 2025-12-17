namespace RentCollection.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TotalProperties { get; set; }
    public int TotalUnits { get; set; }
    public int OccupiedUnits { get; set; }
    public int VacantUnits { get; set; }
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public decimal TotalRentCollected { get; set; }
    public decimal TotalRentExpected { get; set; }
    public decimal CollectionRate { get; set; }
    public decimal PendingPayments { get; set; }

    // Payment Status Counts
    public int UnitsPaid { get; set; }
    public int UnitsOverdue { get; set; }
    public int UnitsPending { get; set; }
}
