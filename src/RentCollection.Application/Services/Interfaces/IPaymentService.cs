using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Payments;

namespace RentCollection.Application.Services.Interfaces;

public interface IPaymentService
{
    Task<Result<IEnumerable<PaymentDto>>> GetAllPaymentsAsync();
    Task<Result<IEnumerable<PaymentDto>>> GetPaymentsByTenantIdAsync(int tenantId);
    Task<Result<PaymentDto>> GetPaymentByIdAsync(int id);
    Task<Result<PaymentDto>> CreatePaymentAsync(CreatePaymentDto createDto);
    Task<Result> DeletePaymentAsync(int id);
    Task<Result<PaginatedList<PaymentDto>>> GetPaymentsPaginatedAsync(int pageNumber, int pageSize);
}
