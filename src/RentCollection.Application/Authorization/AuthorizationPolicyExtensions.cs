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
        services.AddHttpContextAccessor();

        services.AddAuthorization(options =>
        {
            // ===== ROLE-BASED POLICIES =====

            // Single role policies
            options.AddPolicy(Policies.RequireSystemAdmin, policy =>
                policy.RequireRole(UserRole.SystemAdmin.ToString()));

            options.AddPolicy(Policies.RequireLandlord, policy =>
                policy.RequireRole(UserRole.Landlord.ToString()));

            options.AddPolicy(Policies.RequireCaretaker, policy =>
                policy.RequireRole(UserRole.Caretaker.ToString()));

            options.AddPolicy(Policies.RequireAccountant, policy =>
                policy.RequireRole(UserRole.Accountant.ToString()));

            options.AddPolicy(Policies.RequireTenant, policy =>
                policy.RequireRole(UserRole.Tenant.ToString()));

            // Combined role policies
            options.AddPolicy(Policies.RequireManagement, policy =>
                policy.RequireRole(
                    UserRole.SystemAdmin.ToString(),
                    UserRole.Landlord.ToString()));

            options.AddPolicy(Policies.RequirePropertyAccess, policy =>
                policy.RequireRole(
                    UserRole.SystemAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Caretaker.ToString()));

            options.AddPolicy(Policies.RequireFinancialAccess, policy =>
                policy.RequireRole(
                    UserRole.SystemAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Accountant.ToString()));

            options.AddPolicy(Policies.RequireOperationalAccess, policy =>
                policy.RequireRole(
                    UserRole.SystemAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Caretaker.ToString()));

            // ===== PERMISSION-BASED POLICIES =====

            // Property Management
            options.AddPolicy(Policies.CanManageProperties, policy =>
            {
                policy.RequireRole(
                    UserRole.SystemAdmin.ToString(),
                    UserRole.Landlord.ToString());
                policy.AddRequirements(new PropertyOwnerRequirement());
            });

            // Unit Management
            options.AddPolicy(Policies.CanManageUnits, policy =>
                policy.RequireRole(
                    UserRole.SystemAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Caretaker.ToString()));

            // Tenant Management
            options.AddPolicy(Policies.CanManageTenants, policy =>
                policy.RequireRole(
                    UserRole.SystemAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Caretaker.ToString()));

            // Payment Recording
            options.AddPolicy(Policies.CanRecordPayments, policy =>
                policy.RequireRole(
                    UserRole.SystemAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Caretaker.ToString()));

            // Report Access
            options.AddPolicy(Policies.CanViewReports, policy =>
                policy.RequireRole(
                    UserRole.SystemAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Accountant.ToString()));

            // SMS Sending
            options.AddPolicy(Policies.CanSendSms, policy =>
                policy.RequireRole(
                    UserRole.SystemAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Caretaker.ToString()));

            // User Management
            options.AddPolicy(Policies.CanManageUsers, policy =>
                policy.RequireRole(
                    UserRole.SystemAdmin.ToString(),
                    UserRole.Landlord.ToString()));

            // Delete Operations
            options.AddPolicy(Policies.CanDeleteData, policy =>
                policy.RequireRole(
                    UserRole.SystemAdmin.ToString(),
                    UserRole.Landlord.ToString()));

            // Price Changes
            options.AddPolicy(Policies.CanChangeRentPrices, policy =>
                policy.RequireRole(
                    UserRole.SystemAdmin.ToString(),
                    UserRole.Landlord.ToString()));

            // Financial Access
            options.AddPolicy(Policies.CanAccessFinancials, policy =>
                policy.RequireRole(
                    UserRole.SystemAdmin.ToString(),
                    UserRole.Landlord.ToString(),
                    UserRole.Accountant.ToString()));
        });

        return services;
    }
}
