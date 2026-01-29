namespace RentCollection.Domain.Enums;

/// <summary>
/// M-Pesa transaction type
/// </summary>
public enum MPesaTransactionType
{
    /// <summary>
    /// Customer to Business - Payments from customer (STK Push, Paybill)
    /// </summary>
    C2B = 1,

    /// <summary>
    /// Business to Customer - Disbursements to customer (Refunds, Payouts)
    /// </summary>
    B2C = 2
}
