using Microsoft.AspNetCore.Authorization;
using RentCollection.Domain.Enums;

namespace RentCollection.Infrastructure.Security
{
    /// <summary>
    /// Authorization requirement for permission-based access
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public Permission Permission { get; }

        public PermissionRequirement(Permission permission)
        {
            Permission = permission;
        }
    }
}
