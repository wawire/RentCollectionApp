namespace RentCollection.Application.Services.Interfaces;

public interface IPdfService
{
    Task<byte[]> GeneratePaymentReceiptAsync(int paymentId);
    Task<byte[]> GenerateMonthlyReportAsync(int year, int month);
    Task<byte[]> GenerateTenantListAsync();
}
