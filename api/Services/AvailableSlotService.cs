
using api.DAL;
using api.DTO;
using api.Models;

namespace api.Services;
public class AvailableSlotService: IAvailableSlotService
{
    private readonly IAvailableSlotRepository _availableSlotRepository; // Repository for available slots
    private readonly IHealthcareWorkerRepository _healthcareWorkerRepository; // Repository for healthcare workers
    private readonly ILogger<AvailableSlotService> _logger;

    public AvailableSlotService(IAvailableSlotRepository availableSlotRepository, ILogger<AvailableSlotService> logger, IHealthcareWorkerRepository healthcareWorkerRepository)
    {
        _availableSlotRepository = availableSlotRepository;
        _healthcareWorkerRepository = healthcareWorkerRepository;
        _logger = logger;

    }

    // Private helper method for getting current worker id by authUserId
    private async Task<int?> ResolveWorkerIdAsync(string? authUserId)
    {
        if (string.IsNullOrWhiteSpace(authUserId)) return null;
        var worker = await _healthcareWorkerRepository.GetByAuthUserId(authUserId);
        return worker?.Id;
    }

    // Private helper method that helps check if the user is to view or edit a specific slot
    private async Task<bool> IsAuthorizedForSlot(AvailableSlot slot, bool isAdmin, string? authUserId)
    {
        if (isAdmin) return true;
        var currentWorkerId = await ResolveWorkerIdAsync(authUserId);
        return currentWorkerId.HasValue && slot.HealthcareWorkerId == currentWorkerId.Value;
    }
    
    public async Task<IEnumerable<AvailableSlotDto>> GetAll(bool isAdmin) // Admin can see all slots
    {
        if (!isAdmin) // If not admin, return empty
        {
            return Enumerable.Empty<AvailableSlotDto>();

        }
        var allSlots = await _availableSlotRepository.GetAll(); // Calls repository to get all slots
        if (allSlots is null || !allSlots.Any()) return Enumerable.Empty<AvailableSlotDto>(); // Return empty if no slots found
        var allSlotsDtos = allSlots.Select(s => new AvailableSlotDto // Map to DTO
        {
            Id = s.Id,
            HealthcareWorkerId = s.HealthcareWorkerId,
            Start = s.Start,
            End = s.End,
            IsBooked = s.IsBooked
        });
        return allSlotsDtos;
    }

    public async Task<IEnumerable<AvailableSlotDto>> GetAllByWorkerId(string? authUserId) // Healthcare workers can see their own slots by their authUserId
    {
        var currentWorkerId = await ResolveWorkerIdAsync(authUserId); // Get current worker id
        if (!currentWorkerId.HasValue) return Enumerable.Empty<AvailableSlotDto>(); // Return empty if no worker found
        var slots = await _availableSlotRepository.GetByWorkerId(currentWorkerId.Value); // Calls repository to get slots by worker id
        if (slots is null || !slots.Any()) return Enumerable.Empty<AvailableSlotDto>(); // Return empty if no slots found
        var slotDtos = slots.Select(s => new AvailableSlotDto // Map to DTO
        {
            Id = s.Id,
            HealthcareWorkerId = s.HealthcareWorkerId,
            Start = s.Start,
            End = s.End,
            IsBooked = s.IsBooked
        });
        return slotDtos;
    }

    public async Task<AvailableSlotDto?> GetById(int id, bool isAdmin, string? authUserId) // Healthcare workers and Admins can get slot by ID
    {
        var existing = await _availableSlotRepository.GetById(id); // Calls repository to get slot by ID
        if (existing is null) return null; // Handle by controller by using NotFound()

        if (!await IsAuthorizedForSlot(existing, isAdmin, authUserId))
            throw new UnauthorizedAccessException(); // Handle by controller by using Forbid()

        return new AvailableSlotDto // Map to DTO
        {
            Id = existing.Id,
            HealthcareWorkerId = existing.HealthcareWorkerId,
            Start = existing.Start,
            End = existing.End,
            IsBooked = existing.IsBooked
        };
    }
    public async Task<AvailableSlotDto> Create(AvailableSlotDto dto, bool isAdmin, string? authUserId) // Healthcare workers and Admins can create slots
    {
        int workerId;
        // Determine healthcare worker ID based on role
        if (isAdmin) // If Admin, use provided worker ID
        {
            workerId = dto.HealthcareWorkerId;
        }
        else // If Healthcare Worker, resolve worker ID from authUserId
        {
            var currentWorkerId = await ResolveWorkerIdAsync(authUserId);
            if (!currentWorkerId.HasValue) throw new UnauthorizedAccessException(); // If no worker found, throw UnauthorizedAccessException
            workerId = currentWorkerId.Value;
        }

        var slot = new AvailableSlot // Create new AvailableSlot model
        {
            HealthcareWorkerId = workerId,
            Start = dto.Start,
            End = dto.End,
            IsBooked = false
        };

        var ok = await _availableSlotRepository.Create(slot); // Calls repository to create slot
        if (!ok) // If creation failed, log error and throw invalid operation exception
        {
            _logger.LogWarning("[AvailableSlotService] create failed {Id:0000}", slot.Id);
            throw new InvalidOperationException("Failed to create slot.");
        }

        // Return created slot as DTO
        var createdSlotDto = new AvailableSlotDto
        {
            Id = slot.Id,
            HealthcareWorkerId = workerId,
            Start = dto.Start,
            End = dto.End,
            IsBooked = slot.IsBooked
        };

        _logger.LogInformation("[AvailableSlotService] Created available slot {Id:0000}", slot.Id);
        return createdSlotDto;
    }

    public async Task<bool> Update(int id, AvailableSlotDto dto, bool isAdmin, string? authUserId) // Healthcare workers and Admins can update slots
    {
        var existing = await _availableSlotRepository.GetById(id); // Calls repository to get slot by ID
        if (existing is null) return false; // If no slot found, return false, handle by controller by using NotFound()

        // If not authorized, throw UnauthorizedAccessException, handle by controller by using Forbid()
        if (!await IsAuthorizedForSlot(existing, isAdmin, authUserId)) throw new UnauthorizedAccessException();

        // Update fields based on role
        // Admin can update all fields, Healthcare Worker cannot update booked slots
        if (isAdmin)
        {
            existing.HealthcareWorkerId = dto.HealthcareWorkerId;
            existing.Start = dto.Start;
            existing.End = dto.End;
            existing.IsBooked = dto.IsBooked;
        }
        else
        {
            if (existing.IsBooked) throw new ArgumentException("Cannot edit a booked slot.");
            existing.Start = dto.Start;
            existing.End = dto.End;
            existing.IsBooked = false; // Cannot book a slot via this endpoint
        }
        var updated = await _availableSlotRepository.Update(existing); // Calls repository to update slot

        // If update failed, log error, handle by controller by using 500 Internal Server Error
        if (!updated)
        {
            _logger.LogWarning("[AvailableSlotService] update failed {id:0000}", existing.Id);
            throw new InvalidOperationException("Failed to update slot.");
        }

        _logger.LogInformation("[AvailableSlotService] Updated available slot {Id:0000}", existing.Id);
        return updated;

    }
    
    public async Task<bool> Delete(int id, bool isAdmin, string? authUserId) // Healthcare workers and Admins can delete slots 
    {
        var existing = await _availableSlotRepository.GetById(id); // Calls repository to get slot by ID
        if (existing is null) return false; // If no slot found, return false, handle by controller by using NotFound()

        // If not authorized, throw UnauthorizedAccessException, handle by controller by using Forbid()
        if (!await IsAuthorizedForSlot(existing, isAdmin, authUserId)) throw new UnauthorizedAccessException();  

        // If slot is booked, cannot delete, throw ArgumentException, handle by controller by using BadRequest()
        if (existing.IsBooked) throw new ArgumentException("Cannot delete a booked slot. Please cancel the appointment first.");
        var deleted = await _availableSlotRepository.Delete(id);

        // If delete failed, log error, handle by controller by using 500 Internal Server Error
        if (!deleted)
        {
            _logger.LogWarning("[AvailableSlotService] Delete failed for {Id:0000}", id);
            return deleted;
        }
        _logger.LogInformation("[AvailableSlotService] Deleted available slot Id {Id:0000}", id);
        return deleted;

    }

    public async Task<IEnumerable<AvailableSlotDto>> GetAllUnbooked() // Clients can see all unbooked slots
    {
        var allSlots = await _availableSlotRepository.GetAllUnbooked(); // Calls repository to get all unbooked slots
        if (allSlots is null || !allSlots.Any()) return Enumerable.Empty<AvailableSlotDto>(); // Return empty if no slots found
        
        var allSlotsDtos = allSlots.Select(s => new AvailableSlotDto // Map to DTO
        {
            Id = s.Id,
            HealthcareWorkerId = s.HealthcareWorkerId,
            Start = s.Start,
            End = s.End,
            IsBooked = s.IsBooked
        });
        return allSlotsDtos; 
    }

}