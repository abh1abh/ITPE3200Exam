using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using api.DTO;
using api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Services;
using Microsoft.AspNetCore.Authorization;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto) //self registration for clients users
        {
            var result = await _authService.RegisterUserAsync(registerDto); //Call the auth service to register user
            if (result.Succeeded) //Log successful registration
            {
                _logger.LogInformation("[AuthAPIController] user registered successfully for {Username}", registerDto.Email);
                return Ok(new { Message = "User registered successfully" });
            }
            _logger.LogError("[AuthAPIController] user registration failed for {Username}: {Errors}", registerDto.Email, result.Errors); //Log failed registration
            return BadRequest(result.Errors);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterFromAdmin([FromBody] RegisterFromAdminDto registerDto) 
        //Registration from Admin for any role. This is to ensure only admin can create other users with elevated roles.
        {
            var result = await _authService.RegisterUserFromAdminAsync(registerDto); //Call the auth service to register user
            if (result.Succeeded) //Log successful registration
            {
                _logger.LogInformation("[AuthAPIController] admin registered user successfully for {Username}", registerDto.Email);
                return Ok(new { Message = "User registered successfully by admin" });
            }
            //Log failed registration
            _logger.LogError("[AuthAPIController] admin user registration failed for {Username}: {Errors}", registerDto.Email, result.Errors);
            return BadRequest(result.Errors);
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
            _logger.LogWarning("[AuthAPIController] login failed for {Username}: invalid password", loginDto.Username);
            //Log failed login
            return Unauthorized(new { Message = "Invalid username or password" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout() //User logout
        {
            await _authService.Logout(); //Call the auth service to logout user
            _logger.LogInformation("[AuthAPIController] user logged out successfully");
            return Ok(new { Message = "User logged out successfully" });
        }


        private async Task<string> GenerateJwtToken(AuthUser user) //Generate JWT token for authenticated user
        {
            var token = await _authService.GenerateJwtTokenAsync(user);
            return token;
        }
    }
}