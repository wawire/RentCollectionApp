namespace RentCollection.Domain.Enums;

public enum MPesaTransactionStatus
{
    Pending = 1,        // STK Push sent, awaiting customer action
    Completed = 2,      // Payment successful
    Failed = 3,         // Payment failed
    Cancelled = 4,      // Customer cancelled
    Timeout = 5         // Customer didn't respond in time
}
