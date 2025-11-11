using System.Security.Claims;
using api.DTO;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[Authorize] // Authorize all relevant roles
[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly ILogger<AppointmentController> _logger; // Logger for logging errors and information
    private readonly IAppointmentService _service; // Service layer 

    public AppointmentController (IAppointmentService service, ILogger<AppointmentController> logger)
    {
        _service = service;
        _logger = logger;
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
        try
        {
            var appointments = await _service.GetAll(); // Get all appointments from service layer
            return Ok(appointments);
        }
        catch (Exception ex) // Handles general exceptions
        {
            _logger.LogError(ex, "[AppointmentController] Error GetAll");
            return StatusCode(500, "A problem occurred while fetching the appointment list."); // Returns 500 at general exceptions 
        }
    }

    [Authorize(Roles = "Client")]
    [HttpGet("client")]
    public async Task<IActionResult> GetAppointmentsByClient() // Get appointments for the logged in client
    {
        var (_, authUserId) = UserContext(); // Get role and AuthUserId
        try
        {
            var appointments = await _service.GetAppointmentsByClientId(authUserId: authUserId); 
            return Ok(appointments);
        }
        catch (UnauthorizedAccessException) // Handles different exceptions like unauthorized users. 
        {
            return Forbid(); // If unauthorized return Forbid
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppointmentController] Error GetAppointmentsByClient");
            return StatusCode(500, "A problem occurred while fetching the appointment list."); // Returns 500 at general exceptions 
        }
    }

    [Authorize(Roles = "HealthcareWorker")]     
    [HttpGet("worker")]
    public async Task<IActionResult> GetAppointmentsByHealthcareWorker() // Get appointments for the logged in healthcare worker
    {
        var (_, authUserId) = UserContext(); // Get role and AuthUserId
        try
        {
            var appointments = await _service.GetAppointmentsByHealthcareWorkerId(authUserId: authUserId); // Uses service layer for all business logic
            return Ok(appointments);
        }
        catch (UnauthorizedAccessException) // Handles different exceptions like unauthorized users. 
        {
            return Forbid(); // If unauthorized return Forbid
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppointmentController] Error GetAppointmentsByHealthcareWorker");
            return StatusCode(500, "A problem occurred while fetching the appointment list."); // Returns 500 at general exceptions 
        }      
    }

    [Authorize(Roles = "Client,Admin,HealthcareWorker")] 
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) // Get appointment by id
    {
        var (role, authUserId) = UserContext(); // Get role and AuthUserId
        try
        {
            var appointmentDto = await _service.GetById(id, role: role, authUserId: authUserId); // Uses service layer for all business logic 
            if (appointmentDto is null) // If service returns null appointment Not Found
            {
                _logger.LogError("[AppointmentController] appointment not found while executing _service.GetById() for AppointmentId {AppointmentId:0000}", id);
                return NotFound("Appointment not found");
            }
            return Ok(appointmentDto);
        }
        catch (UnauthorizedAccessException) // Handles different exceptions like unauthorized users. 
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppointmentController] Error GetById {Id:0000}", id);
            return StatusCode(500, "A problem occurred while fetching the appointment."); // Returns 500 at general exceptions 
        }
    }

    [Authorize(Roles = "Admin,Client")] // Only Admin and Clients are allow to create Appointments 
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AppointmentDto appointmentDto) 
    {
        var (role, authUserId) = UserContext(); // Get role and AuthUserId

        // Handles input validation for at least one Appointment Task 
        appointmentDto.AppointmentTasks = (appointmentDto.AppointmentTasks ?? new()).Where(t => !string.IsNullOrWhiteSpace(t.Description)).ToList();
        if (appointmentDto.AppointmentTasks.Count == 0) return BadRequest("Enter at least one task."); // If no tasks return BadRequest

        try
        {
            var created = await _service.Create(appointmentDto, authUserId: authUserId, role: role); // Create with service
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created); // Return created appointment 
        }
        catch (UnauthorizedAccessException) // Handles Unauthorized access
        {
            return Forbid();
        }
        catch (ArgumentException e) // Handles bad input exceptions 
        {
            return BadRequest(e.Message);
        }
        catch (InvalidOperationException e)  // Handles operation exceptions 
        {
            _logger.LogWarning(e, "[AppointmentController] Create failed");
            return StatusCode(500, "Internal error creating the appointment"); // Returns 500 at operation exceptions
        }
        catch (Exception ex) // Handles general exceptions 
        {
            _logger.LogError(ex, "[AppointmentController] Unexpected error during Create");
            return StatusCode(500, "Unexpected error."); // Returns 500 at general exceptions
        }

    }

    [Authorize(Roles = "Client,Admin,HealthcareWorker")] 
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AppointmentDto appointmentDto)
    {        
        var (role, authUserId) = UserContext();
        if (id != appointmentDto.Id) return BadRequest("ID mismatch"); // Checks if id and appointment id the same

        try
        {
            var ok = await _service.Update(id, appointmentDto, role: role , authUserId: authUserId); // Calls service to update appointment 
            if (!ok) return NotFound("Appointment not found"); // If service return null Controller returns Not Found 
            return NoContent(); // Controller returns NoContent() if updated went fine
        }
        catch (UnauthorizedAccessException) // Handles unauthorized access
        {
            return Forbid();
        }
        catch (ArgumentException e) // Handles bad input exceptions
        {
            return BadRequest(e.Message);
        }
        catch (InvalidOperationException e) // Handles operation exceptions
        {
            _logger.LogWarning(e, "[AppointmentController] Update failed {Id:0000}", id);
            return StatusCode(500, "Internal error updating the appointment"); // Returns 500 at operation exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppointmentController] Unexpected error updating {Id:0000}", id);
            return StatusCode(500, "Unexpected error."); // Returns 500 at general exceptions
        }
    }


    [Authorize(Roles = "Client,Admin,HealthcareWorker")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) // Delete appointment by id
    {

        var (role, authUserId) = UserContext();
        try
        {
            var ok = await _service.Delete(id, role, authUserId); // Calls Service to delete appointment 
            if (!ok) return NotFound("Appointment not found"); // If service return null Controller returns Not Found 
            return NoContent(); // Controller returns NoContent() if deletion went fine 
        }
        catch (UnauthorizedAccessException) // Handles unauthorized access
        {
            return Forbid();
        }

        catch (InvalidOperationException e) // Handles operation exceptions
        {
            _logger.LogWarning(e, "[AppointmentController] Delete failed {Id:0000}", id);
            return StatusCode(500, "Appointment deletion failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppointmentController] Unexpected error deleting {Id:0000}", id);
            return StatusCode(500, "Unexpected error.");
        }

    }

    [Authorize(Roles = "Client,Admin,HealthcareWorker")] 
    [HttpGet("{id:int}/changelog")]
    public async Task<IActionResult> ChangeLog(int id) // Get changelog for appointment by id
    {

        var (role, authUserId) = UserContext();

        try
        {
            var logs = await _service.GetChangeLog(id, role: role, authUserId: authUserId); // Calls Service to get changelog
            return Ok(logs);
        }
        catch (UnauthorizedAccessException) // Handles unauthorized access
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppointmentController] ChangeLog failed {Id:0000}", id);
            return StatusCode(500, "Failed to fetch change log.");
        }
    }
}


