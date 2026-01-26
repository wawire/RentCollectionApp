using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Utilities;

public class UtilityTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public UtilityBillingMode BillingMode { get; set; }
    public string? UnitOfMeasure { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
}

public class CreateUtilityTypeDto
{
    public string Name { get; set; } = string.Empty;
    public UtilityBillingMode BillingMode { get; set; }
    public string? UnitOfMeasure { get; set; }
    public string? Description { get; set; }
}
