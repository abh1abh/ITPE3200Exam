using HomecareAppointmentManagement.DAL;
using HomecareAppointmentManagement.Models;
using HomecareAppointmentManagement.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomecareAppointmentManagement.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AppointmentTaskController : ControllerBase
{
    private readonly IAppointmentTaskRepository _repository;
    private readonly ILogger<AppointmentTaskController> _logger;

    public AppointmentTaskController(IAppointmentTaskRepository repository, ILogger<AppointmentTaskController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tasks = await _repository.GetAll();
        if (tasks == null)
        {
            _logger.LogWarning("[AppointmentTaskController] No appointment tasks found.");
            return NotFound("No appointment tasks found.");
        }

        var taskDtos = tasks.Select(t => new AppointmentTaskDto
        {
            Id = t.Id,
            AppointmentId = t.AppointmentId,
            Description = t.Description,
            IsCompleted = t.IsCompleted
        });

        return Ok(taskDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _repository.GetById(id);
        if (task == null)
        {
            _logger.LogError("[AppointmentTaskController] Task not found for TaskId {TaskId:0000}", id);
            return NotFound("Appointment task not found");
        }

        var taskDto = new AppointmentTaskDto
        {
            Id = task.Id,
            AppointmentId = task.AppointmentId,
            Description = task.Description,
            IsCompleted = task.IsCompleted
        };

        return Ok(taskDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AppointmentTaskDto taskDto)
    {
        var task = new AppointmentTask
        {
            AppointmentId = taskDto.AppointmentId,
            Description = taskDto.Description,
            IsCompleted = taskDto.IsCompleted
        };

        bool created = await _repository.Create(task);
        if (!created)
        {
            _logger.LogError("[AppointmentTaskController] Task creation failed {@task}", task);
            return StatusCode(500, "A problem happened while handling your request.");
        }

        var createdDto = new AppointmentTaskDto
        {
            Id = task.Id,
            AppointmentId = task.AppointmentId,
            Description = task.Description,
            IsCompleted = task.IsCompleted
        };

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, createdDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AppointmentTaskDto taskDto)
    {
        if (id != taskDto.Id)
        {
            return BadRequest("ID mismatch");
        }

        var existingTask = await _repository.GetById(id);
        if (existingTask == null)
        {
            return NotFound("Appointment task not found");
        }

        existingTask.Description = taskDto.Description;
        existingTask.IsCompleted = taskDto.IsCompleted;
        // Do not allow changing the appointment id
        // existingTask.AppointmentId = taskDto.AppointmentId;

        bool updated = await _repository.Update(existingTask);
        if (!updated)
        {
            _logger.LogError("[AppointmentTaskController] Task update failed for TaskId {TaskId:0000}, {@task}", id, existingTask);
            return StatusCode(500, "A problem happened while handling your request.");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _repository.GetById(id);
        if (task == null)
        {
            return NotFound("Appointment task not found");
        }

        bool deleted = await _repository.Delete(id);
        if (!deleted)
        {
            _logger.LogError("[AppointmentTaskController] Task deletion failed for TaskId {TaskId:0000}", id);
            return StatusCode(500, "A problem happened while handling your request.");
        }

        return NoContent();
    }
}
