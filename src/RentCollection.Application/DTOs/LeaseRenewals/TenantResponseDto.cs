using System.ComponentModel.DataAnnotations;

namespace RentCollection.Application.DTOs.LeaseRenewals
{
    public class TenantResponseDto
    {
        [Required]
        public bool Accept { get; set; }

        [StringLength(1000)]
        public string? RejectionReason { get; set; }
    }
}
