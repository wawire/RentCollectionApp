using System.ComponentModel.DataAnnotations;

namespace RentCollection.Application.DTOs.Tenants;

/// <summary>
/// DTO for landlord to approve or reject tenant application
/// </summary>
public class ReviewApplicationDto
{
    [Required]
    public bool Approved { get; set; }

    [StringLength(500)]
    public string? ReviewNotes { get; set; }

    /// <summary>
    /// If approved, should a user account be created for the tenant?
    /// </summary>
    public bool CreateUserAccount { get; set; } = true;

    /// <summary>
    /// If creating user account, this will be the initial password
    /// </summary>
    [StringLength(100, MinimumLength = 6)]
    public string? InitialPassword { get; set; }
}
