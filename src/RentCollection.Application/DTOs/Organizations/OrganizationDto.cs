using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Organizations;

public class OrganizationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public OrganizationStatus Status { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
