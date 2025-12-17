using AutoMapper;
using RentCollection.Application.DTOs.Properties;
using RentCollection.Application.DTOs.Units;
using RentCollection.Application.DTOs.Tenants;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Domain.Entities;

namespace RentCollection.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Property mappings
        CreateMap<Property, PropertyDto>()
            .ForMember(dest => dest.OccupiedUnits, opt => opt.MapFrom(src => src.Units.Count(u => u.IsOccupied)))
            .ForMember(dest => dest.VacantUnits, opt => opt.MapFrom(src => src.Units.Count(u => !u.IsOccupied)));
        CreateMap<CreatePropertyDto, Property>();
        CreateMap<UpdatePropertyDto, Property>();

        // Unit mappings
        CreateMap<Unit, UnitDto>()
            .ForMember(dest => dest.PropertyName, opt => opt.MapFrom(src => src.Property.Name))
            .ForMember(dest => dest.CurrentTenantName, opt => opt.MapFrom(src =>
                src.Tenants.FirstOrDefault(t => t.IsActive) != null
                    ? $"{src.Tenants.FirstOrDefault(t => t.IsActive)!.FirstName} {src.Tenants.FirstOrDefault(t => t.IsActive)!.LastName}"
                    : null))
            .ForMember(dest => dest.CurrentTenantId, opt => opt.MapFrom(src =>
                src.Tenants.FirstOrDefault(t => t.IsActive) != null
                    ? src.Tenants.FirstOrDefault(t => t.IsActive)!.Id
                    : (int?)null))
            .ForMember(dest => dest.LastPaymentDate, opt => opt.MapFrom(src =>
                src.Tenants.FirstOrDefault(t => t.IsActive) != null
                    ? src.Tenants.FirstOrDefault(t => t.IsActive)!.Payments
                        .Where(p => p.Status == Domain.Enums.PaymentStatus.Completed)
                        .OrderByDescending(p => p.PaymentDate)
                        .Select(p => (DateTime?)p.PaymentDate)
                        .FirstOrDefault()
                    : null))
            .ForMember(dest => dest.LastPaymentAmount, opt => opt.MapFrom(src =>
                src.Tenants.FirstOrDefault(t => t.IsActive) != null
                    ? src.Tenants.FirstOrDefault(t => t.IsActive)!.Payments
                        .Where(p => p.Status == Domain.Enums.PaymentStatus.Completed)
                        .OrderByDescending(p => p.PaymentDate)
                        .Select(p => (decimal?)p.Amount)
                        .FirstOrDefault()
                    : null))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src =>
                src.Tenants.FirstOrDefault(t => t.IsActive) == null
                    ? "NoTenant"
                    : src.Tenants.FirstOrDefault(t => t.IsActive)!.Payments.Any(p => p.Status == Domain.Enums.PaymentStatus.Pending)
                        ? "Pending"
                        : src.Tenants.FirstOrDefault(t => t.IsActive)!.Payments
                            .Where(p => p.Status == Domain.Enums.PaymentStatus.Completed)
                            .OrderByDescending(p => p.PaymentDate)
                            .FirstOrDefault() != null &&
                          src.Tenants.FirstOrDefault(t => t.IsActive)!.Payments
                            .Where(p => p.Status == Domain.Enums.PaymentStatus.Completed)
                            .OrderByDescending(p => p.PaymentDate)
                            .First().PaymentDate >= DateTime.UtcNow.AddDays(-src.Tenants.FirstOrDefault(t => t.IsActive)!.RentDueDay)
                        ? "Paid"
                        : "Overdue"))
            .ForMember(dest => dest.DaysOverdue, opt => opt.MapFrom(src =>
                src.Tenants.FirstOrDefault(t => t.IsActive) != null
                    ? (int?)CalculateDaysOverdue(
                        src.Tenants.FirstOrDefault(t => t.IsActive)!.Payments
                            .Where(p => p.Status == Domain.Enums.PaymentStatus.Completed)
                            .OrderByDescending(p => p.PaymentDate)
                            .Select(p => (DateTime?)p.PaymentDate)
                            .FirstOrDefault(),
                        src.Tenants.FirstOrDefault(t => t.IsActive)!.RentDueDay)
                    : null));
        CreateMap<CreateUnitDto, Unit>();
        CreateMap<UpdateUnitDto, Unit>();

        // Tenant mappings
        CreateMap<Tenant, TenantDto>()
            .ForMember(dest => dest.UnitNumber, opt => opt.MapFrom(src => src.Unit.UnitNumber))
            .ForMember(dest => dest.PropertyName, opt => opt.MapFrom(src => src.Unit.Property.Name));
        CreateMap<CreateTenantDto, Tenant>();
        CreateMap<UpdateTenantDto, Tenant>();

        // Payment mappings
        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => $"{src.Tenant.FirstName} {src.Tenant.LastName}"))
            .ForMember(dest => dest.UnitNumber, opt => opt.MapFrom(src => src.Tenant.Unit.UnitNumber))
            .ForMember(dest => dest.PropertyName, opt => opt.MapFrom(src => src.Tenant.Unit.Property.Name))
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<CreatePaymentDto, Payment>();
    }

    private static int CalculateDaysOverdue(DateTime? lastPaymentDate, int rentDueDay)
    {
        if (!lastPaymentDate.HasValue)
        {
            // No payment ever made - calculate from start of month
            var now = DateTime.UtcNow;
            var dueDate = new DateTime(now.Year, now.Month, Math.Min(rentDueDay, DateTime.DaysInMonth(now.Year, now.Month)));
            if (now > dueDate)
            {
                return (now - dueDate).Days;
            }
            return 0;
        }

        // Calculate days since last payment
        var daysSincePayment = (DateTime.UtcNow - lastPaymentDate.Value).Days;

        // If more than 30 days since last payment, consider it overdue
        if (daysSincePayment > 30)
        {
            return daysSincePayment - 30;
        }

        return 0;
    }
}
