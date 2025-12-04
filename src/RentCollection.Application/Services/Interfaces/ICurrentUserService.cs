namespace RentCollection.Application.Services.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    int? UserIdInt { get; }
    string? Email { get; }
    string? Role { get; }
    string? LandlordId { get; }
    int? LandlordIdInt { get; }
    int? TenantId { get; }
    int? PropertyId { get; }
    bool IsAuthenticated { get; }
    bool IsSystemAdmin { get; }
    bool IsLandlord { get; }
    bool IsCaretaker { get; }
    bool IsAccountant { get; }
    bool IsTenant { get; }
}
