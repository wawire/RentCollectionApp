using System.Text.Json.Serialization;

namespace RentCollection.Application.DTOs.Payments;

/// <summary>
/// STK Push response from M-Pesa API
/// </summary>
public class StkPushCallbackDto
{
    public string? MerchantRequestID { get; set; }
    public string? CheckoutRequestID { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseDescription { get; set; }
    public string? CustomerMessage { get; set; }
}

/// <summary>
/// STK Push callback from Safaricom
/// </summary>
public class StkPushCallbackRequestDto
{
    public StkPushBody? Body { get; set; }

    public class StkPushBody
    {
        [JsonPropertyName("stkCallback")]
        public StkCallback? StkCallback { get; set; }
    }

    public class StkCallback
    {
        public string? MerchantRequestID { get; set; }
        public string? CheckoutRequestID { get; set; }
        public int ResultCode { get; set; }
        public string? ResultDesc { get; set; }
        public List<CallbackMetadata>? CallbackMetadata { get; set; }
    }

    public class CallbackMetadata
    {
        public List<CallbackMetadataItem>? Item { get; set; }
    }

    public class CallbackMetadataItem
    {
        public string? Name { get; set; }
        public object? Value { get; set; }
    }
}

/// <summary>
/// STK Push query response
/// </summary>
public class StkPushQueryResponseDto
{
    public string? ResponseCode { get; set; }
    public string? ResponseDescription { get; set; }
    public string? MerchantRequestID { get; set; }
    public string? CheckoutRequestID { get; set; }
    public string? ResultCode { get; set; }
    public string? ResultDesc { get; set; }
}
