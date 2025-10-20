using HomecareAppointmentManagement.DAL;
using HomecareAppointmentManagement.DTO;
using HomecareAppointmentManagment.DTO;
using HomecareAppointmentManagment.Infrastructure;
using HomecareAppointmentManagment.Models;
using HomecareAppointmentManagment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace HomecareAppointmentManagment.Controllers;

// [Authorize(Roles = "Client,Admin,HealthcareWorker")] // Authorize all relevant roles
[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAvailableSlotRepository _availableSlotRepository; // For slot management
    private readonly IClientRepository _clientRepository; // For client management
    private readonly IAppointmentTaskRepository _appointmentTaskRepository; // For appointment tasks
    private readonly IChangeLogRepository _changeLogRepository; // For change logs
    private readonly ILogger<AppointmentController> _logger;

    public AppointmentController
    (
        IAppointmentRepository appointmentRepository,
        IAvailableSlotRepository availableSlotRepository,
        IClientRepository clientRepository,
        IAppointmentTaskRepository appointmentTaskRepository,
        IChangeLogRepository changeLogRepository,
        ILogger<AppointmentController> logger
    )
    {
        _appointmentRepository = appointmentRepository;
        _availableSlotRepository = availableSlotRepository;
        _clientRepository = clientRepository;
        _appointmentTaskRepository = appointmentTaskRepository;
        _changeLogRepository = changeLogRepository;
        _logger = logger;
    }

    // public async Task<IActionResult> Index()
    // {
    //     // Determine role and fetch appropriate appointments
    //     if (User.IsInRole("Admin"))
    //     {
    //         var all = await _appointmentRepository.GetAll();
    //         if (all == null)
    //         {
    //             _logger.LogError("[AppointmentController] appointment list not found while executing _appointmentRepository.GetAll()");
    //             return NotFound("Appointment list not found");
    //         }
    //         return View(new AppointmentViewModel
    //         {
    //             ViewMode = AppointmentViewMode.Admin,
    //             Appointments = all
    //         });
    //     }

    //     if (User.IsInRole("Client"))
    //     {
    //         var clientId = User.TryGetClientId();
    //         if (clientId is null) return Forbid();
    //         var clientAppointments = await _appointmentRepository.GetByClientId(clientId.Value);
    //         if (clientAppointments == null)
    //         {
    //             _logger.LogError("[AppointmentController] appointment list not found while executing _appointmentRepository.GetByClientId() for ClientId {ClientId:0000}", clientId.Value);
    //             return NotFound("Appointment list not found");
    //         }
    //         return View(new AppointmentViewModel
    //         {
    //             ViewMode = AppointmentViewMode.Client,
    //             Appointments = clientAppointments
    //         });
    //     }

    //     if (User.IsInRole("HealthcareWorker"))
    //     {
    //         var workerId = User.TryGetHealthcareWorkerId();
    //         if (workerId is null) return Forbid();
    //         var workerAppointments = await _appointmentRepository.GetByHealthcareWorkerId(workerId.Value);
    //         if (workerAppointments == null)
    //         {
    //             _logger.LogError("[AppointmentController] appointment list not found while executing _appointmentRepository.GetByHealthcareWorkerId() for HealthcareWorkerId {HealthcareWorkerId:0000}", workerId.Value);
    //             return NotFound("Appointment list not found");
    //         }
    //         return View(new AppointmentViewModel
    //         {
    //             ViewMode = AppointmentViewMode.Worker,
    //             Appointments = workerAppointments
    //         });
    //     }
    //     // If we reach here, the user has no valid role
    //     return Forbid();

    // }

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

    [HttpGet("/{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _appointmentRepository.GetById(id);
        if (appointment == null)
        {
            _logger.LogError("[AppointmentController] appointment not found while executing _appointmentRepository.GetById() for AppointmentId {AppointmentId:0000}", id);
            return NotFound("Appointment not found");
        }

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

    // [Authorize(Roles = "Admin,Client")] // Restrict access to Admins and Clients, not HealthcareWorkers
    // [HttpGet]
    // public async Task<IActionResult> Create()
    // {
    //     var slots = await _availableSlotRepository.GetAll(); // Get all available slots

    //     var viewModel = new AppointmentCreateViewModel // Create AppointmentCreateViewModel
    //     {
    //         IsAdmin = User.IsInRole("Admin"), // Flag if user is Admin 
    //         Slots = (slots ?? Enumerable.Empty<AvailableSlot>())
    //             .Where(s => !s.IsBooked && s.Start > DateTime.UtcNow)
    //             .OrderBy(s => s.Start) // Only future unbooked slots
    //     };

    //     if (viewModel.IsAdmin) // If Admin, populate clients list since admin can create for any client
    //     {
    //         viewModel.Clients = await _clientRepository.GetAll();
    //     }

    //     if (viewModel.AppointmentTasks == null || viewModel.AppointmentTasks.Count == 0)
    //     {
    //         viewModel.AppointmentTasks = new() { new AppointmentTaskViewModel() }; // Initialize with one empty task
    //     }
    //     return View(viewModel);

    // }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AppointmentDto appointmentDto)
    {

        // Local function to rehydrate model data on validation failure for redisplay
        // async Task RehydrateAsync()
        // {
        //     var allSlots = await _availableSlotRepository.GetAll();
        //     model.Slots = (allSlots ?? Enumerable.Empty<AvailableSlot>())
        //         .Where(s => !s.IsBooked && s.Start > DateTime.UtcNow)
        //         .OrderBy(s => s.Start)
        //         .ToList();

        //     model.IsAdmin = User.IsInRole("Admin");
        //     if (model.IsAdmin)
        //         model.Clients = (await _clientRepository.GetAll())?.ToList();

        //     if (model.AppointmentTasks == null || model.AppointmentTasks.Count == 0)
        //         model.AppointmentTasks = new() { new AppointmentTaskViewModel() };
        // }


        // Flags first
        // model.IsAdmin = User.IsInRole("Admin");

        // Normalize tasks once
        appointmentDto.AppointmentTasks = (appointmentDto.AppointmentTasks ?? new())
            .Where(t => !string.IsNullOrWhiteSpace(t.Description))
            .ToList();

        if (appointmentDto.AppointmentTasks.Count == 0)
            return BadRequest("Enter at least one task."); // At least one task required. 
        

        // // Conditional admin requirement. Needs to specify client if admin
        // if (model.IsAdmin && model.SelectedClientId is null)
        //     ModelState.AddModelError(nameof(model.SelectedClientId),
        //         "Admin must specify a client to create an appointment for.");

        // Slot requirement check 
        if (appointmentDto.AvailableSlotId is null)
        {
            _logger.LogError("[AppointmentController] appointment needs to have slot to mark slot");
            return BadRequest("Please choose an available slot.");

        }
            

        // If model state invalid, rehydrate and return
        // if (!ModelState.IsValid)
        // {
        //     await RehydrateAsync();
        //     return View(model);
        // }

        // Validate selected slot
        var slot = await _availableSlotRepository.GetById(appointmentDto.AvailableSlotId.Value); // Ignore, null checked above

        if (slot is null || slot.IsBooked || slot.Start <= DateTime.UtcNow) // Slot must exist, be unbooked, and in the future
        {

            return BadRequest("That slot is no longer available."); // Add model error
        }

        // Determine client ID 
        // int? clientId = null;
        // if (User.IsInRole("Client")) // If Client, use their own client ID
        // {
        //     clientId = User.TryGetClientId();
        //     if (clientId is null) return Forbid(); // If we can't get client ID, forbid
        // }
        // else if (model.IsAdmin) // If Admin, use selected client ID
        // {
        //     // This should be non-null due to the earlier validation
        //     clientId = model.SelectedClientId;
        // }

        // Create appointment object
        var appointment = new Appointment
        {
            ClientId = appointmentDto.ClientId,
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
        // Redirect to index on success
        return CreatedAtAction(nameof(AppointmentList), new { id = appointment.Id }, appointment);

    }


    // [HttpGet]
    // public async Task<IActionResult> Edit(int id)
    // {
    //     var appointment = await _appointmentRepository.GetById(id); // Get appointment by ID
    //     if (appointment == null)
    //     {
    //         _logger.LogWarning("[AppointmentController] appointment not found when editing for AppointmentId {AppointmentId:0000}", id);
    //         return NotFound("Appointment not found");
    //     }

    //     var viewModel = new AppointmentEditViewModel // Create AppointmentEditViewModel
    //     {
    //         Id = appointment.Id,
    //         ClientName = appointment.Client?.Name ?? $"Client #{appointment.ClientId}", // Client name or fallback
    //         HealthcareWorkerName = appointment.HealthcareWorker?.Name ?? $"Worker #{appointment.HealthcareWorkerId}", // Worker name or fallback
    //         Start = appointment.Start,
    //         End = appointment.End,
    //         Notes = appointment.Notes ?? string.Empty,
    //         AppointmentTasks = (appointment.AppointmentTasks ?? new List<AppointmentTask>())
    //             .Select(task => new AppointmentTaskEditItemViewModel
    //             {
    //                 Id = task.Id,
    //                 Description = task.Description,
    //                 IsCompleted = task.IsCompleted
    //             })
    //             .ToList()
    //     };

    //     return View(viewModel);
    // }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AppointmentDto appointmentDto)
    {
        if (!ModelState.IsValid) // Validate model state
        {
            _logger.LogWarning("[AppointmentController] appointment update failed for AppointmentId {AppointmentId:0000}", appointmentDto.Id);
            return BadRequest("Bad request");
        }

        var existing = await _appointmentRepository.GetById(appointmentDto.Id);
        if (existing == null) // If appointment not found, return NotFound
        {
            _logger.LogError("[AppointmentController] appointment not found during edit for AppointmentId {AppointmentId:0000}", appointmentDto.Id);
            return NotFound("Appointment not found");
        }

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
            // Nothing to update/log; just go back
            return RedirectToAction(nameof(AppointmentList), new { id = appointmentDto.Id });
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
            AppointmentId = appointmentDto.Id,
            ChangeDate = DateTime.UtcNow,
            ChangedByUserId = "userId",
            ChangeDescription = description
        };

        var logged = await _changeLogRepository.Create(log); // Log the change
        if (!logged)
        {
            // If logging fails, log warning
            _logger.LogWarning("[AppointmentController] failed to create change log for AppointmentId {AppointmentId:0000}. Description: {Description}", appointmentDto.Id, description);
        }

        return RedirectToAction(nameof(Index));

    }

    // [HttpGet]
    // public async Task<IActionResult> Delete(int id)
    // {
    //     var appointment = await _appointmentRepository.GetById(id);
    //     if (appointment == null)
    //     {
    //         _logger.LogError("[AppointmentController] appointment not found when deleting for AppointmentId {AppointmentId:0000}", id);
    //         return NotFound("Appointment not found");
    //     }
    //     return View(appointment);
    // }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        Appointment? appointment = await _appointmentRepository.GetById(id); // Get appointment by ID
        if (appointment == null) // If not found, log error and return NotFound
        {
            _logger.LogError("[AppointmentController] appointment not found when deleting for AppointmentId {AppointmentId:0000}", id);
            return NotFound("Appointment not found");
        }
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
        // var userId = User.TryGetUserId() ?? string.Empty; // Get user ID for logging

        // Create change log entry for deletion
        bool logged = await _changeLogRepository.Create(new ChangeLog
        {
            AppointmentId = appointment.Id,
            ChangeDate = DateTime.UtcNow,
            ChangedByUserId = "userId",
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
            return BadRequest("Appointment deletion failed");
        }

        return RedirectToAction(nameof(AppointmentList));
    }

    [HttpGet("/{id:int}/changelog")]
    public async Task<IActionResult> ChangeLog(int id)
    {
        // Get appointment to verify existence and for authorization
        var appointment = await _appointmentRepository.GetById(id);
        if (appointment == null) return NotFound("Appointment not found");

        // Authorization based on user  
        // var isAdmin = User.IsInRole("Admin");
        // var isClientOwner = User.IsInRole("Client") && User.TryGetClientId() == appointment.ClientId;
        // var isWorkerOwner = User.IsInRole("HealthcareWorker") && User.TryGetHealthcareWorkerId() == appointment.HealthcareWorkerId;

        // Check if user from appointment are authorized
        // if (!(isAdmin || isClientOwner || isWorkerOwner)) return Forbid();

        var logs = await _changeLogRepository.GetByAppointmentId(id);

        if (logs == null) return NotFound("Change log to found");

        var logDtos = logs.Select(log => new ChangeLogDto
        {
            Id = log.Id,
            AppointmentId  = log.AppointmentId,
            ChangeDate = log.ChangeDate,
            ChangedByUserId = log.ChangedByUserId,
            ChangeDescription = log.ChangeDescription,
        });

        return Ok(logDtos);

    }
}


