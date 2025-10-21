using HomecareAppointmentManagement.DAL;
using HomecareAppointmentManagement.Models;
using HomecareAppointmentManagement.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomecareAppointmentManagement.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class HealthcareWorkerController : ControllerBase
{
    private readonly IHealthcareWorkerRepository _repository;
    private readonly ILogger<HealthcareWorkerController> _logger;

    public HealthcareWorkerController(IHealthcareWorkerRepository repository, ILogger<HealthcareWorkerController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var healthcareWorkers = await _repository.GetAll();
        if (healthcareWorkers == null || !healthcareWorkers.Any())
        {
            _logger.LogWarning("[HealthcareWorkerController] No healthcare workers found.");
            return NotFound("No healthcare workers found.");
        }

        var workerDtos = healthcareWorkers.Select(w => new HealthcareWorkerDto
        {
            HealthcareWorkerId = w.HealthcareWorkerId,
            Name = w.Name,
            Address = w.Address,
            Phone = w.Phone,
            Email = w.Email,
            AuthUserId = w.AuthUserId
        });

        return Ok(workerDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var worker = await _repository.GetById(id);
        if (worker == null)
        {
            _logger.LogError("[HealthcareWorkerController] Healthcare worker not found for HealthcareWorkerId {HealthcareWorkerId:0000}", id);
            return NotFound("Healthcare worker not found");
        }

        var workerDto = new HealthcareWorkerDto
        {
            HealthcareWorkerId = worker.HealthcareWorkerId,
            Name = worker.Name,
            Address = worker.Address,
            Phone = worker.Phone,
            Email = worker.Email,
            AuthUserId = worker.AuthUserId
        };

        return Ok(workerDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] HealthcareWorkerDto workerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var worker = new HealthcareWorker
        {
            Name = workerDto.Name,
            Address = workerDto.Address,
            Phone = workerDto.Phone,
            Email = workerDto.Email,
            AuthUserId = workerDto.AuthUserId
        };

        bool created = await _repository.Create(worker);
        if (!created)
        {
            _logger.LogError("[HealthcareWorkerController] Healthcare worker creation failed {@worker}", worker);
            return StatusCode(500, "A problem happened while handling your request.");
        }

        var createdDto = new HealthcareWorkerDto
        {
            HealthcareWorkerId = worker.HealthcareWorkerId,
            Name = worker.Name,
            Address = worker.Address,
            Phone = worker.Phone,
            Email = worker.Email,
            AuthUserId = worker.AuthUserId
        };

        return CreatedAtAction(nameof(GetById), new { id = worker.HealthcareWorkerId }, createdDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] HealthcareWorkerDto workerDto)
    {
        if (id != workerDto.HealthcareWorkerId)
        {
            return BadRequest("ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingWorker = await _repository.GetById(id);
        if (existingWorker == null)
        {
            return NotFound("Healthcare worker not found");
        }

        existingWorker.Name = workerDto.Name;
        existingWorker.Address = workerDto.Address;
        existingWorker.Phone = workerDto.Phone;
        existingWorker.Email = workerDto.Email;
        existingWorker.AuthUserId = workerDto.AuthUserId;

        bool updated = await _repository.Update(existingWorker);
        if (!updated)
        {
            _logger.LogError("[HealthcareWorkerController] Healthcare worker update failed for HealthcareWorkerId {HealthcareWorkerId:0000}, {@worker}", id, existingWorker);
            return StatusCode(500, "A problem happened while handling your request.");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var worker = await _repository.GetById(id);
        if (worker == null)
        {
            return NotFound("Healthcare worker not found");
        }

        bool deleted = await _repository.Delete(id);
        if (!deleted)
        {
            _logger.LogError("[HealthcareWorkerController] Healthcare worker deletion failed for HealthcareWorkerId {HealthcareWorkerId:0000}", id);
            return StatusCode(500, "A problem happened while handling your request.");
        }

        return NoContent();
    }
}
