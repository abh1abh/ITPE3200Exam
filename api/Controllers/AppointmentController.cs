using System.Security.Claims;
using HomecareAppointmentManagement.DAL;
using HomecareAppointmentManagement.DTO;
using HomecareAppointmentManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace HomecareAppointmentManagement.Controllers;

[Authorize(Roles = "Client,Admin,HealthcareWorker")] // Authorize all relevant roles
[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAvailableSlotRepository _availableSlotRepository; // For slot management
    private readonly IClientRepository _clientRepository; // For client management
    private readonly IAppointmentTaskRepository _appointmentTaskRepository; // For appointment tasks
    private readonly IChangeLogRepository _changeLogRepository; // For change logs
    private readonly IHealthcareWorkerRepository _healthcareWorkerRepository;
    private readonly ILogger<AppointmentController> _logger;

    public AppointmentController
    (
        IAppointmentRepository appointmentRepository,
        IAvailableSlotRepository availableSlotRepository,
        IClientRepository clientRepository,
        IAppointmentTaskRepository appointmentTaskRepository,
        IChangeLogRepository changeLogRepository,
        IHealthcareWorkerRepository healthcareWorkerRepository,
        ILogger<AppointmentController> logger
    )
    {
        _appointmentRepository = appointmentRepository;
        _availableSlotRepository = availableSlotRepository;
        _clientRepository = clientRepository;
        _appointmentTaskRepository = appointmentTaskRepository;
        _changeLogRepository = changeLogRepository;
        _healthcareWorkerRepository = healthcareWorkerRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> AppointmentList()
    {
        var appointments = await _appointmentRepository.GetAll();
        if (appointments == null)
        {
            _logger.LogError("[AppointmentController] appointment list not found while executing _appointmentRepository.GetAll()");
            return NotFound("Appointment list not found");
        }
        var appointmentDtos = appointments.Select(appointment => new AppointmentDto
        {
            Id = appointment.Id,
            ClientId = appointment.ClientId,
            HealthcareWorkerId = appointment.HealthcareWorkerId,
            Start = appointment.Start,
            End = appointment.End,
            Notes = appointment.Notes,
            AvailableSlotId = appointment.AvailableSlotId,
            AppointmentTasks = appointment.AppointmentTasks?.Select(t => new AppointmentTaskDto
            {
                Id = t.Id,
                AppointmentId = t.AppointmentId,
                Description = t.Description,
                IsCompleted = t.IsCompleted
            }).ToList(),
            ChangeLogs = appointment.ChangeLogs?.Select(cl => new ChangeLogDto
            {
                Id = cl.Id,
                AppointmentId = cl.AppointmentId,
                ChangeDate = cl.ChangeDate,
                ChangedByUserId = cl.ChangedByUserId,
                ChangeDescription = cl.ChangeDescription
            }).ToList() 

        });

        return Ok(appointmentDtos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _appointmentRepository.GetById(id);
        if (appointment == null)
        {
            _logger.LogError("[AppointmentController] appointment not found while executing _appointmentRepository.GetById() for AppointmentId {AppointmentId:0000}", id);
            return NotFound("Appointment not found");
        }
        
        var authUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var (ok, error) = await AuthorizeAppointmentAsync(appointment, authUserId);
        if (!ok) return error!;

        var appointmentDto = new AppointmentDto
        {
            Id = appointment.Id,
            ClientId = appointment.ClientId,
            HealthcareWorkerId = appointment.HealthcareWorkerId,
            Start = appointment.Start,
            End = appointment.End,
            Notes = appointment.Notes,
            AvailableSlotId = appointment.AvailableSlotId,
            AppointmentTasks = appointment.AppointmentTasks?.Select(t => new AppointmentTaskDto
            {
                Id = t.Id,
                AppointmentId = t.AppointmentId,
                Description = t.Description,
                IsCompleted = t.IsCompleted
            }).ToList(),
            ChangeLogs = appointment.ChangeLogs?.Select(cl => new ChangeLogDto
            {
                Id = cl.Id,
                AppointmentId = cl.AppointmentId,
                ChangeDate = cl.ChangeDate,
                ChangedByUserId = cl.ChangedByUserId,
                ChangeDescription = cl.ChangeDescription
            }).ToList()
        };
        return Ok(appointmentDto);
    }

    [Authorize(Roles ="Admin,Client")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AppointmentDto appointmentDto)
    {

        // 1) Normalize & validate basic input
        appointmentDto.AppointmentTasks = (appointmentDto.AppointmentTasks ?? new())
            .Where(t => !string.IsNullOrWhiteSpace(t.Description))
            .ToList();
        if (appointmentDto.AppointmentTasks.Count == 0)
            return BadRequest("Enter at least one task.");

        if (appointmentDto.AvailableSlotId is null)
            return BadRequest("Please choose an available slot.");
            

        // Validate selected slot
        var slot = await _availableSlotRepository.GetById(appointmentDto.AvailableSlotId.Value); // Ignore, null checked above

        if (slot is null || slot.IsBooked || slot.Start <= DateTime.UtcNow) // Slot must exist, be unbooked, and in the future
        {

            _logger.LogError("[AppointmentController] appointment needs to have slot to mark slot");
            return BadRequest("That slot is no longer available."); // Add model error
        }

        // Determine client ID 
        int clientId;
        if (User.IsInRole("Client")) // If Client, use their own client ID
        {
            var authUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(authUserId)) return Forbid();

            var client = await _clientRepository.GetByAuthUserId(authUserId);
            if (client is null) return Forbid();

            clientId = client.ClientId; // If we can't get client ID, forbid
        }
        else // If Admin, use selected client ID
        {
            // This should be non-null due to the earlier validation
            if (appointmentDto.ClientId == 0) return BadRequest("ClientId is required when creating as Admin.");
            var targetClient = await _clientRepository.GetClientById(appointmentDto.ClientId);
            if (targetClient is null) return BadRequest($"Client {appointmentDto.ClientId} not found.");
            clientId = targetClient.ClientId;
        }

        // Create appointment object
        var appointment = new Appointment
        {
            ClientId = clientId,
            HealthcareWorkerId = slot.HealthcareWorkerId,
            Start = slot.Start,
            End = slot.End,
            Notes = appointmentDto.Notes ?? string.Empty
            // Set AvailableSlotId later, after booking the slot
        };


        // Book slot
        slot.IsBooked = true;
        var slotUpdated = await _availableSlotRepository.Update(slot); // DB update
        if (!slotUpdated) // If slot booking fails, log error, rehydrate, and return
        {
            _logger.LogError("[AppointmentController] failed to mark slot {SlotId} booked", slot.Id);
            return StatusCode(500, "Could not book the selected slot. Please try another slot.");
            
        }

        // Link appointment to slot
        appointment.AvailableSlotId = slot.Id;

        // Create appointment
        var created = await _appointmentRepository.Create(appointment);
        if (!created) // If appointment creation fails, free up slot, log error, rehydrate, and return
        {
            slot.IsBooked = false;
            await _availableSlotRepository.Update(slot);

            _logger.LogWarning("[AppointmentController] appointment creation failed {@appointment}", appointment);
            return StatusCode(500, "Could not create appointment. Please try again.");
            
        }

        // Create tasks. Now only 1 task per appointment, but designed for multiple in future
        foreach (var t in appointmentDto.AppointmentTasks) // Loop through tasks
        {
            // Create task linked to appointment
            var ok = await _appointmentTaskRepository.Create(new AppointmentTask
            {
                AppointmentId = appointment.Id,
                Description = t.Description!,
                IsCompleted = false
            });

            if (!ok) // If task creation fails, delete appointment, free slot, log error, rehydrate, and return
            {
                await _appointmentRepository.Delete(appointment.Id);
                slot.IsBooked = false;
                await _availableSlotRepository.Update(slot);

                return StatusCode(500, "Could not create tasks. Please try again.");

            }
        }


        var result = new AppointmentDto
        {
            Id = appointment.Id,
            ClientId = appointment.ClientId,
            HealthcareWorkerId = appointment.HealthcareWorkerId,
            Start = appointment.Start,
            End = appointment.End,
            Notes = appointment.Notes,
            AvailableSlotId = appointment.AvailableSlotId,
            AppointmentTasks = (await _appointmentTaskRepository.GetByAppointmentId(appointment.Id))?
                .Select(t => new AppointmentTaskDto
                {
                    Id = t.Id,
                    AppointmentId = t.AppointmentId,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted
                }).ToList(),
            ChangeLogs = [] // do not accept from client
        };
        // Redirect to index on success
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, result);

    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AppointmentDto appointmentDto)
    {        
        var existing = await _appointmentRepository.GetById(id);
        if (existing == null) // If appointment not found, return NotFound
        {
            _logger.LogError("[AppointmentController] appointment not found during edit for AppointmentId {AppointmentId:0000}", appointmentDto.Id);
            return NotFound("Appointment not found");
        }
        
        var authUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var (ok, error) = await AuthorizeAppointmentAsync(existing, authUserId);
        if (!ok) return error!;
               

        var changes = new List<string>(); // To track changes for logging

        if (!string.Equals(existing.Notes ?? string.Empty, appointmentDto.Notes ?? string.Empty, StringComparison.Ordinal))
            changes.Add($"Notes: \"{existing.Notes}\" → \"{appointmentDto.Notes}\""); // If notes changed, log change by adding to changes list

        existing.Notes = appointmentDto.Notes ?? string.Empty; // Update notes

        // AppointmentTask dictionary for easy lookup
        var existingById = existing.AppointmentTasks?.ToDictionary(t => t.Id, t => t);

        foreach (var appointmentTask in appointmentDto.AppointmentTasks ?? []) // Loop through provided tasks
        {
            if (existingById != null && existingById.TryGetValue(appointmentTask.Id, out var t)) // If task exists in DB
            {
                if (!string.Equals(t.Description, appointmentTask.Description, StringComparison.Ordinal) || // Check for description change
                t.IsCompleted != appointmentTask.IsCompleted) // Or completion status change
                {
                    // Add change description
                    changes.Add($"Task #{t.Id}: \"{t.Description}\"/{t.IsCompleted} → \"{appointmentTask.Description}\"/{appointmentTask.IsCompleted}");
                }
                // Update task fields
                t.Description = appointmentTask.Description;
                t.IsCompleted = appointmentTask.IsCompleted;
                await _appointmentTaskRepository.Update(t);
            }
            else
            {
                // New task added
                var newTask = new AppointmentTask
                {
                    AppointmentId = existing.Id,
                    Description = appointmentTask.Description,
                    IsCompleted = appointmentTask.IsCompleted
                };
                await _appointmentTaskRepository.Create(newTask);
                changes.Add($"Task + \"{appointmentTask.Description}\" (new)");
            }
        }

        // If nothing actually changed
        if (changes.Count == 0)
        {
            // Nothing to update/log, just go back
            return NoContent();

        }

        var updatedOk = await _appointmentRepository.Update(existing); // Update appointment
        if (!updatedOk) // If update fails, log warning and return view with model
        {
            _logger.LogWarning("[AppointmentController] appointment update failed for AppointmentId {AppointmentId:0000}", appointmentDto.Id);
            return StatusCode(500, "Internal error updating the appointment");
        }

        // var userId = User.TryGetUserId() ?? string.Empty; // Get user ID for logging, fallback to empty string
        var description = string.Join("; ", changes); // Combine changes into single description
        if (description.Length > 500) description = description[..500];

        // Create change log entry
        var log = new ChangeLog
        {
            AppointmentId = existing.Id,
            AppointmentIdSnapshot = existing.Id,
            ChangeDate = DateTime.UtcNow,
            ChangedByUserId = authUserId!, // Cannot be null since we check for null in AuthorizeAppointmentAsync
            ChangeDescription = description
        };

        var logged = await _changeLogRepository.Create(log); // Log the change
        if (!logged)
        {
            // If logging fails, log warning
            _logger.LogWarning("[AppointmentController] failed to create change log for AppointmentId {AppointmentId:0000}. Description: {Description}", appointmentDto.Id, description);
        }

        return NoContent();

    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        Appointment? appointment = await _appointmentRepository.GetById(id); // Get appointment by ID
        if (appointment == null) // If not found, log error and return NotFound
        {
            _logger.LogError("[AppointmentController] appointment not found when deleting for AppointmentId {AppointmentId:0000}", id);
            return NotFound("Appointment not found");
        }

        var authUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var (ok, error) = await AuthorizeAppointmentAsync(appointment, authUserId);
        if (!ok) return error!;

        // Free up slot
        var slot = appointment.AvailableSlotId.HasValue
            ? await _availableSlotRepository.GetById(appointment.AvailableSlotId.Value)
            : null; // Get slot if linked, fallback to null

        
        if (slot != null)
        {
            slot.IsBooked = false;
            var slotOk = await _availableSlotRepository.Update(slot); // Update slot to free it up
            if (!slotOk)
            {
                _logger.LogWarning("[AppointmentController] failed to free up slot for AppointmentId {AppointmentId:0000}", id);
            }
        }        

        // Create change log entry for deletion
        bool logged = await _changeLogRepository.Create(new ChangeLog
        {
            AppointmentId = appointment.Id,
            AppointmentIdSnapshot =appointment.Id,
            ChangeDate = DateTime.UtcNow,
            ChangedByUserId = authUserId!, // Cannot be null since we check for null in AuthorizeAppointmentAsync
            ChangeDescription = "Appointment deleted."
        });

        if (!logged) // If logging fails, log warning
        {
            _logger.LogWarning("[AppointmentController] failed to create change log for deleted AppointmentId {AppointmentId:0000}", appointment.Id);
        }

        bool returnOk = await _appointmentRepository.Delete(appointment.Id); // Delete appointment
        if (!returnOk) // If deletion fails, log error and return BadRequest
        {
            _logger.LogError("[AppointmentController] appointment deletion failed for AppointmentId {AppointmentId:0000}", appointment.Id);
            return StatusCode(500, "Appointment deletion failed");
        }

        return NoContent();
    }

    [HttpGet("{id:int}/changelog")]
    public async Task<IActionResult> ChangeLog(int id)
    {
        // Get appointment to verify existence and for authorization
        var appointment = await _appointmentRepository.GetById(id);
        if (appointment == null) return NotFound("Appointment not found");

        var authUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var (ok, error) = await AuthorizeAppointmentAsync(appointment, authUserId);
        if (!ok) return error!;

        var logs = await _changeLogRepository.GetByAppointmentId(id);

        if (logs == null) return NotFound("Change log to found");

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
    private async Task<(bool allowed, IActionResult? error)> AuthorizeAppointmentAsync(Appointment appt, string? authUserId, bool allowAdmin = true)
    {
        if (string.IsNullOrEmpty(authUserId))
        {
            _logger.LogWarning("Not Autherized");
            return (false, Unauthorized());
            
        }

        if (allowAdmin && User.IsInRole("Admin"))
            return (true, null);

         bool isClient = false;
        bool isWorker = false;

        // Only do lookups you need (avoid forcing both).
        // If you have roles, use them to short-circuit which lookup to run.
        if (appt.ClientId != 0 && User.IsInRole("Client"))
        {
            // Prefer a lightweight lookup that returns just the ID
            var client = await _clientRepository.GetByAuthUserId(authUserId); 
            if (client is null) return (false, Forbid());
            isClient = appt.ClientId == client.ClientId;
        }

        if (appt.HealthcareWorkerId != 0 && User.IsInRole("HealthcareWorker"))
        {
            var worker = await _healthcareWorkerRepository.GetByAuthUserId(authUserId); 
            if (worker is null) return (false, Forbid());
            isWorker = appt.HealthcareWorkerId == worker.HealthcareWorkerId;
        }

        return (isClient || isWorker) ? (true, null) : (false, Forbid());
    }

}


