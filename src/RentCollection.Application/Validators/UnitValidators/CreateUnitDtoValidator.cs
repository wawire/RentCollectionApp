using FluentValidation;
using RentCollection.Application.DTOs.Units;
using RentCollection.Application.Interfaces;

namespace RentCollection.Application.Validators.UnitValidators;

public class CreateUnitDtoValidator : AbstractValidator<CreateUnitDto>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUnitRepository _unitRepository;

    public CreateUnitDtoValidator(IPropertyRepository propertyRepository, IUnitRepository unitRepository)
    {
        _propertyRepository = propertyRepository;
        _unitRepository = unitRepository;

        RuleFor(x => x.UnitNumber)
            .NotEmpty().WithMessage("Unit number is required")
            .MaximumLength(50).WithMessage("Unit number must not exceed 50 characters");

        RuleFor(x => x.PropertyId)
            .GreaterThan(0).WithMessage("A valid property must be selected")
            .MustAsync(PropertyExists).WithMessage("The selected property does not exist");

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

        // Business rule: Prevent duplicate unit numbers in same property
        RuleFor(x => x)
            .MustAsync(UnitNumberMustBeUniqueInProperty)
            .WithMessage("A unit with this number already exists in the selected property")
            .WithName("UnitNumber");
    }

    private async Task<bool> PropertyExists(int propertyId, CancellationToken cancellationToken)
    {
        return await _propertyRepository.ExistsAsync(propertyId);
    }

    private async Task<bool> UnitNumberMustBeUniqueInProperty(CreateUnitDto dto, CancellationToken cancellationToken)
    {
        var units = await _unitRepository.GetUnitsByPropertyIdAsync(dto.PropertyId);
        var exists = units.Any(u => u.UnitNumber == dto.UnitNumber);

        return !exists;
    }
}
