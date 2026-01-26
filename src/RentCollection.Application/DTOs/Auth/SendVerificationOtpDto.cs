using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Auth;

public class SendVerificationOtpDto
{
    public VerificationChannel Channel { get; set; } = VerificationChannel.Email;
}
