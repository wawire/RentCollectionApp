using RentCollection.Domain.Common;
using RentCollection.Domain.Enums;

namespace RentCollection.Domain.Entities;

/// <summary>
/// Maps users to properties with scoped roles (e.g., Manager/Accountant/Caretaker).
/// </summary>
public class UserPropertyAssignment : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;

    public UserRole AssignmentRole { get; set; }
    public bool IsActive { get; set; } = true;
}
