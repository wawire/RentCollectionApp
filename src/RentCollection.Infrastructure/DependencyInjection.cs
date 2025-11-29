using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Auth;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Infrastructure.Data;
using RentCollection.Infrastructure.Identity;
using RentCollection.Infrastructure.Repositories.Implementations;
using RentCollection.Infrastructure.Services;
using RentCollection.Infrastructure.Services.Auth;

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

        // Add Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IUnitRepository, UnitRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Register services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISmsService, AfricasTalkingSmsService>();
        services.AddScoped<IPdfService, PdfGenerationService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
