
using HomecareAppointmentManagement.DAL;
using HomecareAppointmentManagement.DTO;
using HomecareAppointmentManagement.Models;

namespace HomecareAppointmentManagement.Services;
public class AppointmentService: IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAvailableSlotRepository _availableSlotRepository; // For slot management
    private readonly IClientRepository _clientRepository; // For client management
    private readonly IAppointmentTaskRepository _appointmentTaskRepository; // For appointment tasks
    private readonly IChangeLogRepository _changeLogRepository; // For change logs
    private readonly IHealthcareWorkerRepository _healthcareWorkerRepository;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService
    (
        IAppointmentRepository appointmentRepository,
        IAvailableSlotRepository availableSlotRepository,
        IClientRepository clientRepository,
        IAppointmentTaskRepository appointmentTaskRepository,
        IChangeLogRepository changeLogRepository,
        IHealthcareWorkerRepository healthcareWorkerRepository,
        ILogger<AppointmentService> logger
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


    private async Task<bool> IsAuthorized(Appointment appt, string? authUserId, string? role)
    {
        if (string.IsNullOrEmpty(authUserId)) return false;
        if (role == "Admin") return true;

        var ok = false;

        if (role == "Client" && appt.ClientId != 0)
        {
            var client = await _clientRepository.GetByAuthUserId(authUserId);
            ok = client is not null && client.ClientId == appt.ClientId;
        }

        if (role == "Worker" && appt.HealthcareWorkerId != 0)
        {
            var worker = await _healthcareWorkerRepository.GetByAuthUserId(authUserId);
            ok = worker is not null && worker.HealthcareWorkerId == appt.HealthcareWorkerId;
        }
        return ok;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAll()
    {
        var appointments = await _appointmentRepository.GetAll();
        if (appointments is null || !appointments.Any()) return Enumerable.Empty<AppointmentDto>(); 
        
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
        return appointmentDtos;
    }
    public async Task<AppointmentDto?> GetById(int id, string? role, string? authUserId)
    {
        var appointment = await _appointmentRepository.GetById(id);
        if (appointment is null) return null;

        if (!await IsAuthorized(appointment, authUserId, role)) throw new UnauthorizedAccessException();

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
        return appointmentDto;
    }
    public async Task<AppointmentDto> Create(AppointmentDto dto, string? role, string? authUserId)
    {

        dto.AppointmentTasks = (dto.AppointmentTasks ?? new())
            .Where(t => !string.IsNullOrWhiteSpace(t.Description))
            .ToList();

        if (dto.AvailableSlotId is null) throw new ArgumentException("AvailableSlotId is required.");

        int clientId;
        if (role == "Client")
        {
            if (string.IsNullOrEmpty(authUserId)) throw new UnauthorizedAccessException();
            var client = await _clientRepository.GetByAuthUserId(authUserId) ?? throw new UnauthorizedAccessException();
            clientId = client.ClientId;
        }
        else if (role == "Admin")
        {
            if (dto.ClientId == 0) throw new ArgumentException("ClientId is required for admin creation.");
            var client = await _clientRepository.GetClientById(dto.ClientId) ?? throw new ArgumentException($"Client {dto.ClientId} not found.");
            clientId = client.ClientId;
        }
        else
        {
            throw new UnauthorizedAccessException();
        }

        var slot = await _availableSlotRepository.GetById(dto.AvailableSlotId.Value);
        if (slot is null || slot.IsBooked || slot.Start <= DateTime.UtcNow)
            throw new ArgumentException("That slot is no longer available.");

        slot.IsBooked = true;
        var booked = await _availableSlotRepository.Update(slot);
        if (!booked)
            throw new InvalidOperationException("Could not book the selected slot.");

        var appointment = new Appointment
        {
            ClientId = clientId,
            HealthcareWorkerId = slot.HealthcareWorkerId,
            Start = slot.Start,
            End = slot.End,
            Notes = dto.Notes ?? string.Empty,
            AvailableSlotId = slot.Id
        };

        try
        {
            var created = await _appointmentRepository.Create(appointment);
            if (!created) throw new InvalidOperationException("Could not create appointment.");

            foreach (var t in dto.AppointmentTasks)
            {
                var ok = await _appointmentTaskRepository.Create(new AppointmentTask
                {
                    AppointmentId = appointment.Id,
                    Description = t.Description!,
                    IsCompleted = t.IsCompleted
                });
                if (!ok) throw new InvalidOperationException("Could not create appointment task.");
            }

            var freshAppointment = await _appointmentRepository.GetById(appointment.Id) ?? appointment; // Fallback to appointment without task if not found
            var appointmentDto = new AppointmentDto
            {
                Id = freshAppointment.Id,
                ClientId = freshAppointment.ClientId,
                HealthcareWorkerId = freshAppointment.HealthcareWorkerId,
                Start = freshAppointment.Start,
                End = freshAppointment.End,
                Notes = freshAppointment.Notes,
                AvailableSlotId = freshAppointment.AvailableSlotId,
                AppointmentTasks = freshAppointment.AppointmentTasks?.Select(t => new AppointmentTaskDto
                {
                    Id = t.Id,
                    AppointmentId = t.AppointmentId,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted
                }).ToList(),
                ChangeLogs = freshAppointment.ChangeLogs?.Select(cl => new ChangeLogDto
                {
                    Id = cl.Id,
                    AppointmentId = cl.AppointmentId,
                    ChangeDate = cl.ChangeDate,
                    ChangedByUserId = cl.ChangedByUserId,
                    ChangeDescription = cl.ChangeDescription
                }).ToList()
            };
            return appointmentDto;
        } 
        catch 
        {
            slot.IsBooked = false;
            await _availableSlotRepository.Update(slot);
            throw;
        }
    }
            
    
    public async Task<bool> Update(int id, AppointmentDto appointmentDto, string? role, string? authUserId)
    {
        var existing = await _appointmentRepository.GetById(id);
        if (existing is null) return false;

        if (!await IsAuthorized(existing, authUserId, role))
            throw new UnauthorizedAccessException();
        
        // Only notes + tasks in this flow (no slot changes here)
        var changes = new List<string>(); // To track changes for logging
        if (!string.Equals(existing.Notes ?? "", appointmentDto.Notes ?? "", StringComparison.Ordinal))
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
        if (changes.Count == 0) return true;

        var updatedOk = await _appointmentRepository.Update(existing); // Update appointment
        if (!updatedOk) // If update fails, log warning and return view with model
        {
            _logger.LogWarning("[AppointmentWarning] appointment update failed for AppointmentId {AppointmentId:0000}", appointmentDto.Id);
            throw new InvalidOperationException("Internal error updating the appointment.");
        }

        // var userId = User.TryGetUserId() ?? string.Empty; // Get user ID for logging, fallback to empty string
        var description = string.Join("; ", changes); // Combine changes into single description
        if (description.Length > 500) description = description[..500];

        var log = new ChangeLog
        {
            AppointmentId = existing.Id,
            AppointmentIdSnapshot = existing.Id,
            ChangeDate = DateTime.UtcNow,
            ChangedByUserId = authUserId!,
            ChangeDescription = description
        };
        
        var logged = await _changeLogRepository.Create(log); // Log the change
        if (!logged)
        {
            // If logging fails, log warning
            _logger.LogWarning("[AppointmentService] failed to create change log for AppointmentId {AppointmentId:0000}. Description: {Description}", appointmentDto.Id, description);
        }
        return true;
        
    }
    public async Task<bool> Delete(int id, string? role, string? authUserId)
    {
        var appointment = await _appointmentRepository.GetById(id); // Get appointment by ID
        if (appointment == null) // If not found, log error and return NotFound
        {
            // _logger.LogError("[AppointmentController] appointment not found when deleting for AppointmentId {AppointmentId:0000}", id);
            return false;
        }

        if (!await IsAuthorized(appointment, authUserId, role))
            throw new UnauthorizedAccessException();

        // Free slot first (best effort)
        if (appointment.AvailableSlotId.HasValue)
        {
            var slot = await _availableSlotRepository.GetById(appointment.AvailableSlotId.Value);
            if (slot is not null)
            {
                slot.IsBooked = false;
                var slotOk = await _availableSlotRepository.Update(slot); // Update slot to free it up
                if (!slotOk)
                {
                    _logger.LogWarning("[AppointmentService] failed to free up slot for AppointmentId {AppointmentId:0000}", id);
                }
            }
        }

        bool logged = await _changeLogRepository.Create(new ChangeLog
        {
            AppointmentId = appointment.Id,
            AppointmentIdSnapshot = appointment.Id,
            ChangeDate = DateTime.UtcNow,
            ChangedByUserId = authUserId!, // Cannot be null since we check for null in AuthorizeAppointmentAsync
            ChangeDescription = "Appointment deleted."
        });

        if (!logged) // If logging fails, log warning
        {
            _logger.LogWarning("[AppointmentService] failed to create change log for deleted AppointmentId {AppointmentId:0000}", appointment.Id);
        }

        bool returnOk = await _appointmentRepository.Delete(appointment.Id); // Delete appointment
        if (!returnOk) // If deletion fails, log error and return BadRequest
        {
            _logger.LogError("[AppointmentService] appointment deletion failed for AppointmentId {AppointmentId:0000}", appointment.Id);
            throw new InvalidOperationException("Appointment deletion failed.");
        }

        return true;
    }
    
    public async Task<IEnumerable<ChangeLogDto>> GetChangeLogAsync(int id, string? role, string? authUserId)
    {
        var appointment = await _appointmentRepository.GetById(id);
        if (appointment is null) return Enumerable.Empty<ChangeLogDto>();

        if (!await IsAuthorized(appointment, authUserId, role))
            throw new UnauthorizedAccessException();

        var logs = await _changeLogRepository.GetByAppointmentId(id) ?? Enumerable.Empty<ChangeLog>();

        var logDtos = logs.Select(l => new ChangeLogDto
        {
            Id = l.Id,
            AppointmentId = l.AppointmentId,
            ChangeDate = l.ChangeDate,
            ChangedByUserId = l.ChangedByUserId,
            ChangeDescription = l.ChangeDescription
        });

        return logDtos;

    }


}