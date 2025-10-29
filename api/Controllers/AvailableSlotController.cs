using api.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using api.Services;
using Microsoft.AspNetCore.Authorization;

namespace api.Controllers;

[Authorize(Roles = "HealthcareWorker,Admin")]
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

    // Private method for getting Admin and AuthUserId for Authorization
    // It returns if the user is an Admin and the Id from the AuthUser
    private (bool isAdmin, string? authUserId) UserContext()
    {
        return (User.IsInRole("Admin"), User.FindFirstValue(ClaimTypes.NameIdentifier));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var (isAdmin, authUserId) = UserContext();
        var slots = await _service.GetAll(isAdmin, authUserId);
        if (!slots.Any()) return NotFound("No available slots found.");
        return Ok(slots);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var (isAdmin, authUserId) = UserContext();
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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AvailableSlotDto slotDto)
    {
        var (isAdmin, authUserId) = UserContext();
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AvailableSlotDto slotDto)
    {
        if (id != slotDto.Id) return BadRequest("ID mismatch");
        var (isAdmin, authUserId) = UserContext();

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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (isAdmin, authUserId) = UserContext();

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
