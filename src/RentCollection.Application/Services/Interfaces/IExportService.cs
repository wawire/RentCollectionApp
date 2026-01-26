namespace RentCollection.Application.Services.Interfaces;

public interface IExportService
{
    Task<string> ExportPaymentsAsync(int? propertyId, DateTime? startDate, DateTime? endDate);
    Task<string> ExportInvoicesAsync(int? propertyId, DateTime? startDate, DateTime? endDate);
    Task<string> ExportExpensesAsync(int? propertyId, DateTime? startDate, DateTime? endDate);
    Task<string> ExportArrearsAsync(int? propertyId);
}
