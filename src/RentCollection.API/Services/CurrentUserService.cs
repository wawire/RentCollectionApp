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

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

    public string? LandlordId => _httpContextAccessor.HttpContext?.User?.FindFirstValue("LandlordId");

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsSystemAdmin => Role == UserRoleExtensions.SystemAdmin;

    public bool IsLandlord => Role == UserRoleExtensions.Landlord;

    public bool IsCaretaker => Role == UserRoleExtensions.Caretaker;

    public bool IsAccountant => Role == UserRoleExtensions.Accountant;
}
