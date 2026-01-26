namespace RentCollection.Application.Services.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    int? UserIdInt { get; }
    string? Email { get; }
    string? Role { get; }
    string? LandlordId { get; }
    int? LandlordIdInt { get; }
    int? OrganizationId { get; }
    int? TenantId { get; }
    int? PropertyId { get; }
    bool IsAuthenticated { get; }
    bool IsPlatformAdmin { get; }
    bool IsLandlord { get; }
    bool IsCaretaker { get; }
    bool IsManager { get; }
    bool IsAccountant { get; }
    bool IsTenant { get; }
    Task<IReadOnlyCollection<int>> GetAssignedPropertyIdsAsync(CancellationToken cancellationToken = default);
}

