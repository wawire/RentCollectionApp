using System.Text.Json.Serialization;

namespace RentCollection.Application.DTOs.Payments;

/// <summary>
/// DTO for M-Pesa C2B (Customer to Business) callback from Safaricom
/// This is received when a customer makes a Paybill payment
/// </summary>
public class MPesaC2BCallbackDto
{
    [JsonPropertyName("TransactionType")]
    public string TransactionType { get; set; } = string.Empty;

    [JsonPropertyName("TransID")]
    public string TransID { get; set; } = string.Empty;

    [JsonPropertyName("TransTime")]
    public string TransTime { get; set; } = string.Empty;

    [JsonPropertyName("TransAmount")]
    public decimal TransAmount { get; set; }

    [JsonPropertyName("BusinessShortCode")]
    public string BusinessShortCode { get; set; } = string.Empty;

    [JsonPropertyName("BillRefNumber")]
    public string BillRefNumber { get; set; } = string.Empty;

    [JsonPropertyName("InvoiceNumber")]
    public string? InvoiceNumber { get; set; }

    [JsonPropertyName("OrgAccountBalance")]
    public decimal? OrgAccountBalance { get; set; }

    [JsonPropertyName("ThirdPartyTransID")]
    public string? ThirdPartyTransID { get; set; }

    [JsonPropertyName("MSISDN")]
    public string MSISDN { get; set; } = string.Empty;

    [JsonPropertyName("FirstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("MiddleName")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("LastName")]
    public string? LastName { get; set; }
}

/// <summary>
/// Response DTO for M-Pesa C2B callback
/// This is what we send back to Safaricom to acknowledge the callback
/// </summary>
public class MPesaC2BCallbackResponseDto
{
    [JsonPropertyName("ResultCode")]
    public int ResultCode { get; set; }

    [JsonPropertyName("ResultDesc")]
    public string ResultDesc { get; set; } = string.Empty;
}
