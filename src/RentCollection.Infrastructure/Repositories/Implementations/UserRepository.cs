using Microsoft.EntityFrameworkCore;
using RentCollection.Application.Interfaces;
using RentCollection.Domain.Entities;
using RentCollection.Infrastructure.Data;

namespace RentCollection.Infrastructure.Repositories.Implementations;

/// <summary>
/// Repository implementation for User entity
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        // Normalize phone number: remove spaces, hyphens, and handle +254 vs 0 prefix
        var normalizedPhone = NormalizePhoneNumber(phoneNumber);

        var users = await _dbSet.ToListAsync(cancellationToken);
        return users.FirstOrDefault(u => NormalizePhoneNumber(u.PhoneNumber) == normalizedPhone);
    }

    public async Task<User?> GetByEmailOrPhoneAsync(string emailOrPhone, CancellationToken cancellationToken = default)
    {
        // Try email first
        var user = await GetByEmailAsync(emailOrPhone, cancellationToken);
        if (user != null)
            return user;

        // Try phone number
        return await GetByPhoneNumberAsync(emailOrPhone, cancellationToken);
    }

    public async Task<List<User>> GetAllWithRelatedDataAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Property)
            .Include(u => u.Tenant)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByIdWithRelatedDataAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Property)
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<bool> PhoneNumberExistsAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var normalizedPhone = NormalizePhoneNumber(phoneNumber);
        var users = await _dbSet.ToListAsync(cancellationToken);
        return users.Any(u => NormalizePhoneNumber(u.PhoneNumber) == normalizedPhone);
    }

    /// <summary>
    /// Normalize Kenyan phone numbers for comparison
    /// Converts: 0712345678, +254712345678, 254712345678 to same format
    /// </summary>
    private string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;

        // Remove spaces, hyphens, parentheses
        var cleaned = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());

        // Handle different formats
        if (cleaned.StartsWith("+254"))
            return cleaned.Substring(4); // Remove +254
        if (cleaned.StartsWith("254"))
            return cleaned.Substring(3); // Remove 254
        if (cleaned.StartsWith("0"))
            return cleaned.Substring(1); // Remove leading 0

        return cleaned;
    }
}
