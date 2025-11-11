using FluentValidation;
using RentCollection.Application.DTOs.Units;

namespace RentCollection.Application.Validators.UnitValidators;

public class UpdateUnitDtoValidator : AbstractValidator<UpdateUnitDto>
{
    public UpdateUnitDtoValidator()
    {
        RuleFor(x => x.UnitNumber)
            .NotEmpty().WithMessage("Unit number is required")
            .MaximumLength(50).WithMessage("Unit number must not exceed 50 characters");

        RuleFor(x => x.MonthlyRent)
            .GreaterThan(0).WithMessage("Monthly rent must be greater than 0")
            .LessThanOrEqualTo(10000000).WithMessage("Monthly rent seems unreasonably high");

        RuleFor(x => x.Bedrooms)
            .GreaterThanOrEqualTo(0).WithMessage("Bedrooms must be 0 or greater")
            .LessThanOrEqualTo(50).WithMessage("Number of bedrooms seems unreasonable");

        RuleFor(x => x.Bathrooms)
            .GreaterThanOrEqualTo(0).WithMessage("Bathrooms must be 0 or greater")
            .LessThanOrEqualTo(50).WithMessage("Number of bathrooms seems unreasonable");

        RuleFor(x => x.SquareFeet)
            .GreaterThan(0).WithMessage("Square feet must be greater than 0")
            .LessThanOrEqualTo(100000).WithMessage("Square feet seems unreasonably high")
            .When(x => x.SquareFeet.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
