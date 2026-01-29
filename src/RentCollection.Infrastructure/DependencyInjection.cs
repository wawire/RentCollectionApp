using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services;
using RentCollection.Application.Services.Auth;
using RentCollection.Application.Services.Implementations;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Infrastructure.Configuration;
using RentCollection.Infrastructure.Data;
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

        // Configure M-Pesa settings
        services.Configure<MPesaConfiguration>(configuration.GetSection(MPesaConfiguration.SectionName));

        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IUnitRepository, UnitRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IMaintenanceRequestRepository, MaintenanceRequestRepository>();
        services.AddScoped<ILeaseRenewalRepository, LeaseRenewalRepository>();
        services.AddScoped<ISecurityDepositRepository, SecurityDepositRepository>();
        services.AddScoped<IRentReminderRepository, RentReminderRepository>();
        services.AddScoped<IReminderSettingsRepository, ReminderSettingsRepository>();
        services.AddScoped<ITenantReminderPreferenceRepository, TenantReminderPreferenceRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IMoveOutInspectionRepository, MoveOutInspectionRepository>();

        // Register services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, AfricasTalkingSmsService>();
        services.AddScoped<IVerificationService, VerificationService>();
        services.AddScoped<IPdfService, PdfGenerationService>();
        services.AddScoped<IPublicListingService, PublicListingService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IInvoiceBalanceService, InvoiceBalanceService>();
        services.AddScoped<IPaymentAllocationService, PaymentAllocationService>();
        services.AddScoped<IExportService, CsvExportService>();
        services.AddScoped<IUnmatchedPaymentService, UnmatchedPaymentService>();
        services.AddScoped<ILandlordPaymentAccountService, LandlordPaymentAccountService>();
        services.AddScoped<IMPesaService, MPesaService>();
        services.AddScoped<IMPesaTransactionService, MPesaTransactionService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ITenantPortalService, TenantPortalService>();
        services.AddScoped<IReportsService, ReportsService>();
        services.AddScoped<IMaintenanceRequestService, MaintenanceRequestService>();
        services.AddScoped<ILeaseRenewalService, LeaseRenewalService>();
        services.AddScoped<IBulkImportService, BulkImportService>();
        services.AddScoped<ISecurityDepositService, SecurityDepositService>();
        services.AddScoped<IMoveOutInspectionService, MoveOutInspectionService>();
        services.AddScoped<IUtilityTypeService, UtilityTypeService>();
        services.AddScoped<IUtilityConfigService, UtilityConfigService>();
        services.AddScoped<IMeterReadingService, MeterReadingService>();
        services.AddScoped<IUtilityBillingService, UtilityBillingService>();
        services.AddScoped<IOrganizationService, OrganizationService>();

        // File storage service - Use Local for development, Azure for production
        // To use Azure Blob Storage in production, update this registration to:
        // services.AddScoped<IFileStorageService, AzureBlobStorageService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
