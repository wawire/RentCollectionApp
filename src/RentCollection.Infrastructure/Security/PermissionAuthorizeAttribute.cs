using Microsoft.AspNetCore.Authorization;
using RentCollection.Domain.Enums;

namespace RentCollection.Infrastructure.Security
{
    /// <summary>
    /// Custom authorization attribute for permission-based access control
    /// </summary>
    public class PermissionAuthorizeAttribute : AuthorizeAttribute
    {
        public PermissionAuthorizeAttribute(Permission permission)
        {
            Policy = $"Permission.{permission}";
        }
    }
}
