using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Repositories.Interfaces;

public interface IUserRepository : IRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByUsernameAsync(string username);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<ApplicationUser?> GetByRefreshTokenAsync(string refreshToken);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
}
