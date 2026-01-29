using Microsoft.AspNetCore.Http;
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

    // New payment flow methods
    /// <summary>
    /// Get payment instructions for a tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Payment instructions</returns>
    Task<Result<PaymentInstructionsDto>> GetPaymentInstructionsAsync(int tenantId);

    /// <summary>
    /// Record a payment made by a tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="dto">Payment recording data</param>
    /// <returns>Created payment</returns>
    Task<Result<PaymentDto>> RecordTenantPaymentAsync(int tenantId, TenantRecordPaymentDto dto);

    /// <summary>
    /// Get pending payments awaiting landlord confirmation
    /// </summary>
    /// <param name="propertyId">Optional property filter</param>
    /// <returns>List of pending payments</returns>
    Task<Result<IEnumerable<PaymentDto>>> GetPendingPaymentsAsync(int? propertyId = null);

    /// <summary>
    /// Confirm a payment (landlord action)
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="confirmedByUserId">User ID of confirmer</param>
    /// <returns>Updated payment</returns>
    Task<Result<PaymentDto>> ConfirmPaymentAsync(int paymentId, int confirmedByUserId);

    /// <summary>
    /// Reject a payment (landlord action)
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="reason">Reason for rejection</param>
    /// <returns>Updated payment</returns>
    Task<Result<PaymentDto>> RejectPaymentAsync(int paymentId, string reason);

    /// <summary>
    /// Upload payment proof (tenant action)
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="file">Payment proof file (image or PDF)</param>
    /// <returns>Updated payment</returns>
    Task<Result<PaymentDto>> UploadPaymentProofAsync(int paymentId, int tenantId, IFormFile file);

    /// <summary>
    /// Get overdue payments (pending payments past due date)
    /// </summary>
    /// <param name="propertyId">Optional property ID filter</param>
    /// <returns>List of overdue payments</returns>
    Task<Result<IEnumerable<PaymentDto>>> GetOverduePaymentsAsync(int? propertyId = null);

    /// <summary>
    /// Apply late fee to an overdue payment
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>Updated payment with late fee applied</returns>
    Task<Result<PaymentDto>> ApplyLateFeeAsync(int paymentId);

    /// <summary>
    /// Calculate late fee for a payment without applying it
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>Calculated late fee amount and details</returns>
    Task<Result<LateFeeCalculationDto>> CalculateLateFeeAsync(int paymentId);
}
