using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Documents;
using RentCollection.Application.Interfaces;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Infrastructure.Data;
using RentCollection.Infrastructure.Services;
using RentCollection.UnitTests.TestDoubles;
using Xunit;

namespace RentCollection.UnitTests.Documents;

public class DocumentServiceTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetDocumentsByTenantIdAsync_Fails_For_Other_Tenant()
    {
        using var context = CreateDbContext();

        var tenant = new Tenant
        {
            Id = 2,
            Unit = new Unit
            {
                Property = new Property { LandlordId = 5, OrganizationId = 1 }
            }
        };

        var tenantRepository = new Mock<ITenantRepository>();
        tenantRepository
            .Setup(r => r.GetTenantWithDetailsAsync(2))
            .ReturnsAsync(tenant);

        var documentRepository = new Mock<IDocumentRepository>();
        documentRepository
            .Setup(r => r.GetDocumentsByTenantIdAsync(2))
            .ReturnsAsync(new List<Document>());

        var service = new DocumentService(
            documentRepository.Object,
            tenantRepository.Object,
            Mock.Of<IPropertyRepository>(),
            Mock.Of<IUnitRepository>(),
            Mock.Of<IUserRepository>(),
            Mock.Of<IFileStorageService>(),
            Mock.Of<IAuditLogService>(),
            new FakeCurrentUserService
            {
                IsTenant = true,
                TenantId = 1,
                OrganizationId = 1
            },
            Mock.Of<AutoMapper.IMapper>(),
            Mock.Of<ILogger<DocumentService>>(),
            context);

        var result = await service.GetDocumentsByTenantIdAsync(2);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("You can only view your own documents");
    }
}
