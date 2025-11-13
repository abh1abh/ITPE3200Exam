
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
            ok = client is not null && client.Id == appt.ClientId;
        }

        if (role == "HealthcareWorker" && appt.HealthcareWorkerId != 0)
        {
            var worker = await _healthcareWorkerRepository.GetByAuthUserId(authUserId);
            ok = worker is not null && worker.Id == appt.HealthcareWorkerId;
        }
        return ok;
    }

    public async Task<IEnumerable<AppointmentViewDto>> GetAll()
    {
        var appointments = await _appointmentRepository.GetAll(); // Calls AppointmentRepository to get all appointments 
        if (appointments is null || !appointments.Any()) return Enumerable.Empty<AppointmentViewDto>(); // If appointments are null Service returns a empty AppointmentDto List

        // Service convert the appointment list to AppointmentDto List
        var appointmentDtos = appointments.Select(appointment => new AppointmentViewDto
        {
            Id = appointment.Id,
            ClientId = appointment.ClientId,
            ClientName = appointment.Client.Name ?? $"Client Id: {appointment.ClientId}",
            HealthcareWorkerId = appointment.HealthcareWorkerId,
            HealthcareWorkerName = appointment.HealthcareWorker.Name ?? $"HealthcareWorker{appointment.HealthcareWorkerId}",
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

    public async Task<IEnumerable<AppointmentViewDto>> GetAppointmentsByClientId(string? authUserId)
    {

        var client = await _clientRepository.GetByAuthUserId(authUserId!); // Gets the client from AuthUserId
        if(client == null ) // If client is null throw UnauthorizedAccessException
        {
            throw new UnauthorizedAccessException();
        }
        var appointments = await _appointmentRepository.GetByClientId(client.Id); // Calls AppointmentRepository to get all appointments for client
        if (appointments is null || !appointments.Any()) return Enumerable.Empty<AppointmentViewDto>(); // If appointments are null Service returns a empty AppointmentDto List

        
        // Service convert the appointment list to AppointmentDto List
        var appointmentDtos = appointments.Select(appointment => new AppointmentViewDto
        {
            Id = appointment.Id,
            ClientId = appointment.ClientId,
            ClientName = appointment.Client.Name ?? $"Client Id: {appointment.ClientId}",
            HealthcareWorkerId = appointment.HealthcareWorkerId,
            HealthcareWorkerName = appointment.HealthcareWorker.Name ?? $"HealthcareWorker{appointment.HealthcareWorkerId}",
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


    public async Task<IEnumerable<AppointmentViewDto>> GetAppointmentsByHealthcareWorkerId(string? authUserId)
    {

        var worker = await _healthcareWorkerRepository.GetByAuthUserId(authUserId!); // Gets the healthcare worker from AuthUserId
        if(worker == null ) // If worker is null throw UnauthorizedAccessException
        {
            throw new UnauthorizedAccessException();
        }

        var appointments = await _appointmentRepository.GetByHealthcareWorkerId(worker.Id); // Calls AppointmentRepository to get all appointments for worker
        if (appointments is null || !appointments.Any()) return Enumerable.Empty<AppointmentViewDto>(); // If appointments are null Service returns a empty AppointmentDto List

        // Service convert the appointment list to AppointmentDto List
        var appointmentDtos = appointments.Select(appointment => new AppointmentViewDto
        {
            Id = appointment.Id,
            ClientId = appointment.ClientId,
            ClientName = appointment.Client.Name ?? $"Client Id: {appointment.ClientId}",
            HealthcareWorkerId = appointment.HealthcareWorkerId,
            HealthcareWorkerName = appointment.HealthcareWorker.Name ?? $"HealthcareWorker{appointment.HealthcareWorkerId}",
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
    
    public async Task<AppointmentViewDto?> GetById(int id, string? role, string? authUserId)
    {
        var appointment = await _appointmentRepository.GetById(id); // Gets the appointment from DB
        if (appointment is null) return null; // If the appointment is null, Service returns null

        // Service uses our helper function to check if the user is authorized.
        if (!await IsAuthorized(appointment, authUserId, role)) throw new UnauthorizedAccessException();

        // We convert the appointment to dto before returning the dto
        var appointmentDto = new AppointmentViewDto
        {
            Id = appointment.Id,
            ClientId = appointment.ClientId,
            ClientName = appointment.Client.Name ?? $"Client Id: {appointment.ClientId}",
            HealthcareWorkerId = appointment.HealthcareWorkerId,
            HealthcareWorkerName = appointment.HealthcareWorker.Name ?? $"HealthcareWorker{appointment.HealthcareWorkerId}",
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
        
        // Use role to determine how to get the clientId
        int clientId;
        if (role == "Client") // If the role is client we get the clientId from the AuthUserId
        {
            if (string.IsNullOrEmpty(authUserId)) throw new UnauthorizedAccessException(); // If the service to not receive a AuthUserId we throw an UnauthorizedAccessException 
            var client = await _clientRepository.GetByAuthUserId(authUserId) ?? throw new UnauthorizedAccessException(); // And if the AuthUserId does not belong to a Client we do the same
            clientId = client.Id;
        }
        else if (role == "Admin") // If the role is admin we get the clientId from the incoming AppointmentDto
        {
            if (dto.ClientId == 0) throw new ArgumentException("ClientId is required for admin creation."); // If the incoming AppointmentDto does not have a clientId we throw an ArgumentException
            var client = await _clientRepository.GetClientById(dto.ClientId) ?? throw new ArgumentException($"Client {dto.ClientId} not found."); // As well as if the clientId is not from a client in the DB
            clientId = client.Id;
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
        {
            _logger.LogWarning("[AppointmentService] Could not book slot {SlotId} for Appointment creation", slot.Id);
            throw new InvalidOperationException("Could not book the selected slot."); // If the slot is not booked it throws an InvalidOperationException            
        }


        // Create the appointment object from the dto and the booked slot
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

            if (!created)
            {
                _logger.LogWarning("[AppointmentService] Create failed for Appointment (ClientId {ClientId}, SlotId {SlotId}, Start {Start:u}, End {End:u})",
                    appointment.ClientId, appointment.AvailableSlotId, appointment.Start, appointment.End);
                throw new InvalidOperationException("Could not create appointment."); // If creation fails it throws an InvalidOperationException
            } 
            foreach (var t in dto.AppointmentTasks!) // Service loops through the Appointment task and creates each one
            {
                var ok = await _appointmentTaskRepository.Create(new AppointmentTask
                {
                    AppointmentId = appointment.Id,
                    Description = t.Description!,
                    IsCompleted = t.IsCompleted
                });
                if (!ok)
                {
                    // If one of the task fail, it throws an InvalidOperationException
                    _logger.LogWarning("[AppointmentService] Create failed for AppointmentTask {TaskId} for Appointment {AppointmentId}", t.Id, appointment.Id);
                    throw new InvalidOperationException("Could not create appointment task."); 
                } 
            }

            // To get the full appointment, call the db, then convert to dto
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
            _logger.LogInformation("[AppointmentService] Appointment created {AppointmentId}", appointment.Id);
            return appointmentDto;
        } 
        catch // If the try fails, we unbook the slot, and propagate the exception to the controller.  
        {
            _logger.LogWarning("[AppointmentService] Create failed; rolling back slot {SlotId}", slot.Id);
            slot.IsBooked = false;
            await _availableSlotRepository.Update(slot);
            throw;
        }
    }

    public async Task<bool> Update(int id, AppointmentDto appointmentDto, string? role, string? authUserId) 
    {
        if (appointmentDto == null) return false; // If the incoming dto is null, returns false
        var existing = await _appointmentRepository.GetById(id); // Checks if the appointment exists 
        if (existing is null) return false; // If existing appointment is null, returns false

        // Check the authorization 
        if (!await IsAuthorized(existing, authUserId, role))
            throw new UnauthorizedAccessException();

        // Only notes + tasks can change
        var changes = new List<string>(); // To track changes for logging
        
        // Check for notes change
        if (!string.Equals(existing.Notes ?? "", appointmentDto.Notes ?? "", StringComparison.Ordinal))
            changes.Add($"Notes: \"{existing.Notes}\" → \"{appointmentDto.Notes}\""); // If notes changed, log change by adding to changes list

        existing.Notes = appointmentDto.Notes ?? string.Empty; // Update notes

        // AppointmentTask dictionary for easy lookup
        var existingTaskById = existing.AppointmentTasks?.ToDictionary(t => t.Id, t => t);

        // Loop through provided tasks
        foreach (var appointmentTask in appointmentDto.AppointmentTasks ?? []) // [] to avoid null reference
        {
            if (existingTaskById != null && existingTaskById.TryGetValue(appointmentTask.Id, out var t)) // If task exists in DB
            {
                if
                (
                    !string.Equals(t.Description, appointmentTask.Description, StringComparison.Ordinal) || // Check for description change
                    t.IsCompleted != appointmentTask.IsCompleted // Or completion status change
                )
                {
                    // Add change description
                    changes.Add($"Task #{t.Id}: \"{t.Description}\"/{t.IsCompleted} → \"{appointmentTask.Description}\"/{appointmentTask.IsCompleted}");
                }
                // Update task fields
                t.Description = appointmentTask.Description;
                t.IsCompleted = appointmentTask.IsCompleted;
                await _appointmentTaskRepository.Update(t);
            }
            else // Task not in DB, must be new. Create new task
            {
                // New task creation
                var newTask = new AppointmentTask
                {
                    AppointmentId = existing.Id,
                    Description = appointmentTask.Description,
                    IsCompleted = appointmentTask.IsCompleted
                };
                await _appointmentTaskRepository.Create(newTask);
                changes.Add($"Task + \"{appointmentTask.Description}\" (new)"); // Log new task addition
            }
        }

        if (existingTaskById != null) // If there are existing tasks
        {
            // Get incoming task Ids from the payload
            var incomingIds = appointmentDto.AppointmentTasks!
                .Where(x => x.Id > 0)
                .Select(x => x.Id)
                .ToHashSet(); // HashSet<int>, not nullable for fast lookup

            // Remove tasks that are not present in the incoming payload
            var toRemove = existingTaskById
                .Where(x => !incomingIds.Contains(x.Key))
                .Select(x => x.Value)
                .ToList();

            // Loop through toRemove to remove tasks
            foreach (var removeTask in toRemove)
            {
                changes.Add($"Task - #{removeTask.Id}: \"{removeTask.Description}\" (removed)"); // Log task removal
                await _appointmentTaskRepository.Delete(removeTask.Id);
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

        _logger.LogInformation("[AppointmentService] Appointment updated {AppointmentId}", existing.Id);

        var description = string.Join("; ", changes); // Combine changes into single description
        if (description.Length > 500) description = description[..500]; // Truncate if too long

        // Create change log entry
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
            _logger.LogWarning("[AppointmentService] failed to create change log for AppointmentId {AppointmentId:0000}.", appointmentDto.Id);
        }
        else
        {
            _logger.LogInformation("[AppointmentService] Added Change log for AppointmentId {AppointmentId:0000}", existing.Id);  
        }

        return true;
    }
    
    public async Task<bool> Delete(int id, string? role, string? authUserId)
    {
        var appointment = await _appointmentRepository.GetById(id); // Get appointment by ID
        if (appointment == null) // If not found, log error and return false
        {
            _logger.LogWarning("[AppointmentService] appointment not found when deleting for AppointmentId {AppointmentId:0000}", id);
            return false;
        }

        // Check authorization
        if (!await IsAuthorized(appointment, authUserId, role))
            throw new UnauthorizedAccessException();

        bool returnOk = await _appointmentRepository.Delete(appointment.Id); // Delete appointment
        if (!returnOk) // If deletion fails, log error and throw InvalidOperationException
        {
            _logger.LogWarning("[AppointmentService] appointment deletion failed for AppointmentId {AppointmentId:0000}", appointment.Id);
            throw new InvalidOperationException("Appointment deletion failed.");
        }
        _logger.LogInformation("[AppointmentService] Appointment deleted {AppointmentId}", appointment.Id);

        // Free slot first 
        if (appointment.AvailableSlotId.HasValue)
        {
            var slot = await _availableSlotRepository.GetById(appointment.AvailableSlotId.Value); // Get the slot
            if (slot is not null)
            {
                slot.IsBooked = false; // Free up the slot
                var slotOk = await _availableSlotRepository.Update(slot); // Update slot to free it up
                if (!slotOk)
                {
                    _logger.LogWarning("[AppointmentService] failed to free up slot for AppointmentId {AppointmentId:0000}", id); // Log warning if freeing slot fails
                }
                else
                {
                    _logger.LogInformation("[AppointmentService] Appointment freed up slot {SlotId}", slot.Id);  
                }
            }
        }
        
        // Create changelog 
        bool logged = await _changeLogRepository.Create(new ChangeLog
        {
            AppointmentId = appointment.Id,
            AppointmentIdSnapshot = appointment.Id,
            ChangeDate = DateTime.UtcNow,
            ChangedByUserId = authUserId!, // Cannot be null since we check authorization earlier
            ChangeDescription = "Appointment deleted."
        });

        if (!logged) // If logging fails, log warning
        {
            _logger.LogWarning("[AppointmentService] failed to create change log for deleted AppointmentId {AppointmentId:0000}", appointment.Id);
        }
        else
        {
            _logger.LogInformation("[AppointmentService] Added Change log for deleted AppointmentId {AppointmentId:0000}", appointment.Id);
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