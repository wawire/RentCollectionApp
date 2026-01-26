using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RentCollection.Application.Common.Models;
using RentCollection.Application.Interfaces;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Application.Mappings;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using RentCollection.Infrastructure.Services;
using RentCollection.UnitTests.TestDoubles;
using Xunit;

namespace RentCollection.UnitTests.Payments;

public class PaymentServiceTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        return config.CreateMapper();
    }

    [Fact]
    public async Task ConfirmPaymentAsync_Denies_When_Landlord_Does_Not_Own_Property()
    {
        using var context = CreateDbContext();

        var property = new Property { Id = 1, Name = "P1", LandlordId = 100, OrganizationId = 1 };
        var unit = new Unit { Id = 1, PropertyId = property.Id, Property = property, UnitNumber = "A1" };
        var tenant = new Tenant { Id = 1, UnitId = unit.Id, Unit = unit, FirstName = "T", LastName = "X" };
        var paymentAccount = new LandlordPaymentAccount { Id = 1, PropertyId = property.Id, Property = property };
        var payment = new Payment
        {
            Id = 1,
            TenantId = tenant.Id,
            Tenant = tenant,
            UnitId = unit.Id,
            Unit = unit,
            LandlordAccountId = paymentAccount.Id,
            LandlordAccount = paymentAccount,
            Amount = 1000,
            PaymentDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.Cash,
            Status = PaymentStatus.Pending,
            PeriodStart = DateTime.UtcNow.Date,
            PeriodEnd = DateTime.UtcNow.Date.AddDays(30)
        };

        context.Properties.Add(property);
        context.Units.Add(unit);
        context.Tenants.Add(tenant);
        context.LandlordPaymentAccounts.Add(paymentAccount);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService
        {
            IsLandlord = true,
            UserIdInt = 200,
            OrganizationId = 1
        };

        var paymentAllocationService = new Mock<IPaymentAllocationService>();
        paymentAllocationService
            .Setup(x => x.AllocatePaymentToOutstandingInvoicesAsync(It.IsAny<int>()))
            .ReturnsAsync(Result.Success());

        var paymentService = new PaymentService(
            Mock.Of<IPaymentRepository>(),
            Mock.Of<ITenantRepository>(),
            Mock.Of<IAuditLogService>(),
            Mock.Of<IFileStorageService>(),
            CreateMapper(),
            Mock.Of<ILogger<PaymentService>>(),
            currentUser,
            context,
            paymentAllocationService.Object);

        var result = await paymentService.ConfirmPaymentAsync(payment.Id, 200);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ConfirmPaymentAsync_Allows_When_Landlord_Owns_Property()
    {
        using var context = CreateDbContext();

        var property = new Property { Id = 1, Name = "P1", LandlordId = 100, OrganizationId = 1 };
        var unit = new Unit { Id = 1, PropertyId = property.Id, Property = property, UnitNumber = "A1" };
        var tenant = new Tenant { Id = 1, UnitId = unit.Id, Unit = unit, FirstName = "T", LastName = "X" };
        var paymentAccount = new LandlordPaymentAccount { Id = 1, PropertyId = property.Id, Property = property };
        var payment = new Payment
        {
            Id = 1,
            TenantId = tenant.Id,
            Tenant = tenant,
            UnitId = unit.Id,
            Unit = unit,
            LandlordAccountId = paymentAccount.Id,
            LandlordAccount = paymentAccount,
            Amount = 1000,
            PaymentDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.Cash,
            Status = PaymentStatus.Pending,
            PeriodStart = DateTime.UtcNow.Date,
            PeriodEnd = DateTime.UtcNow.Date.AddDays(30)
        };

        context.Properties.Add(property);
        context.Units.Add(unit);
        context.Tenants.Add(tenant);
        context.LandlordPaymentAccounts.Add(paymentAccount);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService
        {
            IsLandlord = true,
            UserIdInt = 100,
            OrganizationId = 1
        };

        var paymentAllocationService = new Mock<IPaymentAllocationService>();
        paymentAllocationService
            .Setup(x => x.AllocatePaymentToOutstandingInvoicesAsync(It.IsAny<int>()))
            .ReturnsAsync(Result.Success());

        var paymentService = new PaymentService(
            Mock.Of<IPaymentRepository>(),
            Mock.Of<ITenantRepository>(),
            Mock.Of<IAuditLogService>(),
            Mock.Of<IFileStorageService>(),
            CreateMapper(),
            Mock.Of<ILogger<PaymentService>>(),
            currentUser,
            context,
            paymentAllocationService.Object);

        var result = await paymentService.ConfirmPaymentAsync(payment.Id, 100);

        result.IsSuccess.Should().BeTrue();
        var updated = await context.Payments.FirstAsync(p => p.Id == payment.Id);
        updated.Status.Should().Be(PaymentStatus.Completed);
        updated.ConfirmedByUserId.Should().Be(100);
    }
}
