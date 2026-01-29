using RentCollection.Application.Common.Models;

namespace RentCollection.Application.Services.Interfaces;

public interface IPaymentAllocationService
{
    Task<Result> AllocatePaymentAsync(int paymentId, int? invoiceId = null, decimal? amount = null);
    Task<Result> AllocatePaymentToOutstandingInvoicesAsync(int paymentId);
    Task<Result> ReverseAllocationsAsync(int paymentId, string? reason = null);
}
