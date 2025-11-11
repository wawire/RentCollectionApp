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
}
