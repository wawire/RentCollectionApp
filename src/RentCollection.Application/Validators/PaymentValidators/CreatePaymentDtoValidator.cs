using FluentValidation;
using RentCollection.Application.DTOs.Payments;

namespace RentCollection.Application.Validators.PaymentValidators;

public class CreatePaymentDtoValidator : AbstractValidator<CreatePaymentDto>
{
    public CreatePaymentDtoValidator()
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("A valid tenant must be selected");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Payment date is required");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("A valid payment method must be selected");

        RuleFor(x => x.PeriodStart)
            .NotEmpty().WithMessage("Period start date is required");

        RuleFor(x => x.PeriodEnd)
            .GreaterThan(x => x.PeriodStart).WithMessage("Period end date must be after start date");
    }
}
