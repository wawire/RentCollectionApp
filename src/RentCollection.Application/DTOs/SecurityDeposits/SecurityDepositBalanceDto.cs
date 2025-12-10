namespace RentCollection.Application.DTOs.SecurityDeposits;

public class SecurityDepositBalanceDto
{
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string UnitNumber { get; set; } = string.Empty;

    public decimal InitialDeposit { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalRefunds { get; set; }
    public decimal CurrentBalance { get; set; }

    public DateTime? LastTransactionDate { get; set; }
    public int TotalTransactions { get; set; }

    public List<SecurityDepositTransactionDto> RecentTransactions { get; set; } = new();
}
