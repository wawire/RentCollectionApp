using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services;
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
        services.AddScoped<IDashboardService, DashboardService>();

        // Register Rent Reminder Services
        services.AddScoped<IRentReminderService, RentReminderService>();
        services.AddSingleton<MessageTemplateService>();
        services.AddHostedService<RentReminderBackgroundService>();

        // Register HttpClient for M-Pesa API
        services.AddHttpClient();

        return services;
    }
}
