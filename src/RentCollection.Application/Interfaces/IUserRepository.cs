using RentCollection.Domain.Entities;

namespace RentCollection.Application.Interfaces;

/// <summary>
/// Repository interface for User entity
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Find user by email
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find user by phone number
    /// </summary>
    Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find user by email or phone number
    /// </summary>
    Task<User?> GetByEmailOrPhoneAsync(string emailOrPhone, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all users with their related entities (Property, Tenant)
    /// </summary>
    Task<List<User>> GetAllWithRelatedDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user with related entities (Property, Tenant)
    /// </summary>
    Task<User?> GetByIdWithRelatedDataAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if email already exists
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if phone number already exists
    /// </summary>
    Task<bool> PhoneNumberExistsAsync(string phoneNumber, CancellationToken cancellationToken = default);
}
