using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace RentCollection.IntegrationTests.Infrastructure;

public static class TestJwtFactory
{
    public static string CreateToken(string secret, params Claim[] claims)
    {
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "HisaRentalsAPI",
            audience: "HisaRentals",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
