using api.DTO;
using Microsoft.AspNetCore.Identity;

namespace api.Services;

public interface IAuthService
{
    Task<IdentityResult> RegisterClientAsync(AuthUser authUser, string Password);
    Task<IdentityResult> RegisterWorkerAsync(AuthUser authUser, string Password, bool isAdmin);
    Task<IdentityResult> RegisterAdminAsync(RegisterDto registerDto, bool isAdmin);
    Task<(bool Result, string Token)> LoginAsync(LoginDto loginDto);
    Task<bool> Logout();
    Task<bool> DeleteUserAsync(string username, string operationAuthUserId, string role);
    Task<bool> UpdateUserAsync(UpdateUserDto updateUserDto, string authId, string role, string operationAuthUserId);
}