namespace RentCollection.Application.DTOs.Auth
{
    public class Setup2FAResponseDto
    {
        public string Secret { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string QrCodeUri { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
    }
}
