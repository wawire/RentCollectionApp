using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Application.Validators.PaymentValidators;

public class CreatePaymentDtoValidator : AbstractValidator<CreatePaymentDto>
{
    private readonly ApplicationDbContext _context;

    public CreatePaymentDtoValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("A valid tenant must be selected")
            .MustAsync(TenantExists).WithMessage("The selected tenant does not exist")
            .MustAsync(TenantIsActive).WithMessage("The selected tenant is not active");

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

        // Business rule: Validate payment period is reasonable (not more than 1 year)
        RuleFor(x => x)
            .Must(x => (x.PeriodEnd - x.PeriodStart).TotalDays <= 365)
            .WithMessage("Payment period cannot exceed 1 year")
            .WithName("PeriodEnd");

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100).WithMessage("Reference number must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }

    private async Task<bool> TenantExists(int tenantId, CancellationToken cancellationToken)
    {
        return await _context.Tenants.AnyAsync(t => t.Id == tenantId, cancellationToken);
    }

    private async Task<bool> TenantIsActive(int tenantId, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants.FindAsync(new object[] { tenantId }, cancellationToken);
        return tenant?.IsActive ?? false;
    }
}
