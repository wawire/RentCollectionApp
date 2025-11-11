using AutoMapper;
using Microsoft.Extensions.Logging;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Repositories.Interfaces;

namespace RentCollection.Application.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        ITenantRepository tenantRepository,
        IMapper mapper,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _tenantRepository = tenantRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<PaymentDto>>> GetAllPaymentsAsync()
    {
        try
        {
            var payments = await _paymentRepository.GetAllAsync();
            var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);

            return Result<IEnumerable<PaymentDto>>.Success(paymentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all payments");
            return Result<IEnumerable<PaymentDto>>.Failure("An error occurred while retrieving payments");
        }
    }

    public async Task<Result<IEnumerable<PaymentDto>>> GetPaymentsByTenantIdAsync(int tenantId)
    {
        try
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                return Result<IEnumerable<PaymentDto>>.Failure($"Tenant with ID {tenantId} not found");
            }

            var payments = await _paymentRepository.GetPaymentsByTenantIdAsync(tenantId);
            var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);

            return Result<IEnumerable<PaymentDto>>.Success(paymentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payments for tenant {TenantId}", tenantId);
            return Result<IEnumerable<PaymentDto>>.Failure("An error occurred while retrieving payments");
        }
    }

    public async Task<Result<PaymentDto>> GetPaymentByIdAsync(int id)
    {
        try
        {
            var payment = await _paymentRepository.GetPaymentWithDetailsAsync(id);

            if (payment == null)
            {
                return Result<PaymentDto>.Failure($"Payment with ID {id} not found");
            }

            var paymentDto = _mapper.Map<PaymentDto>(payment);
            return Result<PaymentDto>.Success(paymentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment with ID {PaymentId}", id);
            return Result<PaymentDto>.Failure("An error occurred while retrieving the payment");
        }
    }

    public async Task<Result<PaymentDto>> CreatePaymentAsync(CreatePaymentDto createDto)
    {
        try
        {
            // Validate tenant exists
            var tenant = await _tenantRepository.GetTenantWithDetailsAsync(createDto.TenantId);
            if (tenant == null)
            {
                return Result<PaymentDto>.Failure($"Tenant with ID {createDto.TenantId} not found");
            }

            if (!tenant.IsActive)
            {
                return Result<PaymentDto>.Failure("Cannot create payment for an inactive tenant");
            }

            // Validate payment amount
            if (createDto.Amount <= 0)
            {
                return Result<PaymentDto>.Failure("Payment amount must be greater than zero");
            }

            // Validate period dates
            if (createDto.PeriodEnd <= createDto.PeriodStart)
            {
                return Result<PaymentDto>.Failure("Period end date must be after period start date");
            }

            var payment = _mapper.Map<Payment>(createDto);
            payment.CreatedAt = DateTime.UtcNow;
            payment.Status = PaymentStatus.Completed; // Set as completed by default

            var createdPayment = await _paymentRepository.AddAsync(payment);

            // Reload with details
            var paymentWithDetails = await _paymentRepository.GetPaymentWithDetailsAsync(createdPayment.Id);
            var paymentDto = _mapper.Map<PaymentDto>(paymentWithDetails);

            _logger.LogInformation("Payment recorded successfully: {Amount} for tenant {TenantId}",
                createdPayment.Amount, createDto.TenantId);
            return Result<PaymentDto>.Success(paymentDto, "Payment recorded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return Result<PaymentDto>.Failure("An error occurred while recording the payment");
        }
    }

    public async Task<Result> DeletePaymentAsync(int id)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(id);

            if (payment == null)
            {
                return Result.Failure($"Payment with ID {id} not found");
            }

            // Optionally, you can add business rules here
            // e.g., prevent deletion of payments older than X days

            await _paymentRepository.DeleteAsync(payment);

            _logger.LogInformation("Payment deleted successfully: {PaymentId}", id);
            return Result.Success("Payment deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment with ID {PaymentId}", id);
            return Result.Failure("An error occurred while deleting the payment");
        }
    }

    public async Task<Result<PaginatedList<PaymentDto>>> GetPaymentsPaginatedAsync(int pageNumber, int pageSize)
    {
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Max page size

            var allPayments = await _paymentRepository.GetAllAsync();
            var totalCount = allPayments.Count();

            var paginatedPayments = allPayments
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var paymentDtos = _mapper.Map<List<PaymentDto>>(paginatedPayments);
            var paginatedList = new PaginatedList<PaymentDto>(paymentDtos, totalCount, pageNumber, pageSize);

            return Result<PaginatedList<PaymentDto>>.Success(paginatedList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paginated payments");
            return Result<PaginatedList<PaymentDto>>.Failure("An error occurred while retrieving payments");
        }
    }
}
