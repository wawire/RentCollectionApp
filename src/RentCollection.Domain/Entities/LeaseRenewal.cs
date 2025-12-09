using RentCollection.Domain.Common;
using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities
{
    /// <summary>
    /// Represents a lease renewal request/process
    /// </summary>
    public class LeaseRenewal : BaseEntity
    {
        public int TenantId { get; set; }
        public int UnitId { get; set; }
        public int PropertyId { get; set; }

        /// <summary>
        /// Current lease end date
        /// </summary>
        public DateTime CurrentLeaseEndDate { get; set; }

        /// <summary>
        /// Proposed new lease end date
        /// </summary>
        public DateTime ProposedLeaseEndDate { get; set; }

        /// <summary>
        /// Current monthly rent amount
        /// </summary>
        public decimal CurrentRentAmount { get; set; }

        /// <summary>
        /// Proposed new monthly rent amount (may include increase)
        /// </summary>
        public decimal ProposedRentAmount { get; set; }

        /// <summary>
        /// Rent increase percentage (if applicable)
        /// </summary>
        public decimal? RentIncreasePercentage { get; set; }

        /// <summary>
        /// Status of the renewal process
        /// </summary>
        public LeaseRenewalStatus Status { get; set; }

        /// <summary>
        /// Landlord's terms and conditions for renewal
        /// </summary>
        public string? LandlordTerms { get; set; }

        /// <summary>
        /// Reason for rejection (if rejected by either party)
        /// </summary>
        public string? RejectionReason { get; set; }

        /// <summary>
        /// Date when tenant responded (accepted/rejected)
        /// </summary>
        public DateTime? TenantResponseDate { get; set; }

        /// <summary>
        /// Date when landlord approved/rejected
        /// </summary>
        public DateTime? LandlordResponseDate { get; set; }

        /// <summary>
        /// Date when renewal was completed (new lease signed)
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Additional notes about the renewal
        /// </summary>
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual Unit Unit { get; set; } = null!;
        public virtual Property Property { get; set; } = null!;
    }
}
