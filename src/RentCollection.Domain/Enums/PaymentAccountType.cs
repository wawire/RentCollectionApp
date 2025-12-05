namespace RentCollection.Domain.Enums;

/// <summary>
/// Types of payment accounts that landlords can use to receive rent payments
/// </summary>
public enum PaymentAccountType
{
    /// <summary>
    /// M-Pesa Paybill with account numbers (Recommended for automatic unit identification)
    /// </summary>
    MPesaPaybill = 1,

    /// <summary>
    /// M-Pesa Till Number (Less common for rent)
    /// </summary>
    MPesaTillNumber = 2,

    /// <summary>
    /// Personal M-Pesa phone number (For small landlords)
    /// </summary>
    MPesaPhone = 3,

    /// <summary>
    /// Bank account (Equity, KCB, etc.)
    /// </summary>
    BankAccount = 4,

    /// <summary>
    /// Cash payments (Manual tracking)
    /// </summary>
    Cash = 5
}
