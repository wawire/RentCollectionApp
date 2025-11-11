using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Infrastructure.Data;
using RentCollection.Infrastructure.Repositories.Implementations;
using RentCollection.Infrastructure.Repositories.Interfaces;
using RentCollection.Infrastructure.Services;

namespace RentCollection.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IUnitRepository, UnitRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        // Register services
        services.AddScoped<ISmsService, AfricasTalkingSmsService>();
        services.AddScoped<IPdfService, PdfGenerationService>();

        return services;
    }
}
