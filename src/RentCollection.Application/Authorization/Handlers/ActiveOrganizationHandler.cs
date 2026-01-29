using Microsoft.AspNetCore.Authorization;
using RentCollection.Application.Authorization.Requirements;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Authorization.Handlers;

public class ActiveOrganizationHandler : AuthorizationHandler<ActiveOrganizationRequirement>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IOrganizationService _organizationService;

    public ActiveOrganizationHandler(ICurrentUserService currentUserService, IOrganizationService organizationService)
    {
        _currentUserService = currentUserService;
        _organizationService = organizationService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ActiveOrganizationRequirement requirement)
    {
        if (_currentUserService.IsPlatformAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        if (!_currentUserService.OrganizationId.HasValue)
        {
            return;
        }

        var organization = await _organizationService.GetOrganizationByIdAsync(_currentUserService.OrganizationId.Value);
        if (!organization.IsSuccess || organization.Data == null)
        {
            return;
        }

        if (organization.Data.Status == OrganizationStatus.Active)
        {
            context.Succeed(requirement);
        }
    }
}
