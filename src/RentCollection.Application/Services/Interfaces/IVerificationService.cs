using RentCollection.Domain.Enums;

namespace RentCollection.Application.Services.Interfaces;

public interface IVerificationService
{
    Task SendVerificationOtpAsync(int userId, VerificationChannel channel, CancellationToken cancellationToken = default);
    Task VerifyOtpAsync(int userId, string code, CancellationToken cancellationToken = default);
}
