using RentCollection.Application.Common.Models;
using RentCollection.Application.DTOs.Auth;

namespace RentCollection.Application.Services.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto);
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
    Task<Result> LogoutAsync(int userId);
    Task<Result<UserDto>> GetUserByIdAsync(int userId);
    Task<Result<IEnumerable<UserDto>>> GetAllUsersAsync();
}
