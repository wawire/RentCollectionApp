namespace RentCollection.Application.Services.Interfaces;

public interface IPdfService
{
    Task<byte[]> GeneratePaymentReceiptAsync(int paymentId);
    Task<byte[]> GenerateMonthlyReportAsync(int year, int month);
    Task<byte[]> GenerateTenantListAsync();
    Task<byte[]> GeneratePaymentHistoryAsync(int tenantId, DateTime? startDate = null, DateTime? endDate = null);
    Task<byte[]> GeneratePropertyPaymentHistoryAsync(int propertyId, DateTime? startDate = null, DateTime? endDate = null);
}
