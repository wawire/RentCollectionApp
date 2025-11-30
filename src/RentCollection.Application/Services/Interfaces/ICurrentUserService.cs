namespace RentCollection.Application.Services.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    string? LandlordId { get; }
    bool IsAuthenticated { get; }
    bool IsSystemAdmin { get; }
    bool IsLandlord { get; }
    bool IsCaretaker { get; }
    bool IsAccountant { get; }
}
