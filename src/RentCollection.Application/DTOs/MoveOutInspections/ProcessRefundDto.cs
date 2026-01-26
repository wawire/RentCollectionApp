namespace RentCollection.Application.DTOs.MoveOutInspections;

public class ProcessRefundDto
{
    public string RefundMethod { get; set; } = string.Empty; // MPesa, BankTransfer, Cash
    public string? RefundReference { get; set; }
    public string? Notes { get; set; }
}
