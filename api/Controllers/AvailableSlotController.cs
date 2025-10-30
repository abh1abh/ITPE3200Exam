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
    private readonly ILogger<AvailableSlotController> _logger;
    private readonly IAvailableSlotService _service;

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
    public async Task<IActionResult> GetAll()
    {
        var (role, _) = UserContext();
        bool isAdmin = role == "Admin";
        var slots = await _service.GetAll(isAdmin);
        return Ok(slots);
    }

    [Authorize(Roles = "Admin,Client")]
    [HttpGet("unbooked")]
    public async Task<IActionResult> GetAllUnbooked()
    {
        var slots = await _service.GetAllUnbooked();
        return Ok(slots);
    }


    [HttpGet("mine")]
    [Authorize(Roles = "HealthcareWorker,Admin")]
    public async Task<IActionResult> GetAllByWorkerId()
    {
        var (_, authUserId) = UserContext();
        var slots = await _service.GetAllByWorkerId(authUserId);
        return Ok(slots);
    }

    [Authorize(Roles = "HealthcareWorker,Admin")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var (role, authUserId) = UserContext();
            bool isAdmin = role == "Admin";
            var dto = await _service.GetById(id, isAdmin, authUserId);
            if (dto is null) return NotFound("Available slot not found");
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AvailableSlotController] Failed to get available slot ID {Id:0000}", id);
            return StatusCode(500, "A problem occurred while fetching available slot.");
        }

    }
    
    [Authorize(Roles = "HealthcareWorker,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AvailableSlotDto slotDto)
    {
        var (role, authUserId) = UserContext();
        bool isAdmin = role == "Admin";
        try
        {
            var created = await _service.Create(slotDto, isAdmin, authUserId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AvailableSlotController] create failed");
            return StatusCode(500, "Failed to create slot.");
        }
    }

    [Authorize(Roles = "HealthcareWorker,Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AvailableSlotDto slotDto)
    {
        if (id != slotDto.Id) return BadRequest("ID mismatch");
        var (role, authUserId) = UserContext();
        bool isAdmin = role == "Admin";
        try
        {
            var ok = await _service.Update(id, slotDto, isAdmin, authUserId);
            if (!ok) return NotFound();
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogError("[AvailableSlotController] User is not authorized to access available slot");
            return Forbid(); 
        }
        catch (ArgumentException e)
        {
            
            _logger.LogError(e, "[AvailableSlotController] Arguments missing");
            return BadRequest(e.Message); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AvailableSlotController] update failed");
            return StatusCode(500, "Failed to update slot.");
        }
    }

    [Authorize(Roles = "HealthcareWorker,Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (role, authUserId) = UserContext();
        bool isAdmin = role == "Admin";
        try
        {
            var ok = await _service.Delete(id, isAdmin, authUserId);
            if (!ok)
            {
                NotFound();
            }
            return NoContent();

        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("[AvailableSlotController] User is not authorized to access available slot");
            return Forbid();
        }
        catch (ArgumentException e)
        {

            _logger.LogError(e, "[AvailableSlotController] Arguments missing");
            return BadRequest(e.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AvailableSlotController] deleted failed for available slot id {ID:0000}", id);
            return StatusCode(500, "Failed to update slot.");
        }
    }    

}
