using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Domain.Enums;

namespace RentCollection.Application.Services.Interfaces;

public interface IUnmatchedPaymentService
{
    Task<Result<IEnumerable<UnmatchedPaymentDto>>> GetUnmatchedPaymentsAsync(UnmatchedPaymentStatus? status = null);
    Task<Result<UnmatchedPaymentDto>> UpdateStatusAsync(int id, UnmatchedPaymentStatus status);
    Task<Result<UnmatchedPaymentDto>> ResolveUnmatchedPaymentAsync(int id, ResolveUnmatchedPaymentDto dto);
}
