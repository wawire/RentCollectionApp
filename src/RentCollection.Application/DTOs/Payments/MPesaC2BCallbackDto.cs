namespace RentCollection.Application.DTOs.Payments
{
    public class MPesaC2BCallbackDto
    {
        public string TransID { get; set; } = string.Empty;
        public decimal TransAmount { get; set; }
        public string BusinessShortCode { get; set; } = string.Empty;
        public string BillRefNumber { get; set; } = string.Empty;
        public string? InvoiceNumber { get; set; }
        public string? OrgAccountBalance { get; set; }
        public string? ThirdPartyTransID { get; set; }
        public string MSISDN { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string TransTime { get; set; } = string.Empty;
    }
}
