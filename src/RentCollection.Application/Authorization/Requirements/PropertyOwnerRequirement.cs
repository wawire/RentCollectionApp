using Microsoft.AspNetCore.Authorization;

namespace RentCollection.Application.Authorization.Requirements;

/// <summary>
/// Authorization requirement to ensure user can only access their own properties
/// </summary>
public class PropertyOwnerRequirement : IAuthorizationRequirement
{
    public PropertyOwnerRequirement() { }
}
