using System.ComponentModel.DataAnnotations;

namespace RentCollection.Application.DTOs.Payments;

/// <summary>
/// DTO for initiating M-Pesa STK Push payment
/// </summary>
public class InitiateStkPushDto
{
    [Required]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Payment period start date
    /// </summary>
    [Required]
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Payment period end date
    /// </summary>
    [Required]
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Optional notes about the payment
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for STK Push callback response
/// </summary>
public class StkPushCallbackDto
{
    public string MerchantRequestID { get; set; } = string.Empty;
    public string CheckoutRequestID { get; set; } = string.Empty;
    public int ResultCode { get; set; }
    public string ResultDesc { get; set; } = string.Empty;
    public string? MPesaReceiptNumber { get; set; }
    public decimal? Amount { get; set; }
    public string? PhoneNumber { get; set; }
}
