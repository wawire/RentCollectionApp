using Microsoft.EntityFrameworkCore;
using Moq;
using RentCollection.Application.Services.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Domain.Enums;
using RentCollection.Infrastructure.Data;
using RentCollection.Infrastructure.Services;
using RentCollection.UnitTests.TestDoubles;
using Xunit;

namespace RentCollection.UnitTests.Exports;

public class CsvExportServiceTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new List<char>();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Add('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                result.Add(new string(current.ToArray()));
                current.Clear();
                continue;
            }

            current.Add(c);
        }

        result.Add(new string(current.ToArray()));
        return result;
    }

    [Fact]
    public async Task ExportPaymentsAsync_FiltersByLandlordAndDateRange()
    {
        using var context = CreateDbContext();

        var landlord1 = new User { Id = 1, FirstName = "L", LastName = "One", Email = "l1@test.com", OrganizationId = 1 };
        var landlord2 = new User { Id = 2, FirstName = "L", LastName = "Two", Email = "l2@test.com", OrganizationId = 2 };

        var property1 = new Property { Id = 1, Name = "P1", LandlordId = landlord1.Id, OrganizationId = 1 };
        var property2 = new Property { Id = 2, Name = "P2", LandlordId = landlord2.Id, OrganizationId = 2 };

        var unit1 = new Unit { Id = 1, PropertyId = property1.Id, Property = property1, UnitNumber = "A1" };
        var unit2 = new Unit { Id = 2, PropertyId = property2.Id, Property = property2, UnitNumber = "B1" };

        var tenant1 = new Tenant { Id = 1, UnitId = unit1.Id, Unit = unit1, FirstName = "T", LastName = "One" };
        var tenant2 = new Tenant { Id = 2, UnitId = unit2.Id, Unit = unit2, FirstName = "T", LastName = "Two" };

        var account1 = new LandlordPaymentAccount { Id = 1, PropertyId = property1.Id, Property = property1 };
        var account2 = new LandlordPaymentAccount { Id = 2, PropertyId = property2.Id, Property = property2 };

        var paymentInRange = new Payment
        {
            Id = 1,
            TenantId = tenant1.Id,
            Tenant = tenant1,
            UnitId = unit1.Id,
            Unit = unit1,
            LandlordAccountId = account1.Id,
            LandlordAccount = account1,
            Amount = 1000,
            PaymentDate = new DateTime(2025, 1, 10),
            DueDate = new DateTime(2025, 1, 5),
            PeriodStart = new DateTime(2025, 1, 1),
            PeriodEnd = new DateTime(2025, 1, 31),
            PaymentMethod = PaymentMethod.Cash,
            Status = PaymentStatus.Completed
        };

        var paymentOutOfRange = new Payment
        {
            Id = 2,
            TenantId = tenant1.Id,
            Tenant = tenant1,
            UnitId = unit1.Id,
            Unit = unit1,
            LandlordAccountId = account1.Id,
            LandlordAccount = account1,
            Amount = 1200,
            PaymentDate = new DateTime(2025, 2, 1),
            DueDate = new DateTime(2025, 2, 5),
            PeriodStart = new DateTime(2025, 2, 1),
            PeriodEnd = new DateTime(2025, 2, 28),
            PaymentMethod = PaymentMethod.Cash,
            Status = PaymentStatus.Completed
        };

        var paymentOtherLandlord = new Payment
        {
            Id = 3,
            TenantId = tenant2.Id,
            Tenant = tenant2,
            UnitId = unit2.Id,
            Unit = unit2,
            LandlordAccountId = account2.Id,
            LandlordAccount = account2,
            Amount = 1300,
            PaymentDate = new DateTime(2025, 1, 15),
            DueDate = new DateTime(2025, 1, 5),
            PeriodStart = new DateTime(2025, 1, 1),
            PeriodEnd = new DateTime(2025, 1, 31),
            PaymentMethod = PaymentMethod.Cash,
            Status = PaymentStatus.Completed
        };

        context.Users.AddRange(landlord1, landlord2);
        context.Properties.AddRange(property1, property2);
        context.Units.AddRange(unit1, unit2);
        context.Tenants.AddRange(tenant1, tenant2);
        context.LandlordPaymentAccounts.AddRange(account1, account2);
        context.Payments.AddRange(paymentInRange, paymentOutOfRange, paymentOtherLandlord);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService
        {
            IsLandlord = true,
            UserIdInt = landlord1.Id,
            OrganizationId = 1
        };

        var service = new CsvExportService(context, currentUser, Mock.Of<IAuditLogService>());

        var csv = await service.ExportPaymentsAsync(
            null,
            new DateTime(2025, 1, 1),
            new DateTime(2025, 1, 31));

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.TrimEnd('\r'))
            .ToArray();

        Assert.Equal(2, lines.Length);

        var columns = ParseCsvLine(lines[1]);
        Assert.Equal(paymentInRange.Id.ToString(), columns[0]);
    }

    [Fact]
    public async Task ExportInvoicesAsync_FiltersByTenantScope()
    {
        using var context = CreateDbContext();

        var property = new Property { Id = 1, Name = "P1", LandlordId = 100, OrganizationId = 1 };
        var unit1 = new Unit { Id = 1, PropertyId = property.Id, Property = property, UnitNumber = "A1" };
        var unit2 = new Unit { Id = 2, PropertyId = property.Id, Property = property, UnitNumber = "A2" };

        var tenant1 = new Tenant { Id = 1, UnitId = unit1.Id, Unit = unit1, FirstName = "T", LastName = "One" };
        var tenant2 = new Tenant { Id = 2, UnitId = unit2.Id, Unit = unit2, FirstName = "T", LastName = "Two" };

        var invoice1 = new Invoice
        {
            Id = 1,
            TenantId = tenant1.Id,
            Tenant = tenant1,
            UnitId = unit1.Id,
            Unit = unit1,
            PropertyId = property.Id,
            Property = property,
            LandlordId = property.LandlordId ?? 0,
            PeriodStart = new DateTime(2025, 1, 1),
            PeriodEnd = new DateTime(2025, 1, 31),
            DueDate = new DateTime(2025, 1, 5),
            Amount = 1000,
            Balance = 1000,
            OpeningBalance = 0,
            Status = InvoiceStatus.Issued
        };

        var invoice2 = new Invoice
        {
            Id = 2,
            TenantId = tenant2.Id,
            Tenant = tenant2,
            UnitId = unit2.Id,
            Unit = unit2,
            PropertyId = property.Id,
            Property = property,
            LandlordId = property.LandlordId ?? 0,
            PeriodStart = new DateTime(2025, 1, 1),
            PeriodEnd = new DateTime(2025, 1, 31),
            DueDate = new DateTime(2025, 1, 5),
            Amount = 1100,
            Balance = 1100,
            OpeningBalance = 0,
            Status = InvoiceStatus.Issued
        };

        context.Properties.Add(property);
        context.Units.AddRange(unit1, unit2);
        context.Tenants.AddRange(tenant1, tenant2);
        context.Invoices.AddRange(invoice1, invoice2);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService
        {
            IsTenant = true,
            TenantId = tenant1.Id,
            OrganizationId = 1
        };

        var service = new CsvExportService(context, currentUser, Mock.Of<IAuditLogService>());
        var csv = await service.ExportInvoicesAsync(null, null, null);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.TrimEnd('\r'))
            .ToArray();

        Assert.Equal(2, lines.Length);

        var columns = ParseCsvLine(lines[1]);
        Assert.Equal(invoice1.Id.ToString(), columns[0]);
    }

    [Fact]
    public async Task ExportExpensesAsync_FiltersByLandlordScope()
    {
        using var context = CreateDbContext();

        var landlord1 = new User { Id = 10, FirstName = "L", LastName = "One", Email = "l1@test.com", OrganizationId = 1 };
        var landlord2 = new User { Id = 20, FirstName = "L", LastName = "Two", Email = "l2@test.com", OrganizationId = 2 };

        var property1 = new Property { Id = 1, Name = "P1", LandlordId = landlord1.Id, OrganizationId = 1 };
        var property2 = new Property { Id = 2, Name = "P2", LandlordId = landlord2.Id, OrganizationId = 2 };

        var expense1 = new Expense
        {
            Id = 1,
            PropertyId = property1.Id,
            Property = property1,
            LandlordId = landlord1.Id,
            Amount = 500,
            ExpenseDate = new DateTime(2025, 1, 3),
            Category = ExpenseCategory.Maintenance,
            Vendor = "FixCo",
            Description = "Repair"
        };

        var expense2 = new Expense
        {
            Id = 2,
            PropertyId = property2.Id,
            Property = property2,
            LandlordId = landlord2.Id,
            Amount = 800,
            ExpenseDate = new DateTime(2025, 1, 4),
            Category = ExpenseCategory.Management,
            Vendor = "ManageCo",
            Description = "Fees"
        };

        context.Users.AddRange(landlord1, landlord2);
        context.Properties.AddRange(property1, property2);
        context.Expenses.AddRange(expense1, expense2);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService
        {
            IsLandlord = true,
            UserIdInt = landlord1.Id,
            OrganizationId = 1
        };

        var service = new CsvExportService(context, currentUser, Mock.Of<IAuditLogService>());
        var csv = await service.ExportExpensesAsync(null, null, null);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.TrimEnd('\r'))
            .ToArray();

        Assert.Equal(2, lines.Length);

        var columns = ParseCsvLine(lines[1]);
        Assert.Equal(expense1.Id.ToString(), columns[0]);
    }
}
