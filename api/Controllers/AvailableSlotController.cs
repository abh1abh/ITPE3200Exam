using HomecareAppointmentManagement.DAL;
using HomecareAppointmentManagement.Models;
using HomecareAppointmentManagement.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomecareAppointmentManagement.Controllers;

[Authorize(Roles = "HealthcareWorker,Admin")]
[ApiController]
[Route("api/[controller]")]
public class AvailableSlotController : ControllerBase
{
    private readonly IAvailableSlotRepository _availableSlotRepository;
    private readonly IHealthcareWorkerRepository _healthcareWorkerRepository;
    private readonly ILogger<AvailableSlotController> _logger;

    public AvailableSlotController(IAvailableSlotRepository availableSlotRepository, ILogger<AvailableSlotController> logger, IHealthcareWorkerRepository healthcareWorkerRepository)
    {
        _availableSlotRepository = availableSlotRepository;
        _healthcareWorkerRepository = healthcareWorkerRepository;
        _logger = logger;

    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<AvailableSlot>? slots;
        if (User.IsInRole("Admin"))
        {
            slots = await _availableSlotRepository.GetAll();
        }
        else
        {
            var authUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (authUserId is null) return Forbid();
            var worker = await _healthcareWorkerRepository.GetByAuthUserId(authUserId);
            if (worker is null) return Forbid();
            slots = await _availableSlotRepository.GetByWorkerId(worker.HealthcareWorkerId);
        }

        if (slots == null)
        {
            return NotFound("No available slots found.");
        }

        var slotDtos = slots.Select(s => new AvailableSlotDto
        {
            Id = s.Id,
            HealthcareWorkerId = s.HealthcareWorkerId,
            Start = s.Start,
            End = s.End,
            IsBooked = s.IsBooked
        });

        return Ok(slotDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var existing = await _availableSlotRepository.GetById(id);
        if (existing == null)
            return NotFound();

        if (! await IsAuthorizedForSlot(existing))
            return Forbid();;

        return Ok(new AvailableSlotDto
        {
            Id = existing.Id,
            HealthcareWorkerId = existing.HealthcareWorkerId,
            Start = existing.Start,
            End = existing.End,
            IsBooked = existing.IsBooked
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AvailableSlotDto slotDto)
    {
        int workerId;
        if (!User.IsInRole("Admin"))
        {
            var authUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(authUserId))
                return Forbid();

            var worker = await _healthcareWorkerRepository.GetByAuthUserId(authUserId);
            if (worker == null)
                return Forbid();
            workerId = worker.HealthcareWorkerId;
        }
        else
        {
            workerId = slotDto.HealthcareWorkerId;
        }

        var slot = new AvailableSlot
        {
            Start = slotDto.Start,
            End = slotDto.End,
            IsBooked = false, // New slots are not booked
            HealthcareWorkerId = workerId
        };

        var ok = await _availableSlotRepository.Create(slot);
        if (!ok)
        {
            _logger.LogError("[AvailableSlotController] available slot creation failed {@slot}", slot);
            return StatusCode(500, "Failed to create slot.");
        }

        slotDto.Id = slot.Id;
        slotDto.IsBooked = slot.IsBooked;
        
        return CreatedAtAction(nameof(GetById), new { id = slot.Id }, slotDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AvailableSlotDto slotDto)
    {
        if (id != slotDto.Id) return BadRequest("ID mismatch");

        var existing = await _availableSlotRepository.GetById(id);
        if (existing == null) return NotFound();

        if (! await IsAuthorizedForSlot(existing))
            return Forbid();

        if (User.IsInRole("Admin"))
        {
            existing.HealthcareWorkerId = slotDto.HealthcareWorkerId;
            existing.Start = slotDto.Start;
            existing.End = slotDto.End;
            existing.IsBooked = slotDto.IsBooked;
        }
        else // Healthcare Worker rules
        {
            if (!existing.IsBooked)
            {
                existing.Start = slotDto.Start;
                existing.End = slotDto.End;
                existing.IsBooked = false; // Cannot book a slot via this endpoint
            }
            else
            {
                // If slot is booked, worker cannot change it.
                return BadRequest("Cannot edit a booked slot.");
            }
        }

        var ok = await _availableSlotRepository.Update(existing);
        if (!ok)
        {
            _logger.LogError("[AvailableSlotController] update failed {@slot}", existing);
            return StatusCode(500, "Failed to update slot.");
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _availableSlotRepository.GetById(id);
        if (existing == null) return NotFound();

        if (! await IsAuthorizedForSlot(existing))
            return Forbid(); 

        if (existing.IsBooked)
        {
            return BadRequest("Cannot delete a booked slot. Please cancel the appointment first.");
        }

        var ok = await _availableSlotRepository.Delete(id);
        if (!ok)
        {
            _logger.LogError("[AvailableSlotController] available slot deletion failed for {id}", id);
            return StatusCode(500, "Failed to delete slot.");
        }

        return NoContent();
    }
    
    private async Task<bool> IsAuthorizedForSlot(AvailableSlot slot)
    {
        var authUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (authUserId is null) return false;

        var worker = await _healthcareWorkerRepository.GetByAuthUserId(authUserId);
        if (worker is null) return false;

        return User.IsInRole("Admin") || slot.HealthcareWorkerId == worker.HealthcareWorkerId;
    }

}
