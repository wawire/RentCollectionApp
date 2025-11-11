namespace RentCollection.Application.DTOs.Tenants;

public class UpdateTenantDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? IdNumber { get; set; }
    public DateTime LeaseStartDate { get; set; }
    public DateTime? LeaseEndDate { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal? SecurityDeposit { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}
