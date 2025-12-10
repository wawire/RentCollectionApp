using RentCollection.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace RentCollection.Application.DTOs.MaintenanceRequests
{
    public class UpdateMaintenanceRequestDto
    {
        public MaintenanceRequestStatus? Status { get; set; }
        public MaintenancePriority? Priority { get; set; }
        public int? AssignedToUserId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? EstimatedCost { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? ActualCost { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}
