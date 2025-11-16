using api.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Services;
using System.Security.Claims;

namespace api.Controllers;

[Authorize(Roles = "Admin, HealthcareWorker")]
[ApiController]
[Route("api/[controller]")]
public class HealthcareWorkerController : ControllerBase
{
    private readonly IHealthcareWorkerService _service;
    private readonly ILogger<HealthcareWorkerController> _logger;
    private readonly IAuthService _authService;

    public HealthcareWorkerController(IHealthcareWorkerService service, ILogger<HealthcareWorkerController> logger, IAuthService authService)
    {
        _service = service;
        _logger = logger;
        _authService = authService;
    }

    private (string? role, string? authUserId) UserContext() // Get role and AuthUserId from JWT token
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // AuthUserId
        var role = User.FindFirstValue(ClaimTypes.Role); // Specified Role when creating the JWT token
        return (role, userId);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll() // Get all healthcare workers
    {
        var (role, _) = UserContext(); // Get role from JWT token
        bool isAdmin = role == "Admin"; // Check if user is Admin

        try
        {
            var healthcareWorkers = await _service.GetAll(isAdmin); // Get all healthcare workers from database through service
            return Ok(healthcareWorkers); // Return 200 OK with healthcare workers
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HealthcareWorkerController] Error retrieving all healthcare workers");
            return StatusCode(500, "A problem happened while handling your request."); // Return 500 Internal Server Error if exception occurs
        }
    }

    [Authorize(Roles = "Admin, HealthcareWorker")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) // Get healthcare worker by Id
    {
        var (role, authUserId) = UserContext(); // Get role and AuthUserId from JWT token
        var worker = await _service.GetById(id, authUserId!, role!); // Get healthcare worker from database through service
        if (worker == null)
        {
            _logger.LogError("[HealthcareWorkerController] Healthcare worker not found for Id {Id:0000}", id); 
            return NotFound("Healthcare worker not found"); // Return 404 Not Found if healthcare worker not found
        }
        return Ok(worker); // Return 200 OK with healthcare worker
    }

    [Authorize(Roles = "HealthcareWorker")]
    [HttpGet("me")]
    public async Task<IActionResult> GetBySelf() // Get healthcare worker by AuthUserId from JWT token
    {
        var (role, authUserId) = UserContext(); // Get role and AuthUserId
        try
        {
            var worker = await _service.GetByAuthUserId(authUserId!, authUserId!, role!); // Get healthcare worker from database through service
            if (worker == null)
            {
                _logger.LogError("[HealthcareWorkerController] Healthcare worker not found for AuthUserId {AuthUserId}", authUserId);
                return NotFound("Healthcare worker not found"); // Return 404 Not Found if healthcare worker not found
            }
            return Ok(worker); // Return 200 OK with healthcare worker
        }
        catch (UnauthorizedAccessException) // Handles different exceptions like unauthorized users. 
        {
            return Forbid(); 
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterWorker([FromBody] RegisterDto registerDto) // Create a new healthcare worker
    {
        var (role, _) = UserContext(); // Get role from JWT token
        bool isAdmin = role == "Admin"; // Check if user is Admin
        var authUser = new AuthUser // Create AuthUser object for registration
        {
            Email = registerDto.Email,
            UserName = registerDto.Email
        };
        try
        {
            var authCreate = await _authService.RegisterWorkerAsync(authUser, registerDto.Password, isAdmin); // Register client in Auth Database
            try
            {
                var created = await _service.Create(registerDto, authUser.Id, isAdmin); // Create HealthcareWorker in App Database
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created); // Return 201 Created with location header
            }
            catch (Exception)
            {
                // Rollback Auth user creation if App Database creation fails
                await _authService.DeleteUserAsync(authUser.UserName, "Admin", "system");
                throw;
            }
        }
        catch (InvalidOperationException)
        {
            return StatusCode(500, "API had a problem while handling your request.");
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) // Delete healthcare worker by Id
    {
        var (role, authUserId) = UserContext(); // Get role and AuthUserId from JWT token
        var user = await _service.GetById(id, authUserId!, role!); //Find HealthcareWorker in App Database
        if (user == null)
        {
            return NotFound("Healthcare Worker not found");
        }
        try
        {
            string authId = user.AuthUserId!;               //Get username to delete from Auth Database
            bool deleted = await _service.Delete(id, authUserId!, role!);   //Delete client from App Database
            bool authDeleted = await _authService.DeleteUserAsync(authId, authUserId!, role!); //Delete client from Auth Database
            if (!deleted || !authDeleted) //If either deletion fails, throw exception
            {
                throw new InvalidOperationException("Failed to delete Healthcare Worker.");
            }
            return NoContent(); // Return 204 No Content on successful deletion
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[HealthcareWorkerController] Delete failed for Healthcare Worker {Id:0000}", id);
            return StatusCode(500, "Failed to delete Healthcare Worker."); // Return 500 Internal Server Error if deletion fails
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HealthcareWorkerController] Unexpected error deleting Healthcare Worker {Id:0000}", id);
            return StatusCode(500, "Unexpected error."); // Return 500 Internal Server Error for unexpected errors
        }

    }


    [Authorize(Roles = "Admin, HealthcareWorker")]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update([FromBody] UpdateUserDto updateUserDto) // Update Healthcare Worker information
    {
        var (role, authUserId) = UserContext(); // Get role and AuthUserId from JWT token
        int id = updateUserDto.Id;                     //Get Healthcare Worker Id from DTO
        HealthcareWorkerDto? worker = await _service.GetById(id, authUserId!, role!); //Find Healthcare Worker in App Database
        string authId = worker!.AuthUserId!;               //Get authId to update user in Auth Database
        if (worker == null)
        {
            return NotFound("HealthcareWorker not found");
        }
        if (id != worker.Id)
        {
            return BadRequest("ID mismatch");
        }
        try
        {
            var authClientUpdate = await _authService.UpdateUserAsync(updateUserDto, authId, authUserId!, role!); //Update user in Auth Database
            var workerUpdate =  await _service.Update(updateUserDto, authUserId!, role!); //Update Healthcare Worker in App Database
            return Ok(authClientUpdate && workerUpdate);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[HealthcareWorkerController] Client update failed for HealthcareworkerId {id:0000}, {@client}", id, updateUserDto);
            return StatusCode(500, "Failed to update client."); // Return 500 Internal Server Error if update fails
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HealthcareWorkerController] Client update failed for HealthcareworkerId {id:0000}, {@client}", id, updateUserDto);
            return StatusCode(500, "A problem happened while updating the Client."); // Return 500 Internal Server Error for unexpected errors
        }
    }
}
