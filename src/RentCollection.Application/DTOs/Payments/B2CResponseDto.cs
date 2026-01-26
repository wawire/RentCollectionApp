namespace RentCollection.Application.DTOs.Payments;

/// <summary>
/// Response from B2C disbursement request
/// </summary>
public class B2CResponseDto
{
    public string OriginatorConversationID { get; set; } = string.Empty;
    public string ConversationID { get; set; } = string.Empty;
    public string ResponseCode { get; set; } = string.Empty;
    public string ResponseDescription { get; set; } = string.Empty;
    public bool IsSuccessful => ResponseCode == "0";
}
