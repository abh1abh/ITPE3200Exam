using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using HomecareAppointmentManagement.DAL;
using HomecareAppointmentManagement.DTO;
using HomecareAppointmentManagement.Models;
using HomecareAppointmentManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace HomecareAppointmentManagement.Controllers;

[Authorize(Roles = "Client,Admin,HealthcareWorker")] // Authorize all relevant roles
[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly ILogger<AppointmentController> _logger;
    private readonly IAppointmentService _service;

    public AppointmentController (IAppointmentService service, ILogger<AppointmentController> logger)
    {
        _service = service;
        _logger = logger;
    }

    private (string? role, string? authUserId) UserContext()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);
        return (role, userId);
    }

    [HttpGet]
    public async Task<IActionResult> AppointmentList()
    {
        var appointments = await _service.GetAll();
        if (!appointments.Any()) return NotFound("Appointments not found");
        return Ok(appointments);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {

        var (role, authUserId) = UserContext();

        try
        {
            var appointmentDto = await _service.GetById(id, role: role, authUserId: authUserId);
            if (appointmentDto is null)
            {
                _logger.LogError("[AppointmentController] appointment not found while executing _appointmentRepository.GetById() for AppointmentId {AppointmentId:0000}", id);
                return NotFound("Appointment not found");
            }
            return Ok(appointmentDto);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppointmentController] Error GetById {Id:0000}", id);
            return StatusCode(500, "A problem occurred while fetching the appointment.");
        }
    }

    [Authorize(Roles = "Admin,Client")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AppointmentDto appointmentDto)
    {
        var (role, authUserId) = UserContext();

        appointmentDto.AppointmentTasks = (appointmentDto.AppointmentTasks ?? new()).Where(t => !string.IsNullOrWhiteSpace(t.Description)).ToList();
        if (appointmentDto.AppointmentTasks.Count == 0) return BadRequest("Enter at least one task.");
        if (appointmentDto.AvailableSlotId is null) return BadRequest("Please choose an available slot.");

        try
        {
            var created = await _service.Create(appointmentDto, authUserId: authUserId, role: role);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }

        catch (InvalidOperationException e)
        {
            _logger.LogWarning(e, "[AppointmentController] Create failed");
            return StatusCode(500, "Could not create appointment. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppointmentController] Unexpected error during Create");
            return StatusCode(500, "Unexpected error.");
        }

    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AppointmentDto appointmentDto)
    {        
        var (role, authUserId) = UserContext();
        if (id != appointmentDto.Id) return BadRequest("ID mismatch");

        try
        {
            var ok = await _service.Update(id, appointmentDto, role: role , authUserId: authUserId);
            if (!ok) return NotFound("Appointment not found");
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogWarning(e, "[AppointmentController] Update failed {Id:0000}", id);
            return StatusCode(500, "Internal error updating the appointment");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppointmentController] Unexpected error updating {Id:0000}", id);
            return StatusCode(500, "Unexpected error.");
        }
    }



    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {

        var (role, authUserId) = UserContext();
        try
        {
            var ok = await _service.Delete(id, role, authUserId);
            if (!ok) return NotFound("Appointment not found");
            return NoContent();
        }
        catch (UnauthorizedAccessException) 
        {
            return Forbid(); 
        }
        
        catch (InvalidOperationException e)
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

    [HttpGet("{id:int}/changelog")]
    public async Task<IActionResult> ChangeLog(int id)
    {

        var (role, authUserId) = UserContext();

        try
        {
            var logs = await _service.GetChangeLogAsync(id, role: role, authUserId: authUserId);
            if (!logs.Any()) return NotFound("Change log not found");
            return Ok(logs);
        }
        catch (UnauthorizedAccessException)
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


