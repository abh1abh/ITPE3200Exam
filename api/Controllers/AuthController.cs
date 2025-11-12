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
        private readonly IClientService _clientService;
        private readonly IHealthcareWorkerService _healthcareWorkerService;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger,
            IClientService clientService,
            IHealthcareWorkerService healthcareWorkerService)
        {
            _authService = authService;
            _logger = logger;
            _clientService = clientService;
            _healthcareWorkerService = healthcareWorkerService;
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
        [HttpPut("client/{id}")]
        public async Task<IActionResult> UpdateClient([FromBody] UpdateUserDto updateUserDto) //Update client information in Auth and client Db
        {
            int id = updateUserDto.Id;
            ClientDto? client = await _clientService.GetById(id); //Find client in App Database
            if (client == null)
            {
                return NotFound("Client not found");
            }
            if (id != updateUserDto.Id)
            {
                return BadRequest("ID mismatch");
            }
            try
            {
                var authClientUpdate = await _authService.UpdateClientAsync(updateUserDto);
                return Ok(authClientUpdate);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "[AuthController] Client update failed for ClientId {id:0000}, {@client}", id, updateUserDto);
                return StatusCode(500, "Failed to update client.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthController] Client update failed for ClientId {id:0000}, {@client}", id, updateUserDto);
                return StatusCode(500, "A problem happened while updating the Client.");
            }
        }  

        [HttpPut("worker/{id}")]
        public async Task<IActionResult> UpdateWorker([FromBody] UpdateUserDto updateUserDto) //Update healthcare worker information in Auth and worker Db
        {
            int id = updateUserDto.Id;
            HealthcareWorkerDto? worker = await _healthcareWorkerService.GetById(id); //Find healthcare worker in App Database
            if (worker == null)
            {
                return NotFound("HealthcareWorker not found");
            }
            if (id != updateUserDto.Id)
            {
                return BadRequest("ID mismatch");
            }
            try
            {   
                var authWorkerUpdate = await _authService.UpdateHealthcareWorkerAsync(updateUserDto); //Update healthcare worker in Auth Database
                return Ok(authWorkerUpdate);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "[AuthController] Worker update failed for HealthcareWorkerId {id:0000}, {@worker}", id, updateUserDto);
                return StatusCode(500, "Failed to update healthcare worker.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthController] Healthcare Worker update failed for Healthcare Worker {id:0000}, {@worker}", id, updateUserDto);
                return StatusCode(500, "A problem happened while updating the Healthcare Worker.");
            }
        }    
    }
}