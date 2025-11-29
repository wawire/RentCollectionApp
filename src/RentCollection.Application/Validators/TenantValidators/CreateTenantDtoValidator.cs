using FluentValidation;
using RentCollection.Application.DTOs.Tenants;
using RentCollection.Application.Interfaces;

namespace RentCollection.Application.Validators.TenantValidators;

public class CreateTenantDtoValidator : AbstractValidator<CreateTenantDto>
{
    private readonly IUnitRepository _unitRepository;
    private readonly ITenantRepository _tenantRepository;

    public CreateTenantDtoValidator(IUnitRepository unitRepository, ITenantRepository tenantRepository)
    {
        _unitRepository = unitRepository;
        _tenantRepository = tenantRepository;

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

        RuleFor(x => x.IdNumber)
            .MaximumLength(50).WithMessage("ID number must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.IdNumber));

        RuleFor(x => x.UnitId)
            .GreaterThan(0).WithMessage("A valid unit must be selected")
            .MustAsync(UnitExists).WithMessage("The selected unit does not exist");

        RuleFor(x => x.MonthlyRent)
            .GreaterThan(0).WithMessage("Monthly rent must be greater than 0")
            .LessThanOrEqualTo(10000000).WithMessage("Monthly rent seems unreasonably high");

        RuleFor(x => x.SecurityDeposit)
            .GreaterThanOrEqualTo(0).WithMessage("Security deposit must be 0 or greater")
            .LessThanOrEqualTo(50000000).WithMessage("Security deposit seems unreasonably high")
            .When(x => x.SecurityDeposit.HasValue);

        RuleFor(x => x.LeaseStartDate)
            .NotEmpty().WithMessage("Lease start date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(2)).WithMessage("Lease start date cannot be more than 2 years in the future");

        // Business rule: Lease end date must be after start date
        RuleFor(x => x.LeaseEndDate)
            .GreaterThan(x => x.LeaseStartDate).WithMessage("Lease end date must be after start date")
            .When(x => x.LeaseEndDate.HasValue);

        // Business rule: Validate reasonable lease duration (not more than 10 years)
        RuleFor(x => x)
            .Must(x => !x.LeaseEndDate.HasValue || (x.LeaseEndDate.Value - x.LeaseStartDate).TotalDays <= 3650)
            .WithMessage("Lease duration cannot exceed 10 years")
            .When(x => x.LeaseEndDate.HasValue);

        // Business rule: Prevent overlapping tenant leases for the same unit
        RuleFor(x => x)
            .MustAsync(NoOverlappingLeases)
            .WithMessage("Another active tenant already exists for this unit with overlapping lease dates")
            .WithName("LeaseStartDate");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }

    private async Task<bool> UnitExists(int unitId, CancellationToken cancellationToken)
    {
        return await _unitRepository.ExistsAsync(unitId);
    }

    private async Task<bool> NoOverlappingLeases(CreateTenantDto dto, CancellationToken cancellationToken)
    {
        // Check if there are any active tenants for this unit with overlapping lease periods
        var activeTenants = await _tenantRepository.GetTenantsByUnitIdAsync(dto.UnitId);

        var overlappingTenants = activeTenants
            .Where(t => t.IsActive)
            .Where(t =>
                // Case 1: New lease starts during existing lease
                (dto.LeaseStartDate >= t.LeaseStartDate && (!t.LeaseEndDate.HasValue || dto.LeaseStartDate <= t.LeaseEndDate.Value))
                ||
                // Case 2: New lease ends during existing lease
                (dto.LeaseEndDate.HasValue && dto.LeaseEndDate.Value >= t.LeaseStartDate && (!t.LeaseEndDate.HasValue || dto.LeaseEndDate.Value <= t.LeaseEndDate.Value))
                ||
                // Case 3: New lease encompasses existing lease
                (dto.LeaseStartDate <= t.LeaseStartDate && (!dto.LeaseEndDate.HasValue || (t.LeaseEndDate.HasValue && dto.LeaseEndDate.Value >= t.LeaseEndDate.Value)))
            )
            .Any();

        return !overlappingTenants;
    }
}
