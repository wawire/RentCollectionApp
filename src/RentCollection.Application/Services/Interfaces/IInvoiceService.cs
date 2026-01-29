using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Invoices;

namespace RentCollection.Application.Services.Interfaces;

public interface IInvoiceService
{
    Task<Result<IEnumerable<InvoiceDto>>> GetInvoicesAsync(int? propertyId = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<IEnumerable<InvoiceDto>>> GetInvoicesByTenantAsync(int tenantId);
    Task<Result<InvoiceDto>> GetInvoiceByIdAsync(int id);
    Task<Result<GenerateInvoicesResultDto>> GenerateMonthlyInvoicesAsync(int year, int month);
}
