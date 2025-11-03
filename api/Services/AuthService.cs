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
        var user = new AuthUser //create new user object with AuthUser properties
        {
            UserName = registerDto.Username,
            Email = registerDto.Email,
        };
        
        var result = await _userManager.CreateAsync(user, registerDto.Password); //create user in AspNetUsers table
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
            _context.Clients.Add(client); //add client to Clients table
            await _context.SaveChangesAsync(); //save changes to database
            var roleResult = await _userManager.AddToRoleAsync(user, "Client"); //assign Client role to user
            _logger.LogInformation("[AuthService] user registered successfully for {Username}", registerDto.Username);
        }
        else //log registration failure
        {
            _logger.LogWarning("[AuthService] user registration failed for {Username}: {Errors}", registerDto.Username, result.Errors);
        }
        return result; //return result of registration attempt
        }

    public async Task<IdentityResult> RegisterUserFromAdminAsync(RegisterFromAdminDto registerDto) //registration by admin for both clients and healthcare workers
    {
        var user = new AuthUser //create new user object with AuthUser properties
        {
            UserName = registerDto.Username,
            Email = registerDto.Email
        };
        var userRole = registerDto.Role; //determine role from DTO

        if (userRole == "Client") //Check role and create client entity
        {
            try
            {
                var client = new Client //Add client details from DTO
                {
                    AuthUserId = user.Id,
                    Name = registerDto.Name,
                    Phone = registerDto.Number,
                    Address = registerDto.Address,
                    Email = registerDto.Email,
                };
                _context.Clients.Add(client); //add client to Clients table
                await _context.SaveChangesAsync(); //save changes to database
                var result = await _userManager.CreateAsync(user, registerDto.Password); //create user in AspNetUsers table
                var roleResult = await _userManager.AddToRoleAsync(user, "Client"); //assign Client role to user
                _logger.LogInformation("[AuthService] Client user registered successfully for {Username}", registerDto.Username);
                return result;
            }
            catch (Exception ex) //log any errors during client creation
            {
                _logger.LogError(ex, "[AuthService] Error creating Client for {Username}", registerDto.Username);
                return IdentityResult.Failed(new IdentityError { Description = "Error creating Client." });
            }
        }
        else if (userRole == "HealthcareWorker") //Check role and create Healthcare worker entity
        {
            try
            {
                var worker = new HealthcareWorker //Add healthcare worker details from DTO
                {
                    AuthUserId = user.Id,
                    Name = registerDto.Name,
                    Phone = registerDto.Number,
                    Address = registerDto.Address,
                    Email = registerDto.Email,
                };
                _context.HealthcareWorkers.Add(worker); //add healthcare worker to HealthcareWorkers table
                await _context.SaveChangesAsync(); //save changes to database
                var result = await _userManager.CreateAsync(user, registerDto.Password); //create user in AspNetUsers table
                var roleResult = await _userManager.AddToRoleAsync(user, "HealthcareWorker"); //assign HealthcareWorker role to user
                _logger.LogInformation("[AuthService] Worker user registered successfully for {Username}", registerDto.Username);
                return result;
            }
            catch (Exception ex) //log any errors during healthcare worker creation
            {
                _logger.LogError(ex, "[AuthService] Error creating HealthcareWorker for {Username}", registerDto.Username);
                return IdentityResult.Failed(new IdentityError { Description = "Error creating HealthcareWorker." });
            }
        }
        else if(userRole == "Admin") //Check role and create Admin user
        {
            var result = await _userManager.CreateAsync(user, registerDto.Password); //create user in AspNetUsers table
            var roleResult = await _userManager.AddToRoleAsync(user, "Admin"); //assign Admin role to user
            _logger.LogInformation("[AuthService] admin user registered successfully for {Username}", registerDto.Username);
            return result;
        }

        else //log invalid role error
        {
            _logger.LogWarning("[AuthService] user registration failed for {Username}: invalid role {Role}", registerDto.Username, userRole);
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
