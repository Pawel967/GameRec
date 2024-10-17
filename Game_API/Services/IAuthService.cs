using Game_API.Dtos.Account;

namespace Game_API.Services
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterDto registerDto);
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
        Task<bool> AssignRoleAsync(Guid userId, string roleName);
        Task<bool> DeleteUserAsync(Guid userId);
        Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto);
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<IEnumerable<string>> GetAllRolesAsync();
    }
}
