using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using RentCollection.Infrastructure.Data;
using RentCollection.Application.Services.Interfaces;

namespace RentCollection.IntegrationTests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Clear();
            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-secret-please-change-in-tests-only-1234567890",
                ["Jwt:Issuer"] = "HisaRentalsAPI",
                ["Jwt:Audience"] = "HisaRentals",
                ["ConnectionStrings:DefaultConnection"] = "DataSource=TestDb",
                ["MPesa:WebhookToken"] = "test-webhook-token",
                ["Verification:OtpExpiryMinutes"] = "5",
                ["Verification:OtpMaxAttempts"] = "2",
                ["Verification:OtpResendCooldownSeconds"] = "60",
                ["Verification:OtpLockoutMinutes"] = "10",
                ["Authentication:LoginMaxAttempts"] = "2",
                ["Authentication:LoginLockoutMinutes"] = "5"
            };

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));
            services.RemoveAll<IEmailService>();
            services.RemoveAll<ISmsService>();
            services.RemoveAll<TestOtpStore>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("RentCollectionTestDb"));

            services.AddSingleton<TestOtpStore>();
            services.AddSingleton<IEmailService, TestEmailService>();
            services.AddSingleton<ISmsService, TestSmsService>();

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("test-secret-please-change-in-tests-only-1234567890"));
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "HisaRentalsAPI",
                    ValidAudience = "HisaRentals",
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };
            });
        });
    }
}
