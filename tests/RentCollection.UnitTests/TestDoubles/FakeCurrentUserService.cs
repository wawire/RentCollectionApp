using RentCollection.Application.Services.Interfaces;

namespace RentCollection.UnitTests.TestDoubles;

public class FakeCurrentUserService : ICurrentUserService
{
    public string? UserId { get; set; }
    public int? UserIdInt { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public string? LandlordId { get; set; }
    public int? LandlordIdInt { get; set; }
    public int? OrganizationId { get; set; }
    public int? TenantId { get; set; }
    public int? PropertyId { get; set; }
    public bool IsAuthenticated { get; set; }
    public bool IsPlatformAdmin { get; set; }
    public bool IsLandlord { get; set; }
    public bool IsCaretaker { get; set; }
    public bool IsAccountant { get; set; }
    public bool IsTenant { get; set; }
    public bool IsManager { get; set; }

    public Task<IReadOnlyCollection<int>> GetAssignedPropertyIdsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<int>>(Array.Empty<int>());
    }
}
