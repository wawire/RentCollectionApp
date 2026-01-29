using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Organizations;

public class AssignUserToPropertyDto
{
    public int UserId { get; set; }
    public UserRole AssignmentRole { get; set; }
}
