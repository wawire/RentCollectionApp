namespace RentCollection.Application.DTOs.Reports
{
    public class ArrearsReportDto
    {
        public DateTime ReportDate { get; set; }
        public decimal TotalArrears { get; set; }
        public decimal TotalLateFees { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int TotalTenantsInArrears { get; set; }
        public List<TenantArrearsDto> TenantArrears { get; set; } = new();
        public List<PropertyArrearsDto> PropertiesBreakdown { get; set; } = new();
    }

    public class TenantArrearsDto
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public decimal TotalArrears { get; set; }
        public decimal TotalLateFees { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int DaysOverdue { get; set; }
        public DateTime OldestOverdueDate { get; set; }
        public List<OverduePaymentDto> OverduePayments { get; set; } = new();
    }

    public class OverduePaymentDto
    {
        public int PaymentId { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public decimal Amount { get; set; }
        public decimal LateFee { get; set; }
        public decimal TotalDue { get; set; }
        public string PeriodStart { get; set; } = string.Empty;
        public string PeriodEnd { get; set; } = string.Empty;
    }

    public class PropertyArrearsDto
    {
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public decimal TotalArrears { get; set; }
        public decimal TotalLateFees { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int TenantsInArrears { get; set; }
        public int TotalTenants { get; set; }
        public decimal ArrearsRate { get; set; }
    }
}
