using Microsoft.EntityFrameworkCore;
using RentCollection.Domain.Entities;
using RentCollection.Infrastructure.Data;
using RentCollection.Infrastructure.Repositories.Interfaces;

namespace RentCollection.Infrastructure.Repositories.Implementations;

public class UserRepository : Repository<ApplicationUser>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ApplicationUser?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<ApplicationUser?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken &&
                                     u.RefreshTokenExpiryTime > DateTime.UtcNow);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email);
    }

    public override async Task<IEnumerable<ApplicationUser>> GetAllAsync()
    {
        return await _context.Users
            .OrderByDescending(u => u.IsActive)
            .ThenBy(u => u.Username)
            .ToListAsync();
    }
}
