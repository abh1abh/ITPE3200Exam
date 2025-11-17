using Microsoft.AspNetCore.Mvc;
using api.DTO;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController( IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        private (string? role, string? authUserId) UserContext() // Get role and AuthUserId from JWT token
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // AuthUserId when creating the JWT token
            var role = User.FindFirstValue(ClaimTypes.Role); // Specified Role when creating the JWT token
            return (role, userId);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto) //User registration
        {
            var (role, _) = UserContext(); // Get role from JWT token
            bool isAdmin = role == "Admin"; // Check if the user is an Admin
            try
            {
                await _authService.RegisterAdminAsync(registerDto, isAdmin); //Call the auth service to register user
            }
            catch (UnauthorizedAccessException) //Handle unauthorized access
            {
                _logger.LogWarning("[AuthAPIController] unauthorized admin registration attempt for {Username}", registerDto.Email);
                return Forbid(); //Return 403 Forbidden
            }
            catch (Exception ex) //Log any errors during registration
            {
                _logger.LogError(ex, "[AuthAPIController] error during registration for {Username}", registerDto.Email);
                return StatusCode(500, "A problem happened while handling your request."); //Internal Server Error
            }
            _logger.LogInformation("[AuthAPIController] user registered successfully for {Username}", registerDto.Email);
            return Ok(new { Message = "User registered successfully" }); //Return success message
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto) //User login
        {
            var (result, token) = await _authService.LoginAsync(loginDto); //Call the auth service to login user
            if (result) //Log successful login
            {
                _logger.LogInformation("[AuthAPIController] user logged in successfully for {Username}", loginDto.Username);
                return Ok(new { Token = token, Message = "User logged in successfully" });
            }
            _logger.LogWarning("[AuthAPIController] login failed for {Username}: invalid password", loginDto.Username); //Log failed login
            return Unauthorized(new { Message = "Invalid username or password" }); //Return unauthorized message
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout() //User logout
        {
            await _authService.Logout(); //Call the auth service to logout user
            _logger.LogInformation("[AuthAPIController] user logged out successfully");
            return Ok(new { Message = "User logged out successfully" });
        }
    }
}