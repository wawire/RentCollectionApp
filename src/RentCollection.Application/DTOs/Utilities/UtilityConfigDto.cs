using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Utilities;

public class UtilityConfigDto
{
    public int Id { get; set; }
    public int UtilityTypeId { get; set; }
    public string UtilityTypeName { get; set; } = string.Empty;
    public int PropertyId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public int? UnitId { get; set; }
    public string? UnitNumber { get; set; }
    public UtilityBillingMode BillingMode { get; set; }
    public decimal? FixedAmount { get; set; }
    public decimal? Rate { get; set; }
    public decimal? SharedAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Notes { get; set; }
}

public class CreateUtilityConfigDto
{
    public int UtilityTypeId { get; set; }
    public int PropertyId { get; set; }
    public int? UnitId { get; set; }
    public UtilityBillingMode BillingMode { get; set; }
    public decimal? FixedAmount { get; set; }
    public decimal? Rate { get; set; }
    public decimal? SharedAmount { get; set; }
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow.Date;
    public DateTime? EffectiveTo { get; set; }
    public string? Notes { get; set; }
}

public class UpdateUtilityConfigDto
{
    public UtilityBillingMode BillingMode { get; set; }
    public decimal? FixedAmount { get; set; }
    public decimal? Rate { get; set; }
    public decimal? SharedAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveTo { get; set; }
    public string? Notes { get; set; }
}
