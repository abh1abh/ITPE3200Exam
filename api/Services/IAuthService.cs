using api.DTO;
using Microsoft.AspNetCore.Identity;

namespace api.Services;

public interface IAuthService
{
    Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto);
    Task<IdentityResult> RegisterUserFromAdminAsync(RegisterFromAdminDto registerFromAdminDto);
    Task<(bool Result, string Token)> LoginAsync(LoginDto loginDto);
    Task<bool> Logout();
    Task<bool> DeleteUserAsync(string username);
    Task<bool> DeleteUserAdminAsync(string username);
    Task<bool> UpdateClientAsync(UpdateUserDto updateUserDto);
    Task<bool> UpdateHealthcareWorkerAsync(UpdateUserDto updateUserDto);
    Task<string> GenerateJwtTokenAsync(AuthUser user);
}