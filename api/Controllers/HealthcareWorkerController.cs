using HomecareAppointmentManagement.DAL;
using HomecareAppointmentManagement.Models;
using HomecareAppointmentManagement.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HomecareAppointmentManagement.Services;

namespace HomecareAppointmentManagement.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class HealthcareWorkerController : ControllerBase
{
    private readonly IHealthcareWorkerService _service;
    private readonly ILogger<HealthcareWorkerController> _logger;

    public HealthcareWorkerController(IHealthcareWorkerService service, ILogger<HealthcareWorkerController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var healthcareWorkers = await _service.GetAll();
        if (!healthcareWorkers.Any())
        {
            _logger.LogWarning("[HealthcareWorkerController] No healthcare workers found.");
            return NotFound("No healthcare workers found.");
        }
        return Ok(healthcareWorkers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var worker = await _service.GetById(id);
        if (worker == null)
        {
            _logger.LogError("[HealthcareWorkerController] Healthcare worker not found for HealthcareWorkerId {HealthcareWorkerId:0000}", id);
            return NotFound("Healthcare worker not found");
        }
        return Ok(worker);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] HealthcareWorkerDto workerDto)
    {
        try
        {
            var created = await _service.Create(workerDto);
            return CreatedAtAction(nameof(GetById), new { id = created.HealthcareWorkerId }, created);
        } catch (InvalidOperationException)
        {
            return StatusCode(500, "API had a problem while handling your request.");
        } catch (AggregateException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] HealthcareWorkerDto workerDto)
    {
        if (id != workerDto.HealthcareWorkerId)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            var existingWorker = await _service.Update(id, workerDto);
            if (!existingWorker)
            {
                return NotFound("Healthcare worker not found");
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[ClientController] Worker update failed for HealthcareWorkerId {id:0000}, {@worker}", id, workerDto);
            return StatusCode(500, "Failed to update worker.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ClientController] Worker update failed for HealthcareWorkerId {id:0000}, {@worker}", id, workerDto);
            return StatusCode(500, "A problem happened while updating the worker.");
        }        
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            bool deleted = await _service.Delete(id);
            if (!deleted)
            {
                return NotFound("Healthcare worker was not found or delete failed");
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[ClientController] Worker deletion failed for HealthcareWorkerId {id:0000}", id);
            return StatusCode(500, "Failed to delete worker.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ClientController] Worker deletion failed for HealthcareWorkerId {id:0000}", id);
            return StatusCode(500, "A problem happened while deleting the worker.");
        }
    }
}
