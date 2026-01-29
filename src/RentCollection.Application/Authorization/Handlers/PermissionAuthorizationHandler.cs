using Microsoft.AspNetCore.Authorization;
using RentCollection.Application.Authorization.Requirements;
using RentCollection.Domain.Enums;
using System.Security.Claims;

namespace RentCollection.Application.Authorization.Handlers;

/// <summary>
/// Authorization handler that checks if user has required permission based on their role
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Get user's role from claims
        var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(roleClaim))
        {
            return Task.CompletedTask; // Fail - no role claim
        }

        if (!Enum.TryParse<UserRole>(roleClaim, out var userRole))
        {
            return Task.CompletedTask; // Fail - invalid role
        }

        // Check if user's role has the required permission
        if (HasPermission(userRole, requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Determines if a role has a specific permission
    /// </summary>
    private static bool HasPermission(UserRole role, Permission permission)
    {
        // PlatformAdmin has all permissions
        if (role == UserRole.PlatformAdmin)
        {
            return true;
        }

        // Define role-permission mappings
        return permission switch
        {
            // User Management - Admin only
            Permission.CreateUser => role == UserRole.PlatformAdmin || role == UserRole.Landlord,
            Permission.ViewUsers => role == UserRole.PlatformAdmin || role == UserRole.Landlord,
            Permission.UpdateUser => role == UserRole.PlatformAdmin || role == UserRole.Landlord,
            Permission.DeleteUser => role == UserRole.PlatformAdmin,

            // Property Management - Admin and Landlord
            Permission.CreateProperty => role == UserRole.PlatformAdmin || role == UserRole.Landlord,
            Permission.ViewProperties => role == UserRole.PlatformAdmin || role == UserRole.Landlord || role == UserRole.Caretaker || role == UserRole.Manager || role == UserRole.Accountant,
            Permission.UpdateProperty => role == UserRole.PlatformAdmin || role == UserRole.Landlord,
            Permission.DeleteProperty => role == UserRole.PlatformAdmin || role == UserRole.Landlord,

            // Unit Management
            Permission.CreateUnit => role == UserRole.PlatformAdmin || role == UserRole.Landlord,
            Permission.ViewUnits => role == UserRole.PlatformAdmin || role == UserRole.Landlord || role == UserRole.Caretaker || role == UserRole.Manager || role == UserRole.Accountant,
            Permission.UpdateUnit => role == UserRole.PlatformAdmin || role == UserRole.Landlord || role == UserRole.Caretaker || role == UserRole.Manager,
            Permission.DeleteUnit => role == UserRole.PlatformAdmin || role == UserRole.Landlord,

            // Tenant Management
            Permission.CreateTenant => role == UserRole.PlatformAdmin || role == UserRole.Landlord || role == UserRole.Caretaker || role == UserRole.Manager,
            Permission.ViewTenants => role == UserRole.PlatformAdmin || role == UserRole.Landlord || role == UserRole.Caretaker || role == UserRole.Manager || role == UserRole.Accountant,
            Permission.UpdateTenant => role == UserRole.PlatformAdmin || role == UserRole.Landlord || role == UserRole.Caretaker || role == UserRole.Manager,
            Permission.DeleteTenant => role == UserRole.PlatformAdmin || role == UserRole.Landlord,

            // Payment Management
            Permission.CreatePayment => role == UserRole.PlatformAdmin || role == UserRole.Landlord || role == UserRole.Manager,
            Permission.ViewPayments => role == UserRole.PlatformAdmin || role == UserRole.Landlord || role == UserRole.Manager || role == UserRole.Accountant || role == UserRole.Tenant,
            Permission.UpdatePayment => role == UserRole.PlatformAdmin || role == UserRole.Landlord || role == UserRole.Manager,
            Permission.DeletePayment => role == UserRole.PlatformAdmin,
            Permission.ProcessRefund => role == UserRole.PlatformAdmin || role == UserRole.Landlord,

            // Document Management
            Permission.UploadDocument => true, // All authenticated users can upload
            Permission.ViewDocuments => true, // All authenticated users can view (filtered by service layer)
            Permission.VerifyDocument => role == UserRole.PlatformAdmin || role == UserRole.Landlord || role == UserRole.Manager || role == UserRole.Accountant,
            Permission.DeleteDocument => role == UserRole.PlatformAdmin || role == UserRole.Landlord,

            // Reports - Landlord, Accountant, Admin
            Permission.ViewReports => role == UserRole.PlatformAdmin || role == UserRole.Landlord || role == UserRole.Manager || role == UserRole.Accountant,
            Permission.ExportReports => role == UserRole.PlatformAdmin || role == UserRole.Landlord || role == UserRole.Manager || role == UserRole.Accountant,

            // Notifications
            Permission.SendNotifications => role == UserRole.PlatformAdmin || role == UserRole.Landlord,
            Permission.ViewNotifications => true, // All users can view their own notifications

            // Maintenance Requests
            Permission.CreateMaintenanceRequest => role == UserRole.Tenant, // Only tenants create requests
            Permission.ViewMaintenanceRequests => true, // All users can view (filtered by service layer)
            Permission.UpdateMaintenanceRequest => role == UserRole.PlatformAdmin || role == UserRole.Landlord || role == UserRole.Caretaker,
            Permission.DeleteMaintenanceRequest => role == UserRole.PlatformAdmin || role == UserRole.Landlord,
            Permission.AssignMaintenanceRequest => role == UserRole.PlatformAdmin || role == UserRole.Landlord,
            Permission.CompleteMaintenanceRequest => role == UserRole.PlatformAdmin || role == UserRole.Landlord || role == UserRole.Caretaker || role == UserRole.Manager,

            // Lease Renewals
            Permission.CreateLeaseRenewal => role == UserRole.PlatformAdmin || role == UserRole.Landlord || role == UserRole.Manager,
            Permission.ViewLeaseRenewals => true, // All users can view (filtered by service layer)
            Permission.UpdateLeaseRenewal => role == UserRole.PlatformAdmin || role == UserRole.Landlord || role == UserRole.Manager,
            Permission.DeleteLeaseRenewal => role == UserRole.PlatformAdmin || role == UserRole.Landlord,
            Permission.RespondToLeaseRenewal => role == UserRole.Tenant, // Only tenants respond
            Permission.ApproveLeaseRenewal => role == UserRole.PlatformAdmin || role == UserRole.Landlord || role == UserRole.Manager,

            // System
            Permission.ManageSettings => role == UserRole.PlatformAdmin,
            Permission.ViewAuditLogs => role == UserRole.PlatformAdmin || role == UserRole.Landlord,

            _ => false // Deny by default
        };
    }
}

