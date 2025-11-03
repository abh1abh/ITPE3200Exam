using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using api.DTO;
using api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Services;
using api.DAL;
namespace api.Services;

public class AuthService: IAuthService{
        private readonly UserManager<AuthUser> _userManager;
        private readonly SignInManager<AuthUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly AppDbContext _context;

        public AuthService(
        UserManager<AuthUser> userManager,
        SignInManager<AuthUser> signInManager,
        IConfiguration configuration,
        AppDbContext context,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
        _logger = logger;
    }
    public async Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto) //self registration for clients users
    {
        var user = new AuthUser
        {
            UserName = registerDto.Username,
            Email = registerDto.Email,
        };
        
        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (result.Succeeded)
        {
            var client = new Client  //Add client details
            {
                AuthUserId = user.Id,
                Name = registerDto.Name,
                Phone = registerDto.Number,
                Address = registerDto.Address,
                Email = registerDto.Email,
                };
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            var roleResult = await _userManager.AddToRoleAsync(user, "Client");
            _logger.LogInformation("[AuthService] user registered successfully for {Username}", registerDto.Username);
        }
        else
        {
            _logger.LogWarning("[AuthService] user registration failed for {Username}: {Errors}", registerDto.Username, result.Errors);
        }
        return result; 
        }

    public async Task<IdentityResult> RegisterUserFromAdminAsync(RegisterFromAdminDto registerDto)
    {
        var user = new AuthUser
        {
            UserName = registerDto.Username,
            Email = registerDto.Email
        };
        var userRole = registerDto.Role;

        if (userRole == "Client")
        {
            try
            {
                var client = new Client
                {
                    AuthUserId = user.Id,
                    Name = registerDto.Name,
                    Phone = registerDto.Number,
                    Address = registerDto.Address,
                    Email = registerDto.Email,
                };
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
                var result = await _userManager.CreateAsync(user, registerDto.Password);
                var roleResult = await _userManager.AddToRoleAsync(user, "Client");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error creating Client for {Username}", registerDto.Username);
                return IdentityResult.Failed(new IdentityError { Description = "Error creating Client." });
            }
        }
        else if (userRole == "HealthcareWorker")
        {
            try
            {
                var worker = new HealthcareWorker
                {
                    AuthUserId = user.Id,
                    Name = registerDto.Name,
                    Phone = registerDto.Number,
                    Address = registerDto.Address,
                    Email = registerDto.Email,
                };
                _context.HealthcareWorkers.Add(worker);
                await _context.SaveChangesAsync();
                var result = await _userManager.CreateAsync(user, registerDto.Password);
                var roleResult = await _userManager.AddToRoleAsync(user, "HealthcareWorker");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error creating HealthcareWorker for {Username}", registerDto.Username);
                return IdentityResult.Failed(new IdentityError { Description = "Error creating HealthcareWorker." });
            }
        }
        else
        {
            _logger.LogWarning("[AuthService] user registration failed for {Username}: invalid role {Role}", registerDto.Username, userRole);
            return IdentityResult.Failed(new IdentityError { Description = "Invalid role specified." });
        }
    }


        public async Task<(bool Result, string Token)> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user == null)
            {
                _logger.LogWarning("[AuthAPIController] login failed for {Username}: user not found", loginDto.Username);
                return (false, string.Empty);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (result.Succeeded)
            {
                _logger.LogInformation("[AuthAPIController] user logged in successfully for {Username}", loginDto.Username);
                var token = GenerateJwtTokenAsync(user);
                return (true, await token);
            }

            _logger.LogWarning("[AuthAPIController] login failed for {Username}: invalid password", loginDto.Username);
            return (false, string.Empty);
        }

        public async Task<bool> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("[AuthAPIController] user logged out successfully");
            return true;
        }


        public async Task<string> GenerateJwtTokenAsync(AuthUser user)
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
