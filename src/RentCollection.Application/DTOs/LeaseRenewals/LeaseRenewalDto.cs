using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.LeaseRenewals
{
    public class LeaseRenewalDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string TenantEmail { get; set; } = string.Empty;
        public string TenantPhone { get; set; } = string.Empty;
        public int UnitId { get; set; }
        public string UnitNumber { get; set; } = string.Empty;
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public DateTime CurrentLeaseEndDate { get; set; }
        public DateTime ProposedLeaseEndDate { get; set; }
        public decimal CurrentRentAmount { get; set; }
        public decimal ProposedRentAmount { get; set; }
        public decimal? RentIncreasePercentage { get; set; }
        public decimal RentIncrease => ProposedRentAmount - CurrentRentAmount;
        public LeaseRenewalStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? LandlordTerms { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? TenantResponseDate { get; set; }
        public DateTime? LandlordResponseDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int DaysUntilExpiry => (CurrentLeaseEndDate - DateTime.UtcNow).Days;
        public bool IsExpiringSoon => DaysUntilExpiry <= 60 && DaysUntilExpiry >= 0;
    }
}
