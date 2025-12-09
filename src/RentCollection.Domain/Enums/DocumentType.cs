namespace RentCollection.Domain.Enums;

/// <summary>
/// Types of documents that can be uploaded in the system
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// Lease agreement document
    /// </summary>
    LeaseAgreement = 1,

    /// <summary>
    /// Tenant's ID card or passport copy
    /// </summary>
    IDCopy = 2,

    /// <summary>
    /// Tenant's proof of income (payslip, bank statement, etc.)
    /// </summary>
    ProofOfIncome = 3,

    /// <summary>
    /// Reference letter from previous landlord
    /// </summary>
    ReferenceLetter = 4,

    /// <summary>
    /// Other miscellaneous documents
    /// </summary>
    Other = 99
}
