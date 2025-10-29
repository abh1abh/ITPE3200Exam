
using api.DAL;
using api.DTO;
using api.Models;

namespace api.Services;
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


    // Private helper function to easily know if user is authorized or not. 
    // Service uses the appointment, role and AuthUserId to check if the user is authorized.
    // If the user is not admin, or the worker/client is not reference in the appointment the user will be unauthorized.
    // Only admins and relevant users can view, edit or delete an appointment 
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
        var appointments = await _appointmentRepository.GetAll(); // Calls AppointmentRepository to get all appointments 
        if (appointments is null || !appointments.Any()) return Enumerable.Empty<AppointmentDto>(); // If appointments are null Service returns a empty AppointmentDto List

        // Service convert the appointment list to AppointmentDto List
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
        var appointment = await _appointmentRepository.GetById(id); // Gets the appointment from DB
        if (appointment is null) return null; // If the appointment is null, Service returns null

        // Service uses our helper function to check if the user is authorized.
        if (!await IsAuthorized(appointment, authUserId, role)) throw new UnauthorizedAccessException();

        // We convert the appointment to dto before returning the dto
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

        // Here we use role to get the clientId either the AuthUserId or the incoming appointment Dto
        int clientId;
        if (role == "Client")
        {
            if (string.IsNullOrEmpty(authUserId)) throw new UnauthorizedAccessException(); // If the service to not receive a AuthUserId we throw an UnauthorizedAccessException 
            var client = await _clientRepository.GetByAuthUserId(authUserId) ?? throw new UnauthorizedAccessException(); // And if the AuthUserId does not belong to a Client we do the same
            clientId = client.ClientId;
        }
        else if (role == "Admin")
        {
            if (dto.ClientId == 0) throw new ArgumentException("ClientId is required for admin creation."); // If the incoming AppointmentDto does not have a clientId we throw an ArgumentException
            var client = await _clientRepository.GetClientById(dto.ClientId) ?? throw new ArgumentException($"Client {dto.ClientId} not found."); // As well as if the clientId is not from a client in the DB
            clientId = client.ClientId;
        }
        else
        {
            throw new UnauthorizedAccessException(); // We throw an UnauthorizedAccessException if the role is not Admin or Client
        }

        var slot = await _availableSlotRepository.GetById(dto.AvailableSlotId!.Value); // We find the Available Slot from the Id from the incoming Dto 
        if (slot is null || slot.IsBooked || slot.Start <= DateTime.UtcNow)
            throw new ArgumentException("That slot is no longer available."); // If the slot does not exist, is booked or the slot have expired we return an ArgumentException 

        // Service books the slots and updates the slot
        slot.IsBooked = true; 
        var booked = await _availableSlotRepository.Update(slot);
        if (!booked)
            throw new InvalidOperationException("Could not book the selected slot."); // If the slot is not booked it throws an InvalidOperationException

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
            var created = await _appointmentRepository.Create(appointment); // Call the AppointmentRepository to create the appointment 
            if (!created) throw new InvalidOperationException("Could not create appointment.");

            foreach (var t in dto.AppointmentTasks) // Service loops through the Appointment task and creates each one
            {
                var ok = await _appointmentTaskRepository.Create(new AppointmentTask
                {
                    AppointmentId = appointment.Id,
                    Description = t.Description!,
                    IsCompleted = t.IsCompleted
                });
                if (!ok) throw new InvalidOperationException("Could not create appointment task."); // If one of the task fail, it throws an InvalidOperationException
            }

            // To get the full appointment, we call the Db to get it. Then we convert it to a Dto and return it.
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
        catch // If the try fails, we unbook the slot, and propagate the exception to the controller.  
        {
            slot.IsBooked = false;
            await _availableSlotRepository.Update(slot);
            throw;
        }
    }
            
    
    public async Task<bool> Update(int id, AppointmentDto appointmentDto, string? role, string? authUserId)
    {
        var existing = await _appointmentRepository.GetById(id); // Checks if the appointment exists 
        if (existing is null) return false;

        // Check the authorization 
        if (!await IsAuthorized(existing, authUserId, role))
            throw new UnauthorizedAccessException();
        
        // Only notes + tasks can change
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
        if (!updatedOk) // If update fails, log warning and throw InvalidOperationException
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
        if (appointment == null) // If not found, log error and return false
        {
            // _logger.LogError("[AppointmentController] appointment not found when deleting for AppointmentId {AppointmentId:0000}", id);
            return false;
        }

        if (!await IsAuthorized(appointment, authUserId, role))
            throw new UnauthorizedAccessException();

        // Free slot first 
        if (appointment.AvailableSlotId.HasValue)
        {
            var slot = await _availableSlotRepository.GetById(appointment.AvailableSlotId.Value);
            if (slot is not null)
            {
                slot.IsBooked = false;
                var slotOk = await _availableSlotRepository.Update(slot); // Update slot to free it up
                if (!slotOk)
                {
                    // TODO: Maybe should we throw an exception? 
                    // Log warning.
                    _logger.LogWarning("[AppointmentService] failed to free up slot for AppointmentId {AppointmentId:0000}", id);
                }
            }
        }

        // Create changelog 
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
        if (!returnOk) // If deletion fails, log error and throw InvalidOperationException
        {
            _logger.LogError("[AppointmentService] appointment deletion failed for AppointmentId {AppointmentId:0000}", appointment.Id);
            throw new InvalidOperationException("Appointment deletion failed.");
        }

        return true;
    }
    
    public async Task<IEnumerable<ChangeLogDto>> GetChangeLog(int id, string? role, string? authUserId)
    {
        var appointment = await _appointmentRepository.GetById(id); // Get appointment by id
        if (appointment is null) return Enumerable.Empty<ChangeLogDto>(); // if appointment is null return empty list of ChangeLogDtos

        // Check authorization 
        if (!await IsAuthorized(appointment, authUserId, role))
            throw new UnauthorizedAccessException();

        // Get ChangeLogs and convert them to dtos
        var logs = await _changeLogRepository.GetByAppointmentId(id) ?? Enumerable.Empty<ChangeLog>(); // Fallback to empty list of ChangeLog

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