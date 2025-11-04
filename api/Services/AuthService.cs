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
        //private readonly AppDbContext _context;
        private readonly IClientService _clientService;
        private readonly IHealthcareWorkerService _healthcareWorkerService;

    public AuthService(
        UserManager<AuthUser> userManager,
        SignInManager<AuthUser> signInManager,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IClientService clientService,
        IHealthcareWorkerService healthcareWorkerService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
        _clientService = clientService;
        _healthcareWorkerService = healthcareWorkerService;
    }
    public async Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto) //self registration for clients users
    {
        var user = new AuthUser //create new user object with AuthUser properties
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
        };
        
        var result = await _userManager.CreateAsync(user, registerDto.Password); //create user in AspNetUsers table
        if (result.Succeeded)
        {
            var client = new ClientDto  //Add client details
            {
                AuthUserId = user.Id,
                Name = registerDto.Name,
                Phone = registerDto.Number,
                Address = registerDto.Address,
                Email = registerDto.Email,
                };
            var createClient =  await _clientService.Create(client); //use ClientService to create client
            var roleResult = await _userManager.AddToRoleAsync(user, "Client"); //assign Client role to user
            _logger.LogInformation("[AuthService] user registered successfully for {Username}", registerDto.Email);
        }
        else //log registration failure
        {
            _logger.LogWarning("[AuthService] user registration failed for {Username}: {Errors}", registerDto.Email, result.Errors);
        }
        return result; //return result of registration attempt
        }

    public async Task<IdentityResult> RegisterUserFromAdminAsync(RegisterFromAdminDto registerDto) //registration by admin for both clients and healthcare workers
    {
        var user = new AuthUser //create new user object with AuthUser properties
        {
            UserName = registerDto.Email,
            Email = registerDto.Email
        };
        var userRole = registerDto.Role; //determine role from DTO

        if (userRole == "Client") //Check role and create client entity
        {
            try
            {
                var client = new ClientDto //Add client details from DTO
                {
                    AuthUserId = user.Id,
                    Name = registerDto.Name,
                    Phone = registerDto.Number,
                    Address = registerDto.Address,
                    Email = registerDto.Email,
                };
                var createClient =  await _clientService.Create(client); //use ClientService to create client
                var result = await _userManager.CreateAsync(user, registerDto.Password); //create user in AspNetUsers table
                var roleResult = await _userManager.AddToRoleAsync(user, "Client"); //assign Client role to user
                _logger.LogInformation("[AuthService] Client user registered successfully for {Username}", registerDto.Email);
                return result;
            }
            catch (Exception ex) //log any errors during client creation
            {
                _logger.LogError(ex, "[AuthService] Error creating Client for {Username}", registerDto.Email);
                return IdentityResult.Failed(new IdentityError { Description = "Error creating Client." });
            }
        }
        else if (userRole == "HealthcareWorker") //Check role and create Healthcare worker entity
        {
            try
            {
                var worker = new HealthcareWorkerDto //Add healthcare worker details from DTO
                {
                    AuthUserId = user.Id,
                    Name = registerDto.Name,
                    Phone = registerDto.Number,
                    Address = registerDto.Address,
                    Email = registerDto.Email,
                };
                var createWorker =  await _healthcareWorkerService.Create(worker); //use HealthcareWorkerService to create healthcare worker
                var result = await _userManager.CreateAsync(user, registerDto.Password); //create user in AspNetUsers table
                var roleResult = await _userManager.AddToRoleAsync(user, "HealthcareWorker"); //assign HealthcareWorker role to user
                _logger.LogInformation("[AuthService] Worker user registered successfully for {Username}", registerDto.Email);
                return result;
            }
            catch (Exception ex) //log any errors during healthcare worker creation
            {
                _logger.LogError(ex, "[AuthService] Error creating HealthcareWorker for {Username}", registerDto.Email);
                return IdentityResult.Failed(new IdentityError { Description = "Error creating HealthcareWorker." });
            }
        }
        else if(userRole == "Admin") //Check role and create Admin user
        {
            var result = await _userManager.CreateAsync(user, registerDto.Password); //create user in AspNetUsers table
            var roleResult = await _userManager.AddToRoleAsync(user, "Admin"); //assign Admin role to user
            _logger.LogInformation("[AuthService] admin user registered successfully for {Username}", registerDto.Email);
            return result;
        }

        else //log invalid role error
        {
            _logger.LogWarning("[AuthService] user registration failed for {Username}: invalid role {Role}", registerDto.Email, userRole);
            return IdentityResult.Failed(new IdentityError { Description = "Invalid role specified." });
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

        public async Task<bool> DeleteUserAsync(string username) //method to delete user by username
        {
            var user = await _userManager.FindByNameAsync(username); //find user by username
            if (user == null) //log user not found
            {
                _logger.LogWarning("[AuthService] delete user failed for {Username}: user not found", username);
                return false;
            }

            var result = await _userManager.DeleteAsync(user); //delete user
            if (result.Succeeded) //log successful deletion
            {
                _logger.LogInformation("[AuthService] user deleted successfully for {Username}", username);
                return true;
            }

            _logger.LogWarning("[AuthService] delete user failed for {Username}: {Errors}", username, result.Errors); //log deletion failure
            return false;
        }

        public async Task<bool> DeleteUserAdminAsync(string username) //method to delete user by admin
        {
            var user = await _userManager.FindByNameAsync(username); //find user by username
            if (user == null) //log user not found
            {
                _logger.LogWarning("[AuthService] admin delete user failed for {Username}: user not found", username);
                return false;
            }

            var result = await _userManager.DeleteAsync(user); //delete user
            if (result.Succeeded) //log successful deletion
            {
                _logger.LogInformation("[AuthService] admin deleted user successfully for {Username}", username);
                return true;
            }

            _logger.LogWarning("[AuthService] admin delete user failed for {Username}: {Errors}", username, result.Errors); //log deletion failure
            return false;
        }


    public async Task<string> GenerateJwtTokenAsync(AuthUser user) //method to generate JWT token for authenticated user
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
