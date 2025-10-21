using HomecareAppointmentManagement.DAL;
using HomecareAppointmentManagement.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomecareAppointmentManagement.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChangeLogController : ControllerBase
{
    private readonly IChangeLogRepository _repository;
    private readonly ILogger<ChangeLogController> _logger;

    public ChangeLogController(IChangeLogRepository repository, ILogger<ChangeLogController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var logs = await _repository.GetAll();
        if (logs == null)
        {
            _logger.LogWarning("[ChangeLogController] No change logs found.");
            return NotFound("No change logs found.");
        }

        var logDtos = logs.Select(log => new ChangeLogDto
        {
            Id = log.Id,
            AppointmentId = log.AppointmentId,
            ChangeDate = log.ChangeDate,
            ChangedByUserId = log.ChangedByUserId,
            ChangeDescription = log.ChangeDescription,
        });

        return Ok(logDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var log = await _repository.GetById(id);
        if (log == null)
        {
            _logger.LogError("[ChangeLogController] Change log not found for ChangeLogId {ChangeLogId:0000}", id);
            return NotFound("Change log not found");
        }

        var logDto = new ChangeLogDto
        {
            Id = log.Id,
            AppointmentId = log.AppointmentId,
            ChangeDate = log.ChangeDate,
            ChangedByUserId = log.ChangedByUserId,
            ChangeDescription = log.ChangeDescription,
        };

        return Ok(logDto);
    }
}
