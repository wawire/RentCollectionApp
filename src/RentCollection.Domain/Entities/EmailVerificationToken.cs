namespace RentCollection.Domain.Entities;

/// <summary>
/// Tracks email verification tokens for user accounts
/// </summary>
public class EmailVerificationToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    public string? IpAddress { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;

    /// <summary>
    /// Check if the token is valid (not expired and not used)
    /// </summary>
    public bool IsValid => !IsUsed && DateTime.UtcNow <= ExpiresAt;
}
