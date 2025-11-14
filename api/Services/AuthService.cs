using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using api.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
namespace api.Services;

public class AuthService: IAuthService{
        private readonly UserManager<AuthUser> _userManager;
        private readonly SignInManager<AuthUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        //private readonly AppDbContext _context;

    public AuthService(
        UserManager<AuthUser> userManager,
        SignInManager<AuthUser> signInManager,
        IConfiguration configuration,
        ILogger<AuthService> logger
        )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }
    private bool IsAuthorized(string?authUserId, string? operationAuthUserId, string? role)
    {
        if (string.IsNullOrEmpty(authUserId)) return false;

        var ok = false;

        if (role == "Admin")
        {
            ok = true;
        }
        else if (role == "Client" && authUserId == operationAuthUserId)
        {
            ok = true;
        }
        else if (role == "HealthcareWorker" && authUserId == operationAuthUserId)
        {
            ok = true;
        }
        return ok;
    }

        public async Task<IdentityResult> RegisterAdminAsync(RegisterDto registerDto, bool isAdmin) //self registration for clients users
    {
        var user = new AuthUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
        };
        var Password = registerDto.Password;
        if(!isAdmin)
        {
            _logger.LogWarning("[AuthService] unauthorized admin registration attempt for {Username}", user.Email);
            throw new UnauthorizedAccessException("Only Admin users can register new Admin users.");
        }
        try
        {
            var result = await _userManager.CreateAsync(user, Password); //create user in AspNetUsers table 
            try
            {
                await _userManager.AddToRoleAsync(user, "Admin"); //assign Client role to user
                _logger.LogInformation("[AuthService] Admin user registered successfully for {Username}", user.Email);
                return result;
            }
            catch (Exception ex) //log any errors during role assignment
            {
                _logger.LogError(ex, "[AuthService] Error assigning Admin role to user {Username}", user.Email);
                // If role assignment fails, delete the created user to maintain data consistency
                await _userManager.DeleteAsync(user);
                return IdentityResult.Failed(new IdentityError { Description = "Error assigning Admin role." });
            }
        }
        catch (Exception ex) //log any errors during user creation
        {
            _logger.LogError(ex, "[AuthService] Error creating Admin user {Username}", user.Email);
            return IdentityResult.Failed(new IdentityError { Description = "Error creating Admin user." });
        }
    }

    public async Task<IdentityResult> RegisterClientAsync(AuthUser user, string Password) //self registration for clients users
    {
        try
        {
            var result = await _userManager.CreateAsync(user, Password); //create user in AspNetUsers table 
            try
            {
                await _userManager.AddToRoleAsync(user, "Client"); //assign Client role to user
                _logger.LogInformation("[AuthService] Client user registered successfully for {Username}", user.Email);
                return result;
            }
            catch (Exception ex) //log any errors during role assignment
            {
                _logger.LogError(ex, "[AuthService] Error assigning Client role to user {Username}", user.Email);
                // If role assignment fails, delete the created user to maintain data consistency
                await _userManager.DeleteAsync(user);
                return IdentityResult.Failed(new IdentityError { Description = "Error assigning Client role." });
            }
        }
        catch (Exception ex) //log any errors during user creation
        {
            _logger.LogError(ex, "[AuthService] Error creating Client user {Username}", user.Email);
            return IdentityResult.Failed(new IdentityError { Description = "Error creating Client user." });
        }
    }

    public async Task<IdentityResult> RegisterWorkerAsync(AuthUser user, string Password, bool isAdmin) //self registration for clients users
    {
        
        try
        {
            var result = await _userManager.CreateAsync(user, Password); //create user in AspNetUsers table 
            try
            {
                await _userManager.AddToRoleAsync(user, "HealthcareWorker"); //assign Client role to user
                _logger.LogInformation("[AuthService] HealthcareWorker user registered successfully for {Username}", user.Email);
                return result;
            }
            catch (Exception ex) //log any errors during role assignment
            {
                _logger.LogError(ex, "[AuthService] Error assigning HealthcareWorker role to user {Username}", user.Email);
                // If role assignment fails, delete the created user to maintain data consistency
                await _userManager.DeleteAsync(user);
                return IdentityResult.Failed(new IdentityError { Description = "Error assigning HealthcareWorker role." });
            }
        }
        catch (Exception ex) //log any errors during user creation
        {
            _logger.LogError(ex, "[AuthService] Error creating HealthcareWorker user {Username}", user.Email);
            return IdentityResult.Failed(new IdentityError { Description = "Error creating HealthcareWorker user." });
        }
    }

    public async Task<(bool Result, string Token)> LoginAsync(LoginDto loginDto) //login method returning success status and JWT token
    {
        var user = await _userManager.FindByNameAsync(loginDto.Username); //find user by username
        if (user == null) //log user not found
        {
            _logger.LogWarning("[AuthAPIController] login failed for {Username}: user not found", loginDto.Username);
            return (false, string.Empty);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false); //check password
        if (result.Succeeded) //log successful login and generate JWT token
        {
            _logger.LogInformation("[AuthAPIController] user logged in successfully for {Username}", loginDto.Username);
            var token = GenerateJwtTokenAsync(user);
            return (true, await token);
        }

        _logger.LogWarning("[AuthAPIController] login failed for {Username}: invalid password", loginDto.Username); //log invalid password
        return (false, string.Empty);
    }

    public async Task<bool> Logout() //logout method
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("[AuthAPIController] user logged out successfully");
        return true;
    }

    public async Task<bool> DeleteUserAsync(string authId, string operationAuthUserId, string role) //method to delete user by AuthId
    {
        var user = await _userManager.FindByIdAsync(authId); //find user by AuthId
        if (user == null) //log user not found
        {
            _logger.LogWarning("[AuthService] delete user failed for {AuthId}: user not found", authId);
            return false;
        }
        if(!IsAuthorized(authId, operationAuthUserId, role)) //check if authorized to delete user
        {
            _logger.LogWarning("[AuthService] unauthorized delete attempt by {OperationAuthId} for user {AuthId}", operationAuthUserId, authId);
            throw new UnauthorizedAccessException("You are not authorized to delete this user.");
        }
        var result = await _userManager.DeleteAsync(user); //delete user
        if (result.Succeeded) //log successful deletion
        {
            _logger.LogInformation("[AuthService] user deleted successfully for {AuthId}", authId);
            return true;
        }
        else
        {
            _logger.LogWarning("[AuthService] delete user failed for {AuthId}: {Errors}",authId, result.Errors); //log deletion failure
            return false;
        }
    }
    
//Authorize update methods to ensure only authorized users can update their details.

    public async Task<bool> UpdateUserAsync(UpdateUserDto userDto, string authId, string operationAuthUserId, string role) //method to update user details
    {
        if (string.IsNullOrEmpty(authId)) // check if authId is null or empty
        {
            _logger.LogWarning("[AuthService] update user failed: authId is null or empty");
            return false;
        }
        var user = await _userManager.FindByIdAsync(authId); //find user by userId
        if (user == null) //log user not found
        {
            _logger.LogWarning("[AuthService] update user failed for {UserId}: user not found", authId);
            return false;
        }
        if(!IsAuthorized(authId, operationAuthUserId, role)) //check if authorized to update user
        {
            _logger.LogWarning("[AuthService] unauthorized update attempt by {OperationAuthId} for user {AuthId}", operationAuthUserId, authId);
            throw new UnauthorizedAccessException("You are not authorized to update this user.");
        }
        try
        {
            if (!string.IsNullOrEmpty(userDto.Email) && userDto.Email != user.Email) //update email if provided and different
            {
                user.Email = userDto.Email;
                user.UserName = userDto.Email; //assuming username is same as email
                var updateResult = await _userManager.UpdateAsync(user); //update user details
                if (!updateResult.Succeeded) //log update failure
                {
                    _logger.LogWarning("[AuthService] update user failed for {UserId}: {Errors}", authId, updateResult.Errors);
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(userDto.Password)) //update password if provided
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, userDto.Password);
                if (!passwordResult.Succeeded) //log password update failure
                {
                    _logger.LogWarning("[AuthService] password update failed for {UserId}: {Errors}", authId, passwordResult.Errors);
                    return false;
                }
            }
        }
        catch (Exception ex) //log any errors during user update
        {
            _logger.LogError(ex, "[AuthService] Error updating User for {UserId}", authId);
            return false;
        }

        _logger.LogInformation("[AuthService] user updated successfully for {UserId}", authId); //log successful update
        return true;
    }
    private async Task<string> GenerateJwtTokenAsync(AuthUser user) //method to generate JWT token for authenticated user
    {
        var jwtKey = _configuration["Jwt:Key"]; // The secret key used for the signature
        if (string.IsNullOrEmpty(jwtKey)) // Ensure the key is not null or empty
        {
            _logger.LogError("[AuthAPIController] JWT key is missing from configuration.");
            throw new InvalidOperationException("JWT key is missing from configuration.");
        }
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)); // Reading the key from the configuration
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); // Using HMAC SHA256 algorithm for signing the token

        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!), // optional
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName!), // optional
                new Claim(JwtRegisteredClaimNames.Email, user.Email!), // Unique identifier for the user
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique identifier for the token
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()) // Issued at timestamp
            };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var r in roles)
        {
            // you configured RoleClaimType = ClaimTypes.Role in JwtBearer options
            claims.Add(new Claim(ClaimTypes.Role, r));
            // If you ever switch to using "role" in tokens, set RoleClaimType = "role" in JwtBearer options instead.
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(120), // Token expiration time set to 120 minutes
            signingCredentials: credentials); // Signing the token with the specified credentials

        _logger.LogInformation("[AuthAPIController] JWT token created for {@username}", user.UserName);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
