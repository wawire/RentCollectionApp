namespace RentCollection.Application.DTOs.Auth
{
    public class Verify2FACodeDto
    {
        public string EmailOrPhone { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
