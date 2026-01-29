using FluentAssertions;
using RentCollection.Application.Helpers;
using Xunit;

namespace RentCollection.UnitTests.Auth;

public class PasswordPolicyTests
{
    [Theory]
    [InlineData("short")]
    [InlineData("12345678901")]
    [InlineData("passw0rd!")]
    public void PasswordPolicy_Rejects_ShortPasswords(string password)
    {
        var result = PasswordPolicy.IsValid(password, out var error);

        result.Should().BeFalse();
        error.Should().Contain("at least");
    }

    [Fact]
    public void PasswordPolicy_Allows_StrongPassword()
    {
        var result = PasswordPolicy.IsValid("HisaRentals@2025", out var error);

        result.Should().BeTrue();
        error.Should().BeNull();
    }
}
