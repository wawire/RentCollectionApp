using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.MaintenanceRequests
{
    public class MaintenanceRequestDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string TenantPhone { get; set; } = string.Empty;
        public int UnitId { get; set; }
        public string UnitNumber { get; set; } = string.Empty;
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public MaintenancePriority Priority { get; set; }
        public string PriorityName { get; set; } = string.Empty;
        public MaintenanceRequestStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;

        public List<string> PhotoUrls { get; set; } = new();

        public int? AssignedToUserId { get; set; }
        public string? AssignedToName { get; set; }

        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string? Notes { get; set; }

        // Computed properties
        public int DaysSinceCreated => (DateTime.UtcNow - CreatedAt).Days;
        public bool IsOverdue => Status != MaintenanceRequestStatus.Completed
                                 && DaysSinceCreated > GetSlaDay();

        private int GetSlaDay() => Priority switch
        {
            MaintenancePriority.Emergency => 1,
            MaintenancePriority.High => 3,
            MaintenancePriority.Medium => 7,
            MaintenancePriority.Low => 14,
            _ => 7
        };
    }
}
