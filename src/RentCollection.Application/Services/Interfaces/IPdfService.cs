namespace RentCollection.Application.Services.Interfaces;

public interface IPdfService
{
    Task<byte[]> GeneratePaymentReceiptAsync(int paymentId);
    Task<byte[]> GenerateMonthlyReportAsync(int year, int month, int? landlordId = null, IReadOnlyCollection<int>? propertyIds = null, int? organizationId = null);
    Task<byte[]> GenerateTenantListAsync(int? landlordId = null, IReadOnlyCollection<int>? propertyIds = null, int? organizationId = null);
    Task<byte[]> GeneratePaymentHistoryAsync(int tenantId, DateTime? startDate = null, DateTime? endDate = null);
    Task<byte[]> GeneratePropertyPaymentHistoryAsync(int propertyId, DateTime? startDate = null, DateTime? endDate = null);
    Task<byte[]> GenerateMoveOutInspectionReportAsync(int inspectionId);
    Task<byte[]> GenerateOwnerStatementAsync(int landlordId, int year, int month, int? propertyId = null);
    Task<byte[]> GenerateRentRollAsync(int? landlordId = null, IReadOnlyCollection<int>? propertyIds = null, int? organizationId = null);
}
