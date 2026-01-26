using RentCollection.Domain.Common;
using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities
{
    /// <summary>
    /// Tracks move-out inspection for security deposit settlement
    /// </summary>
    public class MoveOutInspection : BaseEntity
    {
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        public int UnitId { get; set; }
        public Unit Unit { get; set; } = null!;

        public int PropertyId { get; set; }
        public Property Property { get; set; } = null!;

        public DateTime MoveOutDate { get; set; }
        public DateTime InspectionDate { get; set; }

        public int InspectedByUserId { get; set; }
        public User InspectedBy { get; set; } = null!;

        public MoveOutInspectionStatus Status { get; set; }

        // Overall condition assessment
        public string OverallCondition { get; set; } = string.Empty; // Excellent, Good, Fair, Poor
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
        public decimal TenantOwes { get; set; } // If deductions > deposit

        // Settlement
        public bool IsSettled { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string? SettlementNotes { get; set; }

        // Refund tracking
        public bool RefundProcessed { get; set; }
        public DateTime? RefundDate { get; set; }
        public string? RefundMethod { get; set; } // MPesa, Bank Transfer, Cash
        public string? RefundReference { get; set; } // M-Pesa transaction ID

        // Collections
        public ICollection<InspectionItem> InspectionItems { get; set; } = new List<InspectionItem>();
        public ICollection<InspectionPhoto> Photos { get; set; } = new List<InspectionPhoto>();
    }

    /// <summary>
    /// Individual item checked during move-out inspection
    /// </summary>
    public class InspectionItem : BaseEntity
    {
        public int MoveOutInspectionId { get; set; }
        public MoveOutInspection MoveOutInspection { get; set; } = null!;

        public InspectionCategory Category { get; set; }
        public string ItemName { get; set; } = string.Empty; // e.g., "Living Room Wall", "Kitchen Sink"
        public string MoveInCondition { get; set; } = string.Empty; // Condition when tenant moved in
        public string MoveOutCondition { get; set; } = string.Empty; // Condition now

        public bool IsDamaged { get; set; }
        public string? DamageDescription { get; set; }
        public decimal EstimatedRepairCost { get; set; }

        public string? Notes { get; set; }

        // Photos
        public ICollection<InspectionPhoto> Photos { get; set; } = new List<InspectionPhoto>();
    }

    /// <summary>
    /// Photo documentation for inspection
    /// </summary>
    public class InspectionPhoto : BaseEntity
    {
        public int MoveOutInspectionId { get; set; }
        public MoveOutInspection MoveOutInspection { get; set; } = null!;

        public int? InspectionItemId { get; set; }
        public InspectionItem? InspectionItem { get; set; }

        public string PhotoUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public PhotoType PhotoType { get; set; } // MoveIn, MoveOut, Damage
        public DateTime TakenAt { get; set; }
    }
}
