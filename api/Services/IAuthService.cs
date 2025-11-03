using api.DTO;
using Microsoft.AspNetCore.Identity;

namespace api.Services;

public interface IAuthService
{
    Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto);
    Task<IdentityResult> RegisterUserFromAdminAsync(RegisterFromAdminDto registerFromAdminDto);
    Task<(bool Result, string Token)> LoginAsync(LoginDto loginDto);
    Task<bool> Logout();
    Task<string> GenerateJwtTokenAsync(AuthUser user);
}