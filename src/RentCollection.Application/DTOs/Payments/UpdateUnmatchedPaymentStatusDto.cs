using RentCollection.Domain.Enums;

namespace RentCollection.Application.DTOs.Payments;

public class UpdateUnmatchedPaymentStatusDto
{
    public UnmatchedPaymentStatus Status { get; set; }
}
