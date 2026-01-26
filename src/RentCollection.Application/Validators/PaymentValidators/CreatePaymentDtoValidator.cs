using FluentValidation;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Interfaces;

namespace RentCollection.Application.Validators.PaymentValidators;

public class CreatePaymentDtoValidator : AbstractValidator<CreatePaymentDto>
{
    private readonly ITenantRepository _tenantRepository;

    public CreatePaymentDtoValidator(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;

        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("A valid tenant must be selected")
            .MustAsync(TenantExists).WithMessage("The selected tenant does not exist")
            .MustAsync(TenantIsActive).WithMessage("The selected tenant is not active");

        RuleFor(x => x.UnitId)
            .GreaterThan(0).WithMessage("Unit ID must be a valid value")
            .When(x => x.UnitId.HasValue)
            .MustAsync(UnitMatchesTenant).WithMessage("Unit does not match the selected tenant")
            .When(x => x.UnitId.HasValue);

        RuleFor(x => x.LandlordAccountId)
            .GreaterThan(0).WithMessage("Landlord account ID must be a valid value")
            .When(x => x.LandlordAccountId.HasValue);

        // Business rule: Validate payment amounts
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .LessThanOrEqualTo(50000000).WithMessage("Payment amount seems unreasonably high")
            .ScalePrecision(2, 18).WithMessage("Amount cannot have more than 2 decimal places");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Payment date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Payment date cannot be in the future");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("A valid payment method must be selected");

        RuleFor(x => x.PeriodStart)
            .NotEmpty().WithMessage("Period start date is required");

        RuleFor(x => x.PeriodEnd)
            .GreaterThan(x => x.PeriodStart).WithMessage("Period end date must be after start date");

        RuleFor(x => x.DueDate)
            .Must((dto, dueDate) => dueDate >= dto.PeriodStart && dueDate <= dto.PeriodEnd)
            .WithMessage("Due date must fall within the payment period")
            .When(x => x.DueDate.HasValue);

        // Business rule: Validate payment period is reasonable (not more than 1 year)
        RuleFor(x => x)
            .Must(x => (x.PeriodEnd - x.PeriodStart).TotalDays <= 365)
            .WithMessage("Payment period cannot exceed 1 year")
            .WithName("PeriodEnd");

        RuleFor(x => x.TransactionReference)
            .MaximumLength(100).WithMessage("Transaction reference must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.TransactionReference));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }

    private async Task<bool> TenantExists(int tenantId, CancellationToken cancellationToken)
    {
        return await _tenantRepository.ExistsAsync(tenantId);
    }

    private async Task<bool> TenantIsActive(int tenantId, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        return tenant?.IsActive ?? false;
    }

    private async Task<bool> UnitMatchesTenant(CreatePaymentDto dto, int? unitId, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetTenantWithDetailsAsync(dto.TenantId);
        if (tenant == null)
        {
            return false;
        }

        return tenant.UnitId == unitId;
    }
}
