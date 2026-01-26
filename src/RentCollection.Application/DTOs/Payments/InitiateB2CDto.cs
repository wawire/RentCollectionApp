namespace RentCollection.Application.DTOs.Payments;

/// <summary>
/// DTO for initiating B2C (Business to Customer) disbursement
/// </summary>
public class InitiateB2CDto
{
    /// <summary>
    /// Phone number to receive the disbursement (format: 2547XXXXXXXX)
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Amount to disburse
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Description/remarks for the transaction
    /// </summary>
    public string Remarks { get; set; } = string.Empty;

    /// <summary>
    /// Occasion/reason for disbursement
    /// </summary>
    public string Occasion { get; set; } = string.Empty;

    /// <summary>
    /// Command ID: BusinessPayment, SalaryPayment, or PromotionPayment
    /// Default: BusinessPayment (for refunds)
    /// </summary>
    public string CommandID { get; set; } = "BusinessPayment";
}
