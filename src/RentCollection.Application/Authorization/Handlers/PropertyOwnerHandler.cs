using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RentCollection.Application.Authorization.Requirements;
using RentCollection.Domain.Enums;
using System.Security.Claims;

namespace RentCollection.Application.Authorization.Handlers;

/// <summary>
/// Authorization handler to ensure landlords can only access their own properties
/// </summary>
public class PropertyOwnerHandler : AuthorizationHandler<PropertyOwnerRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PropertyOwnerHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PropertyOwnerRequirement requirement)
    {
        // Get user role from claims
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);
        if (roleClaim == null)
        {
            return Task.CompletedTask;
        }

        var role = roleClaim.Value;

        // PlatformAdmin can access all properties
        if (role == UserRole.PlatformAdmin.ToString())
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Accountant has read-only access to all properties
        if (role == UserRole.Accountant.ToString())
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // For Landlord and Caretaker, check property ownership
        if (role == UserRole.Landlord.ToString() || role == UserRole.Caretaker.ToString())
        {
            var propertyIdClaim = context.User.FindFirst("PropertyId");

            // Get propertyId from route if available
            var httpContext = _httpContextAccessor.HttpContext;
            var routePropertyId = httpContext?.Request.RouteValues["propertyId"]?.ToString()
                ?? httpContext?.Request.RouteValues["id"]?.ToString();

            // If no property filter is being applied, allow (they'll see their own data via service layer)
            if (string.IsNullOrEmpty(routePropertyId))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // If property filter matches their property, allow
            if (propertyIdClaim != null && propertyIdClaim.Value == routePropertyId)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // PlatformAdmin without PropertyId can access all
            if (role == UserRole.PlatformAdmin.ToString())
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        // Tenant can only access their own data (handled separately)
        if (role == UserRole.Tenant.ToString())
        {
            // Tenants have very restricted access - handled by tenant-specific endpoints
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

