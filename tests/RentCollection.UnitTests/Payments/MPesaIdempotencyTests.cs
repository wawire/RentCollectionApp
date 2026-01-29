using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Configuration;
using RentCollection.Infrastructure.Data;
using RentCollection.Infrastructure.Services;
using RentCollection.UnitTests.TestDoubles;
using Xunit;

namespace RentCollection.UnitTests.Payments;

public class MPesaIdempotencyTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task HandleC2BCallbackAsync_Is_Idempotent_On_TransactionId()
    {
        using var context = CreateDbContext();

        var landlord = new User { Id = 1, FirstName = "L", LastName = "L", Email = "l@l.com", OrganizationId = 1 };
        var property = new Property { Id = 1, Name = "P1", LandlordId = landlord.Id, Landlord = landlord, OrganizationId = 1 };
        var unit = new Unit { Id = 1, PropertyId = property.Id, Property = property, UnitNumber = "A1", PaymentAccountNumber = "A1" };
        var tenant = new Tenant { Id = 1, UnitId = unit.Id, Unit = unit, FirstName = "T", LastName = "X", Status = TenantStatus.Active };
        var paymentAccount = new LandlordPaymentAccount
        {
            Id = 1,
            PropertyId = property.Id,
            Property = property,
            AccountType = PaymentAccountType.MPesaPaybill,
            MPesaShortCode = "123456",
            IsActive = true
        };

        context.Users.Add(landlord);
        context.Properties.Add(property);
        context.Units.Add(unit);
        context.Tenants.Add(tenant);
        context.LandlordPaymentAccounts.Add(paymentAccount);
        await context.SaveChangesAsync();

        var mpesaConfig = Options.Create(new MPesaConfiguration
        {
            UseSandbox = true,
            CallbackBaseUrl = "https://example.com"
        });

        var service = new MPesaService(
            context,
            Mock.Of<ILogger<MPesaService>>(),
            Mock.Of<IHttpClientFactory>(),
            mpesaConfig,
            new FakeCurrentUserService { IsAuthenticated = false });

        var callback = new MPesaC2BCallbackDto
        {
            TransID = "TX-1",
            TransAmount = 1000,
            BusinessShortCode = "123456",
            BillRefNumber = "A1",
            MSISDN = "0712345678",
            FirstName = "John",
            LastName = "Doe",
            TransTime = "20251218093000"
        };

        var result1 = await service.HandleC2BCallbackAsync(callback, "corr-1");
        var result2 = await service.HandleC2BCallbackAsync(callback, "corr-2");

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        (await context.Payments.CountAsync()).Should().Be(1);
    }
}
