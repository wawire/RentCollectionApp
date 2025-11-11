namespace RentCollection.Application.DTOs.Sms;

public class SendSmsDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? TenantId { get; set; }
}
