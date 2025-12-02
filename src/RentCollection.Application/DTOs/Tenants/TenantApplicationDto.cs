using System.ComponentModel.DataAnnotations;

namespace RentCollection.Application.DTOs.Tenants;

/// <summary>
/// DTO for prospective tenant self-registration/application
/// </summary>
public class TenantApplicationDto
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(50)]
    public string? IdNumber { get; set; }

    [Required]
    public int UnitId { get; set; }

    [Required]
    public DateTime LeaseStartDate { get; set; }

    public DateTime? LeaseEndDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? SecurityDeposit { get; set; }

    [StringLength(1000)]
    public string? ApplicationNotes { get; set; }
}
