using Microsoft.AspNetCore.Http;
using RentCollection.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace RentCollection.Application.DTOs.MaintenanceRequests
{
    public class CreateMaintenanceRequestDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public MaintenancePriority Priority { get; set; }

        /// <summary>
        /// Photos of the maintenance issue (optional, up to 5 photos)
        /// </summary>
        public List<IFormFile>? Photos { get; set; }
    }
}
