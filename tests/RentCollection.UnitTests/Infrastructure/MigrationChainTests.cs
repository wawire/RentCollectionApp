using FluentAssertions;
using Microsoft.EntityFrameworkCore.Migrations;
using RentCollection.Infrastructure.Data;
using Xunit;

namespace RentCollection.UnitTests.Infrastructure;

public class MigrationChainTests
{
    [Fact]
    public void Migration_Chain_Includes_Baseline()
    {
        var migrationTypes = typeof(ApplicationDbContext).Assembly
            .GetTypes()
            .Where(t => typeof(Migration).IsAssignableFrom(t))
            .Where(t => t.Namespace != null && t.Namespace.Contains("Migrations"))
            .ToList();

        migrationTypes.Should().NotBeEmpty();
        migrationTypes.Should().Contain(t => t.Name.Contains("BaselineCurrentModel"));
    }
}
