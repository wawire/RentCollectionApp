using System.ComponentModel.DataAnnotations;

namespace RentCollection.Application.DTOs.LeaseRenewals
{
    public class CreateLeaseRenewalDto
    {
        [Required]
        public int TenantId { get; set; }

        [Required]
        public DateTime ProposedLeaseEndDate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ProposedRentAmount { get; set; }

        [StringLength(2000)]
        public string? LandlordTerms { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}
