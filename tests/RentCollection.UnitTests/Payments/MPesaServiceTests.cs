using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RentCollection.Application.DTOs.Payments;
using RentCollection.Infrastructure.Configuration;
using RentCollection.Infrastructure.Data;
using RentCollection.Infrastructure.Services;
using RentCollection.UnitTests.TestDoubles;
using Xunit;

namespace RentCollection.UnitTests.Payments;

public class MPesaServiceTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task HandleC2BCallbackAsync_Quarantines_Invalid_Account_Reference()
    {
        using var context = CreateDbContext();

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
            TransID = "TX123",
            TransAmount = 1000,
            BusinessShortCode = "123456",
            BillRefNumber = "##INVALID##",
            MSISDN = "0712345678",
            FirstName = "John",
            LastName = "Doe",
            TransTime = "20251218093000"
        };

        var result = await service.HandleC2BCallbackAsync(callback, "corr-1");

        result.IsSuccess.Should().BeTrue();
        var unmatched = await context.UnmatchedPayments.FirstOrDefaultAsync();
        unmatched.Should().NotBeNull();
        unmatched!.Reason.Should().Contain("Invalid account reference");
        unmatched.CorrelationId.Should().Be("corr-1");
    }
}
