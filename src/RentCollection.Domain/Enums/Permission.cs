namespace RentCollection.Domain.Enums
{
    /// <summary>
    /// Fine-grained permissions for role-based access control
    /// </summary>
    public enum Permission
    {
        // User Management
        CreateUser = 1,
        ViewUsers = 2,
        UpdateUser = 3,
        DeleteUser = 4,

        // Property Management
        CreateProperty = 10,
        ViewProperties = 11,
        UpdateProperty = 12,
        DeleteProperty = 13,

        // Unit Management
        CreateUnit = 20,
        ViewUnits = 21,
        UpdateUnit = 22,
        DeleteUnit = 23,

        // Tenant Management
        CreateTenant = 30,
        ViewTenants = 31,
        UpdateTenant = 32,
        DeleteTenant = 33,

        // Payment Management
        CreatePayment = 40,
        ViewPayments = 41,
        UpdatePayment = 42,
        DeletePayment = 43,
        ProcessRefund = 44,

        // Document Management
        UploadDocument = 50,
        ViewDocuments = 51,
        VerifyDocument = 52,
        DeleteDocument = 53,

        // Reports
        ViewReports = 60,
        ExportReports = 61,

        // Notifications
        SendNotifications = 70,
        ViewNotifications = 71,

        // Maintenance Requests
        CreateMaintenanceRequest = 90,
        ViewMaintenanceRequests = 91,
        UpdateMaintenanceRequest = 92,
        DeleteMaintenanceRequest = 93,
        AssignMaintenanceRequest = 94,
        CompleteMaintenanceRequest = 95,

        // Lease Renewals
        CreateLeaseRenewal = 100,
        ViewLeaseRenewals = 101,
        UpdateLeaseRenewal = 102,
        DeleteLeaseRenewal = 103,
        RespondToLeaseRenewal = 104,
        ApproveLeaseRenewal = 105,

        // Security Deposits
        RecordSecurityDeposit = 110,
        ViewSecurityDeposits = 111,
        DeductSecurityDeposit = 112,
        RefundSecurityDeposit = 113,

        // System
        ManageSettings = 80,
        ViewAuditLogs = 81
    }
}
