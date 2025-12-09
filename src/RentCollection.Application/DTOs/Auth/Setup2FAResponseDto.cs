namespace RentCollection.Application.DTOs.Auth
{
    public class Setup2FAResponseDto
    {
        public string Secret { get; set; } = string.Empty;
        public string QrCodeUri { get; set; } = string.Empty;
    }
}
