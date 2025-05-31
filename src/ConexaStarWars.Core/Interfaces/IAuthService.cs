using ConexaStarWars.Core.DTOs;

namespace ConexaStarWars.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<bool> CreateRoleAsync(string roleName);
    Task<bool> AssignRoleToUserAsync(string email, string roleName);
}