using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;

namespace RentCollection.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApplicationDbContext _context;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
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

    public int? OrganizationId
    {
        get
        {
            var organizationId = _httpContextAccessor.HttpContext?.User?.FindFirstValue("OrganizationId");
            return int.TryParse(organizationId, out var id) ? id : null;
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

    public bool IsPlatformAdmin => Role == UserRoleExtensions.PlatformAdmin;

    public bool IsLandlord => Role == UserRoleExtensions.Landlord;

    public bool IsCaretaker => Role == UserRoleExtensions.Caretaker;

    public bool IsAccountant => Role == UserRoleExtensions.Accountant;

    public bool IsTenant => Role == UserRoleExtensions.Tenant;

    public bool IsManager => Role == UserRoleExtensions.Manager;

    public async Task<IReadOnlyCollection<int>> GetAssignedPropertyIdsAsync(CancellationToken cancellationToken = default)
    {
        if (!UserIdInt.HasValue)
        {
            return Array.Empty<int>();
        }

        var assignedPropertyIds = await _context.UserPropertyAssignments
            .AsNoTracking()
            .Where(a => a.UserId == UserIdInt.Value && a.IsActive)
            .Where(a => OrganizationId.HasValue && a.Property.OrganizationId == OrganizationId.Value)
            .Select(a => a.PropertyId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (assignedPropertyIds.Count == 0 && PropertyId.HasValue)
        {
            assignedPropertyIds.Add(PropertyId.Value);
        }

        return assignedPropertyIds;
    }
}

