using FluentValidation;
using RentCollection.Application.DTOs.Tenants;

namespace RentCollection.Application.Validators.TenantValidators;

public class CreateTenantDtoValidator : AbstractValidator<CreateTenantDto>
{
    public CreateTenantDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email is required")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^(\+?254|0)[17]\d{8}$").WithMessage("A valid Kenyan phone number is required");

        RuleFor(x => x.UnitId)
            .GreaterThan(0).WithMessage("A valid unit must be selected");

        RuleFor(x => x.MonthlyRent)
            .GreaterThan(0).WithMessage("Monthly rent must be greater than 0");

        RuleFor(x => x.LeaseStartDate)
            .NotEmpty().WithMessage("Lease start date is required");

        RuleFor(x => x.LeaseEndDate)
            .GreaterThan(x => x.LeaseStartDate).WithMessage("Lease end date must be after start date")
            .When(x => x.LeaseEndDate.HasValue);
    }
}
