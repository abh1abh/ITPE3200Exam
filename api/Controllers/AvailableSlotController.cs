using api.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using api.Services;
using Microsoft.AspNetCore.Authorization;

namespace api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AvailableSlotController : ControllerBase
{
    private readonly ILogger<AvailableSlotController> _logger; // Logger for logging errors and information
    private readonly IAvailableSlotService _service; // Service layer

    public AvailableSlotController(IAvailableSlotService service, ILogger<AvailableSlotController> logger)
    {
        _logger = logger;
        _service = service;

    }

    // Private helper method to get role and AuthUserId based on the JWT token the request received. 
    private (string? role, string? authUserId) UserContext()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role); // Specified Role when creating the JWT token
        return (role, userId);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll() // Admin can see all slots
    {
        var (role, _) = UserContext(); 
        bool isAdmin = role == "Admin";
        var slots = await _service.GetAll(isAdmin); // Calls service layer to get all slots
        return Ok(slots);
    }

    [Authorize(Roles = "Admin,Client")]
    [HttpGet("unbooked")]
    public async Task<IActionResult> GetAllUnbooked() // Admin and Clients can see unbooked slots for appointment booking
    {
        var slots = await _service.GetAllUnbooked();
        return Ok(slots);
    }


    [HttpGet("mine")]
    [Authorize(Roles = "HealthcareWorker,Admin")]
    public async Task<IActionResult> GetAllByWorkerId() // Healthcare workers can see their own slots
    {
        var (_, authUserId) = UserContext();
        var slots = await _service.GetAllByWorkerId(authUserId); // Calls service layer to get slots by worker id
        return Ok(slots);
    }

    [Authorize(Roles = "HealthcareWorker,Admin")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)  // Healthcare workers and Admins can get slot by ID
    {
        try
        {
            var (role, authUserId) = UserContext();
            bool isAdmin = role == "Admin";
            var dto = await _service.GetById(id, isAdmin, authUserId); // Calls service layer to get slot by ID
            if (dto is null) return NotFound("Available slot not found");
            return Ok(dto);
        }
        catch (UnauthorizedAccessException) // Handles unauthorized access
        {
            _logger.LogWarning("[AvailableSlotController] User is not authorized to access available slot ID {Id:0000}", id);
            return Forbid(); // Returns 403 Forbidden if user is not authorized
        }
        catch (Exception ex) // Handles general exceptions
        {
            _logger.LogError(ex, "[AvailableSlotController] Failed to get available slot ID {Id:0000}", id);
            return StatusCode(500, "A problem occurred while fetching available slot.");
        }

    }
    
    [Authorize(Roles = "HealthcareWorker,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AvailableSlotDto slotDto) // Healthcare workers and Admins can create slots
    {
        var (role, authUserId) = UserContext();
        bool isAdmin = role == "Admin";
        try
        {
            var created = await _service.Create(slotDto, isAdmin, authUserId); // Calls service layer to create slot
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (UnauthorizedAccessException) // Handles unauthorized access
        {
            _logger.LogWarning("[AvailableSlotController] User is not authorized to create available slot");
            return Forbid();
        }
        catch (InvalidOperationException)  // Handles operation exceptions
        {
            return StatusCode(500, "Internal error creating the available slot"); // Returns 500 at operation exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AvailableSlotController] create failed");
            return StatusCode(500, "Failed to create slot.");
        }
    }

    [Authorize(Roles = "HealthcareWorker,Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AvailableSlotDto slotDto) // Healthcare workers and Admins can update slots
    {
        if (id != slotDto.Id) return BadRequest("ID mismatch"); // Checks if id and slot id are the same
        var (role, authUserId) = UserContext(); 
        bool isAdmin = role == "Admin";
        try
        {
            var ok = await _service.Update(id, slotDto, isAdmin, authUserId); // Calls service layer to update slot
            if (!ok) return NotFound();
            return NoContent(); // Returns 204 No Content if update is successful
        }
        catch (UnauthorizedAccessException) // Handles unauthorized access
        {
            _logger.LogWarning("[AvailableSlotController] User is not authorized to access available slot");
            return Forbid();
        }
        catch (ArgumentException e) // Handles bad input exceptions
        {

            _logger.LogError(e, "[AvailableSlotController] Arguments missing");
            return BadRequest(e.Message);
        }
        catch (InvalidOperationException) // Handles operation exceptions
        {
            return StatusCode(500, "Internal error updating the available slot.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AvailableSlotController] update failed");
            return StatusCode(500, "Failed to update slot.");
        }
    }

    [Authorize(Roles = "HealthcareWorker,Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) // Healthcare workers and Admins can delete slots
    {
        var (role, authUserId) = UserContext();
        bool isAdmin = role == "Admin";
        try
        {
            var ok = await _service.Delete(id, isAdmin, authUserId); // Calls service layer to delete slot

            // If delete failed return 500 Internal Server Error
            if (!ok) 
            {
                _logger.LogWarning("[AvailableSlotController] Delete operation failed for available slot id {ID:0000}", id); 
                return StatusCode(500, "Failed to delete slot.");
            }
            return NoContent(); // Returns 204 No Content if delete is successful

        }
        catch (UnauthorizedAccessException) // Handles unauthorized access
        {
            _logger.LogWarning("[AvailableSlotController] User is not authorized to access available slot");
            return Forbid(); // Returns 403 Forbidden if user is not authorized
        }
        catch (ArgumentException e) // Handles bad input exceptions
        {

            _logger.LogWarning("[AvailableSlotController] Arguments missing");
            return BadRequest(e.Message);
        }
        catch (Exception ex) // Handles general exceptions
        {
            _logger.LogError(ex, "[AvailableSlotController] deleted failed for available slot id {ID:0000}", id);
            return StatusCode(500, "Failed to delete slot."); // Returns 500 Internal Server Error for general exceptions
        }
    }    

}
