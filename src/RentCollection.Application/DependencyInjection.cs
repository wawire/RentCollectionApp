using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RentCollection.Application.Services.Implementations;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Add FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register Application Services
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<IUnitService, UnitService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ILandlordPaymentAccountService, LandlordPaymentAccountService>();
        services.AddScoped<IMPesaService, MPesaService>();

        // Register HttpClient for M-Pesa API
        services.AddHttpClient();

        return services;
    }
}
