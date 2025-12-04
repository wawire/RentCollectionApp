using System.Security.Claims;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;

namespace RentCollection.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public int? UserIdInt
    {
        get
        {
            var userId = UserId;
            return int.TryParse(userId, out var id) ? id : null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

    public string? LandlordId => _httpContextAccessor.HttpContext?.User?.FindFirstValue("LandlordId");

    public int? LandlordIdInt
    {
        get
        {
            var landlordId = LandlordId;
            return int.TryParse(landlordId, out var id) ? id : null;
        }
    }

    public int? TenantId
    {
        get
        {
            var tenantId = _httpContextAccessor.HttpContext?.User?.FindFirstValue("TenantId");
            return int.TryParse(tenantId, out var id) ? id : null;
        }
    }

    public int? PropertyId
    {
        get
        {
            var propertyId = _httpContextAccessor.HttpContext?.User?.FindFirstValue("PropertyId");
            return int.TryParse(propertyId, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsSystemAdmin => Role == UserRoleExtensions.SystemAdmin;

    public bool IsLandlord => Role == UserRoleExtensions.Landlord;

    public bool IsCaretaker => Role == UserRoleExtensions.Caretaker;

    public bool IsAccountant => Role == UserRoleExtensions.Accountant;

    public bool IsTenant => Role == UserRoleExtensions.Tenant;
}
