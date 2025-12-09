using RentCollection.Domain.Common;
using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities
{
    /// <summary>
    /// Represents a maintenance request submitted by a tenant
    /// </summary>
    public class MaintenanceRequest : BaseEntity
    {
        public int TenantId { get; set; }
        public int UnitId { get; set; }
        public int PropertyId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public MaintenancePriority Priority { get; set; }
        public MaintenanceRequestStatus Status { get; set; }

        /// <summary>
        /// Photos of the issue (comma-separated URLs)
        /// </summary>
        public string? PhotoUrls { get; set; }

        /// <summary>
        /// User assigned to handle this request (Caretaker)
        /// </summary>
        public int? AssignedToUserId { get; set; }

        /// <summary>
        /// Estimated cost for the maintenance
        /// </summary>
        public decimal? EstimatedCost { get; set; }

        /// <summary>
        /// Actual cost incurred
        /// </summary>
        public decimal? ActualCost { get; set; }

        /// <summary>
        /// Date when request was completed
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Notes from the caretaker/landlord
        /// </summary>
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual Unit Unit { get; set; } = null!;
        public virtual Property Property { get; set; } = null!;
        public virtual User? AssignedToUser { get; set; }
    }
}
