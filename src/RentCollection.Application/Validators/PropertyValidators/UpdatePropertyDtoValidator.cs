using FluentValidation;
using RentCollection.Application.DTOs.Properties;

namespace RentCollection.Application.Validators.PropertyValidators;

public class UpdatePropertyDtoValidator : AbstractValidator<UpdatePropertyDto>
{
    public UpdatePropertyDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Property name is required")
            .MaximumLength(200).WithMessage("Property name must not exceed 200 characters");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(300).WithMessage("Location must not exceed 300 characters");

        RuleFor(x => x.TotalUnits)
            .GreaterThan(0).WithMessage("Total units must be greater than 0");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));
    }
}
