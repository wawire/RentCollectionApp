using RentCollection.Application.DTOs.Auth;

namespace RentCollection.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<UserDto> GetCurrentUserAsync(string userId);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<bool> UpdateUserAsync(string userId, UserDto userDto);
    Task<bool> DeleteUserAsync(string userId);
}
