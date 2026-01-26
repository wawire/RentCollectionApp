namespace RentCollection.Application.DTOs.Auth;

public class CompletePasswordChangeDto
{
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
