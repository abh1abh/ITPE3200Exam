using api.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Services;

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
            _logger.LogError("[HealthcareWorkerController] Healthcare worker not found for Id {Id:0000}", id);
            return NotFound("Healthcare worker not found");
        }
        return Ok(worker);
    }

    [HttpGet("workerauth/{authUserId}")]
    public async Task<IActionResult> GetByAuthUserId(string authUserId)
    {
        var worker = await _service.GetByAuthUserId(authUserId);
        if (worker == null)
        {
            _logger.LogError("[HealthcareWorkerController] Healthcare worker not found for AuthUserId {AuthUserId}", authUserId);
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
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        } catch (InvalidOperationException)
        {
            return StatusCode(500, "API had a problem while handling your request.");
        } catch (AggregateException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromBody] UpdateWorkerDto workerDto)
    {
        int id = workerDto.Id;
        HealthcareWorkerDto? worker = await _service.GetById(id);
        if (worker == null)
        {
            return NotFound("Healthcare worker not found");
        }
        if (id != workerDto.Id)
        {
            return BadRequest("ID mismatch");
        }
        try
        {
            var existingWorker = await _service.Update(id, workerDto);
            var authWorkerUpdate = await _authService.UpdateUserAsync(worker.AuthUserId, workerDto.Email!, workerDto.Password!);
            if (!existingWorker)
            {
                return NotFound("Healthcare worker not found");
            }
            if (!authWorkerUpdate)
            {
                _logger.LogError("[HealthcareWorkerController] Worker update failed for AuthUserId {userId}, {@worker}", worker.AuthUserId, workerDto);
                return StatusCode(500, "Failed to update associated user.");
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[HealthcareWorkerController] Worker update failed for Id {Id:0000}, {@worker}", id, workerDto);
            return StatusCode(500, "Failed to update worker.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HealthcareWorkerController] Worker update failed for Id {Id:0000}, {@worker}", id, workerDto);
            return StatusCode(500, "A problem happened while updating the worker.");
        }        
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {   
        string? userName = (await _service.GetById(id))?.Email;
        _logger.LogError("[HealthcareWorkerController] Deleting worker with Id {Id:0000} and AuthUserId {userId}", id, userName);
        try
        {
            bool deleted = await _service.Delete(id);
            bool deletedUser = await _authService.DeleteUserAdminAsync(userName!);
            if (!deleted)
            {
                return NotFound("Healthcare worker was not found or delete failed");
            }
            else if (!deletedUser)
            {
                _logger.LogError("[HealthcareWorkerController] Worker deletion failed for AuthUserId {userId}", userName);
                return StatusCode(500, "Failed to delete associated user.");
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[HealthcareWorkerController] Worker deletion failed for Id {Id:0000}", id);
            return StatusCode(500, "Failed to delete worker.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HealthcareWorkerController] Worker deletion failed for Id {Ids:0000}", id);
            return StatusCode(500, "A problem happened while deleting the worker.");
        }
    }
}
