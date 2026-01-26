namespace RentCollection.Application.Authorization;

/// <summary>
/// Authorization policy names used throughout the application
/// </summary>
public static class Policies
{
    // Role-based policies
    public const string RequirePlatformAdmin = "RequirePlatformAdmin";
    public const string RequireLandlord = "RequireLandlord";
    public const string RequireCaretaker = "RequireCaretaker";
    public const string RequireManager = "RequireManager";
    public const string RequireAccountant = "RequireAccountant";
    public const string RequireTenant = "RequireTenant";
    public const string RequireVerifiedUser = "RequireVerifiedUser";
    public const string RequirePasswordChangeComplete = "RequirePasswordChangeComplete";
    public const string RequireActiveOrganization = "RequireActiveOrganization";

    // Combined role policies
    public const string RequireManagement = "RequireManagement"; // PlatformAdmin + Landlord
    public const string RequirePropertyAccess = "RequirePropertyAccess"; // PlatformAdmin + Landlord + Caretaker + Manager
    public const string RequireFinancialAccess = "RequireFinancialAccess"; // PlatformAdmin + Landlord + Accountant + Manager
    public const string RequireOperationalAccess = "RequireOperationalAccess"; // PlatformAdmin + Landlord + Caretaker + Manager

    // Permission-based policies
    public const string CanManageProperties = "CanManageProperties";
    public const string CanManageUnits = "CanManageUnits";
    public const string CanManageTenants = "CanManageTenants";
    public const string CanRecordPayments = "CanRecordPayments";
    public const string CanViewReports = "CanViewReports";
    public const string CanSendSms = "CanSendSms";
    public const string CanManageUsers = "CanManageUsers";
    public const string CanDeleteData = "CanDeleteData";
    public const string CanChangeRentPrices = "CanChangeRentPrices";
    public const string CanAccessFinancials = "CanAccessFinancials";
}

/// <summary>
/// Permission names for granular access control
/// </summary>
public static class Permissions
{
    // Property permissions
    public const string CreateProperty = "Permissions.Property.Create";
    public const string ViewProperty = "Permissions.Property.View";
    public const string UpdateProperty = "Permissions.Property.Update";
    public const string DeleteProperty = "Permissions.Property.Delete";

    // Unit permissions
    public const string CreateUnit = "Permissions.Unit.Create";
    public const string ViewUnit = "Permissions.Unit.View";
    public const string UpdateUnit = "Permissions.Unit.Update";
    public const string DeleteUnit = "Permissions.Unit.Delete";
    public const string UpdateUnitPrice = "Permissions.Unit.UpdatePrice";

    // Tenant permissions
    public const string CreateTenant = "Permissions.Tenant.Create";
    public const string ViewTenant = "Permissions.Tenant.View";
    public const string UpdateTenant = "Permissions.Tenant.Update";
    public const string DeleteTenant = "Permissions.Tenant.Delete";

    // Payment permissions
    public const string CreatePayment = "Permissions.Payment.Create";
    public const string ViewPayment = "Permissions.Payment.View";
    public const string UpdatePayment = "Permissions.Payment.Update";
    public const string DeletePayment = "Permissions.Payment.Delete";

    // Report permissions
    public const string ViewFinancialReports = "Permissions.Report.ViewFinancial";
    public const string ViewOperationalReports = "Permissions.Report.ViewOperational";
    public const string ExportReports = "Permissions.Report.Export";

    // SMS permissions
    public const string SendSms = "Permissions.Sms.Send";
    public const string ViewSmsLogs = "Permissions.Sms.ViewLogs";

    // User management permissions
    public const string CreateUser = "Permissions.User.Create";
    public const string ViewUser = "Permissions.User.View";
    public const string UpdateUser = "Permissions.User.Update";
    public const string DeleteUser = "Permissions.User.Delete";
}

