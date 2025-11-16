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
            _logger.LogWarning(ex, "[HealthcareWorkerController] Error retrieving all healthcare workers");
            return StatusCode(500, "A problem happened while handling your request."); // Return 500 Internal Server Error if exception occurs
        }
    }

    [Authorize(Roles = "Admin, HealthcareWorker")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) // Get healthcare worker by Id
    {
        var (role, authUserId) = UserContext(); // Get role and AuthUserId from JWT token
        try
        {
              var worker = await _service.GetById(id, authUserId!, role!); // Get healthcare worker from database through service
            if (worker == null)
            {
                _logger.LogWarning("[HealthcareWorkerController] Healthcare worker not found for Id {Id:0000}", id); 
                return NotFound("Healthcare worker not found"); // Return 404 Not Found if healthcare worker not found
            }
            return Ok(worker); // Return 200 OK with healthcare worker
        } 
        catch (UnauthorizedAccessException) // Handles different exceptions like unauthorized users. 
        {
            _logger.LogWarning("[HealthcareWorkerController] Unauthorized access attempt to get HealthcareWorkerId {HealthcareWorkerId:0000}", id);
            return Forbid(); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HealthcareWorkerController] Error retrieving healthcare worker for Id {Id:0000}", id);
            return StatusCode(500, "A problem happened while handling your request."); // Return 500 Internal Server Error if exception occurs
        }
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
                _logger.LogWarning("[HealthcareWorkerController] Healthcare worker not found for AuthUserId {AuthUserId}", authUserId);
                return NotFound("Healthcare worker not found"); // Return 404 Not Found if healthcare worker not found
            }
            return Ok(worker); // Return 200 OK with healthcare worker
        }
        catch (UnauthorizedAccessException) // Handles different exceptions like unauthorized users. 
        {
            return Forbid(); 
        } catch (Exception ex)
        {
            _logger.LogError(ex, "[HealthcareWorkerController] Error retrieving healthcare worker for AuthUserId {AuthUserId}", authUserId);
            return StatusCode(500, "A problem happened while handling your request."); // Return 500 Internal Server Error if exception occurs
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
            var authCreate = await _authService.RegisterWorkerAsync(authUser, registerDto.Password, isAdmin); // Register healthcare worker in Auth Database
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
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("[HealthcareWorkerController] Unauthorized registration attempt for {Username}", registerDto.Email);
            return Forbid(); // Return 403 Forbidden if unauthorized
        }
        catch (InvalidOperationException)
        {
            return StatusCode(500, "API had a problem while handling your request.");
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HealthcareWorkerController] Unexpected error during registration.");
            return StatusCode(500, "Unexpected error.");
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) // Delete healthcare worker by Id
    {
        var (role, authUserId) = UserContext(); // Get role and AuthUserId from JWT token
        try
        {
             // Get username to delete from Auth Database
            string? deletedAuthUserId = await _service.Delete(id, authUserId!, role!); // Delete worker from App Database
            if (deletedAuthUserId == null)
            {
                return NotFound("Healthcare Worker not found");
            }

            bool authDeleted = await _authService.DeleteUserAsync(deletedAuthUserId, authUserId!, role!); // Delete worker from Auth Database
            if (!authDeleted) // If either deletion fails, throw exception
            {
                return NotFound("Healthcare Worker not found in Auth Database");
            }
            return NoContent(); // Return 204 No Content on successful deletion
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("[HealthcareWorkerController] Unauthorized delete attempt for Healthcare Worker {Id:0000}", id);
            return Forbid(); // Return 403 Forbidden if unauthorized
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
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto updateUserDto) // Update Healthcare Worker information
    {

        var (role, authUserId) = UserContext(); // Get role and AuthUserId from JWT token
        // Check if the id in the route matches the id in the body
        if (id != updateUserDto.Id)
        {
            return BadRequest("ID mismatch");
        }
        try
        {
            var workerUpdate =  await _service.Update(updateUserDto, authUserId!, role!); // Update Healthcare Worker in App Database
            if(workerUpdate == null)
            {
                _logger.LogWarning("[HealthcareWorkerController] Healthcare Worker not found for Id {Id:0000}", id);
                return NotFound("HealthcareWorker not found");
            }
            string authId = workerUpdate; // Get authId to update user in Auth Database
            var authWorkerUpdate = await _authService.UpdateUserAsync(updateUserDto, authId, authUserId!, role!); // Update user in Auth Database
            if(!authWorkerUpdate)
            {
                _logger.LogWarning("[HealthcareWorkerController] Auth User Not found for AuthId {AuthId}", authId);
                return NotFound("Auth User not found");
            }
            return NoContent(); // Return 204 No Content on successful update
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("[HealthcareWorkerController] Unauthorized update attempt for HealthcareworkerId {id:0000}, {@worker}", id, updateUserDto);
            return Forbid(); // Return 403 Forbidden if unauthorized
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[HealthcareWorkerController] Worker update failed for HealthcareworkerId {id:0000}, {@worker}", id, updateUserDto);
            return StatusCode(500, "Failed to update healthcare worker."); // Return 500 Internal Server Error if update fails
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HealthcareWorkerController] Worker update failed for HealthcareworkerId {id:0000}, {@worker}", id, updateUserDto);
            return StatusCode(500, "A problem happened while updating the Healthcare Worker."); // Return 500 Internal Server Error for unexpected errors
        }
    }
}
