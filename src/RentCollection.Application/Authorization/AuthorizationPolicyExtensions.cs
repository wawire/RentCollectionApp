using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using RentCollection.Application.Authorization.Handlers;
using RentCollection.Application.Authorization.Requirements;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Authorization;

/// <summary>
/// Extension methods for configuring authorization policies
/// </summary>
public static class AuthorizationPolicyExtensions
{
    /// <summary>
    /// Add all authorization policies for role-based access control
    /// </summary>
    public static IServiceCollection AddRentCollectionAuthorization(this IServiceCollection services)
    {
        // Register authorization handlers
        services.AddScoped<IAuthorizationHandler, PropertyOwnerHandler>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, VerifiedUserHandler>();
        services.AddScoped<IAuthorizationHandler, PasswordChangeCompleteHandler>();
        services.AddScoped<IAuthorizationHandler, ActiveOrganizationHandler>();
        services.AddHttpContextAccessor();

        services.AddAuthorization(options =>
        {
            // ===== PERMISSION-BASED DYNAMIC POLICIES =====
            // Register dynamic policies for each permission
            foreach (Permission permission in Enum.GetValues(typeof(Permission)))
            {
                options.AddPolicy($"Permission.{permission}", policy =>
                    policy.Requirements.Add(new PermissionRequirement(permission)));
            }

            // ===== ROLE-BASED POLICIES =====

            // Single role policies
            options.AddPolicy(Policies.RequirePlatformAdmin, policy =>
                policy.RequireRole(UserRole.PlatformAdmin.ToString()));

            options.AddPolicy(Policies.RequireLandlord, policy =>
                policy.RequireRole(UserRole.Landlord.ToString()));

            options.AddPolicy(Policies.RequireCaretaker, policy =>
                policy.RequireRole(UserRole.Caretaker.ToString()));

            options.AddPolicy(Policies.RequireManager, policy =>
                policy.RequireRole(UserRole.Manager.ToString()));

            options.AddPolicy(Policies.RequireAccountant, policy =>
                policy.RequireRole(UserRole.Accountant.ToString()));

            options.AddPolicy(Policies.RequireTenant, policy =>
                policy.RequireRole(UserRole.Tenant.ToString()));

            options.AddPolicy(Policies.RequireVerifiedUser, policy =>
                policy.Requirements.Add(new VerifiedUserRequirement()));

            options.AddPolicy(Policies.RequirePasswordChangeComplete, policy =>
                policy.Requirements.Add(new PasswordChangeCompleteRequirement()));

            options.AddPolicy(Policies.RequireActiveOrganization, policy =>
                policy.Requirements.Add(new ActiveOrganizationRequirement()));

            // Combined role policies
            options.AddPolicy(Policies.RequireManagement, policy =>
                policy.RequireRole(
                    UserRole.PlatformAdmin.ToString(),
                    UserRole.Landlord.ToString()));

            options.AddPolicy(Policies.RequirePropertyAccess, policy =>
                policy.RequireRole(
                    UserRole.PlatformAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Caretaker.ToString(),
                    UserRole.Manager.ToString()));

            options.AddPolicy(Policies.RequireFinancialAccess, policy =>
                policy.RequireRole(
                    UserRole.PlatformAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Accountant.ToString(),
                    UserRole.Manager.ToString()));

            options.AddPolicy(Policies.RequireOperationalAccess, policy =>
                policy.RequireRole(
                    UserRole.PlatformAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Caretaker.ToString(),
                    UserRole.Manager.ToString()));

            // ===== PERMISSION-BASED POLICIES =====

            // Property Management
            options.AddPolicy(Policies.CanManageProperties, policy =>
            {
                policy.RequireRole(
                    UserRole.PlatformAdmin.ToString(),
                    UserRole.Landlord.ToString());
                policy.AddRequirements(new PropertyOwnerRequirement());
            });

            // Unit Management
            options.AddPolicy(Policies.CanManageUnits, policy =>
                policy.RequireRole(
                    UserRole.PlatformAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Caretaker.ToString(),
                    UserRole.Manager.ToString()));

            // Tenant Management
            options.AddPolicy(Policies.CanManageTenants, policy =>
                policy.RequireRole(
                    UserRole.PlatformAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Caretaker.ToString(),
                    UserRole.Manager.ToString()));

            // Payment Recording
            options.AddPolicy(Policies.CanRecordPayments, policy =>
                policy.RequireRole(
                    UserRole.PlatformAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Caretaker.ToString(),
                    UserRole.Manager.ToString()));

            // Report Access
            options.AddPolicy(Policies.CanViewReports, policy =>
                policy.RequireRole(
                    UserRole.PlatformAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Accountant.ToString(),
                    UserRole.Manager.ToString()));

            // SMS Sending
            options.AddPolicy(Policies.CanSendSms, policy =>
                policy.RequireRole(
                    UserRole.PlatformAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Caretaker.ToString(),
                    UserRole.Manager.ToString()));

            // User Management
            options.AddPolicy(Policies.CanManageUsers, policy =>
                policy.RequireRole(
                    UserRole.PlatformAdmin.ToString(),
                    UserRole.Landlord.ToString()));

            // Delete Operations
            options.AddPolicy(Policies.CanDeleteData, policy =>
                policy.RequireRole(
                    UserRole.PlatformAdmin.ToString(),
                    UserRole.Landlord.ToString()));

            // Price Changes
            options.AddPolicy(Policies.CanChangeRentPrices, policy =>
                policy.RequireRole(
                    UserRole.PlatformAdmin.ToString(),
                    UserRole.Landlord.ToString()));

            // Financial Access
            options.AddPolicy(Policies.CanAccessFinancials, policy =>
                policy.RequireRole(
                    UserRole.PlatformAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Accountant.ToString(),
                    UserRole.Manager.ToString()));
        });

        return services;
    }
}

