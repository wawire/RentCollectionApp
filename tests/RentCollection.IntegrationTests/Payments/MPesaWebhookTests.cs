using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using RentCollection.IntegrationTests.Infrastructure;
using Xunit;

namespace RentCollection.IntegrationTests.Payments;

public class MPesaWebhookTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public MPesaWebhookTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task C2BValidation_Rejects_Missing_Token()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/mpesa/c2b/validation", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task C2BConfirmation_Is_Idempotent_On_TransactionId()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var landlord = new User { Id = 1, FirstName = "L", LastName = "L", Email = "l@l.com", OrganizationId = 1 };
        var property = new Property { Id = 1, Name = "P1", LandlordId = landlord.Id, Landlord = landlord, OrganizationId = 1 };
        var unit = new Unit
        {
            Id = 1,
            PropertyId = property.Id,
            Property = property,
            UnitNumber = "A1",
            PaymentAccountNumber = "A1"
        };
        var tenant = new Tenant
        {
            Id = 1,
            UnitId = unit.Id,
            Unit = unit,
            FirstName = "T",
            LastName = "X",
            Status = TenantStatus.Active,
            IsActive = true
        };
        var paymentAccount = new LandlordPaymentAccount
        {
            Id = 1,
            PropertyId = property.Id,
            Property = property,
            LandlordId = landlord.Id,
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

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-MPesa-Token", "test-webhook-token");

        var payload = new
        {
            TransID = "TX-1",
            TransAmount = 1000m,
            BusinessShortCode = "123456",
            BillRefNumber = "A1",
            MSISDN = "0712345678",
            FirstName = "John",
            LastName = "Doe",
            TransTime = "20251218093000"
        };

        var response1 = await client.PostAsJsonAsync("/api/mpesa/c2b/confirmation", payload);
        var response2 = await client.PostAsJsonAsync("/api/mpesa/c2b/confirmation", payload);

        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        (await context.Payments.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task StkPushCallback_Is_Idempotent_On_CheckoutRequestId()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var landlord = new User { Id = 1, FirstName = "L", LastName = "L", Email = "l@l.com", OrganizationId = 1 };
        var property = new Property { Id = 1, Name = "P1", LandlordId = landlord.Id, Landlord = landlord, OrganizationId = 1 };
        var unit = new Unit
        {
            Id = 1,
            PropertyId = property.Id,
            Property = property,
            UnitNumber = "A1",
            PaymentAccountNumber = "A1"
        };
        var tenant = new Tenant
        {
            Id = 1,
            UnitId = unit.Id,
            Unit = unit,
            FirstName = "T",
            LastName = "X",
            Status = TenantStatus.Active,
            IsActive = true,
            RentDueDay = 5,
            MonthlyRent = 1000
        };
        var paymentAccount = new LandlordPaymentAccount
        {
            Id = 1,
            PropertyId = property.Id,
            Property = property,
            LandlordId = landlord.Id,
            AccountType = PaymentAccountType.MPesaPaybill,
            MPesaShortCode = "123456",
            IsActive = true
        };
        var transaction = new MPesaTransaction
        {
            Id = 1,
            TenantId = tenant.Id,
            TransactionType = MPesaTransactionType.C2B,
            CheckoutRequestID = "CR-1",
            Amount = 1000,
            Status = MPesaTransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(landlord);
        context.Properties.Add(property);
        context.Units.Add(unit);
        context.Tenants.Add(tenant);
        context.LandlordPaymentAccounts.Add(paymentAccount);
        context.MPesaTransactions.Add(transaction);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-MPesa-Token", "test-webhook-token");

        var payload = new
        {
            Body = new
            {
                stkCallback = new
                {
                    MerchantRequestID = "MR-1",
                    CheckoutRequestID = "CR-1",
                    ResultCode = 0,
                    ResultDesc = "Success",
                    CallbackMetadata = new[]
                    {
                        new
                        {
                            Item = new[]
                            {
                                new { Name = "MpesaReceiptNumber", Value = (object)"ABCD1234" },
                                new { Name = "TransactionDate", Value = (object)20251218093000L }
                            }
                        }
                    }
                }
            }
        };

        var response1 = await client.PostAsJsonAsync("/api/mpesa/stkpush/callback", payload);
        var response2 = await client.PostAsJsonAsync("/api/mpesa/stkpush/callback", payload);

        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        (await context.Payments.CountAsync()).Should().Be(1);
    }
}
