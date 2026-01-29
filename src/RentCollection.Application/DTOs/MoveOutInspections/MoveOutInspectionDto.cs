using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.MoveOutInspections;

public class MoveOutInspectionDto
{
    public int Id { get; set; }

    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;

    public int UnitId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;

    public int PropertyId { get; set; }
    public string PropertyName { get; set; } = string.Empty;

    public DateTime MoveOutDate { get; set; }
    public DateTime InspectionDate { get; set; }

    public int InspectedByUserId { get; set; }
    public string InspectedByUserName { get; set; } = string.Empty;

    public MoveOutInspectionStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;

    public string OverallCondition { get; set; } = string.Empty;
    public string GeneralNotes { get; set; } = string.Empty;

    // Calculated amounts
    public decimal CleaningCharges { get; set; }
    public decimal RepairCharges { get; set; }
    public decimal UnpaidRent { get; set; }
    public decimal UnpaidUtilities { get; set; }
    public decimal OtherCharges { get; set; }
    public decimal TotalDeductions { get; set; }

    public decimal SecurityDepositHeld { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal TenantOwes { get; set; }

    // Settlement
    public bool IsSettled { get; set; }
    public DateTime? SettlementDate { get; set; }
    public string? SettlementNotes { get; set; }

    // Refund tracking
    public bool RefundProcessed { get; set; }
    public DateTime? RefundDate { get; set; }
    public string? RefundMethod { get; set; }
    public string? RefundReference { get; set; }

    public List<InspectionItemDto> InspectionItems { get; set; } = new();
    public List<InspectionPhotoDto> Photos { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}
