
using api.DAL;
using api.DTO;
using api.Models;

namespace api.Services;
public class AvailableSlotService: IAvailableSlotService
{
    private readonly IAvailableSlotRepository _availableSlotRepository;
    private readonly IHealthcareWorkerRepository _healthcareWorkerRepository;
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

    public async Task<IEnumerable<AvailableSlotDto>> GetAll(bool isAdmin)
    {
        if (!isAdmin)
        {
            return Enumerable.Empty<AvailableSlotDto>();

        }
        var allSlots = await _availableSlotRepository.GetAll();
        if (allSlots is null || !allSlots.Any()) return Enumerable.Empty<AvailableSlotDto>();
        var allSlotsDtos = allSlots.Select(s => new AvailableSlotDto
        {
            Id = s.Id,
            HealthcareWorkerId = s.HealthcareWorkerId,
            Start = s.Start,
            End = s.End,
            IsBooked = s.IsBooked
        });
        return allSlotsDtos;
    }

    public async Task<IEnumerable<AvailableSlotDto>> GetAllByWorkerId(string? authUserId)
    {
        var currentWorkerId = await ResolveWorkerIdAsync(authUserId);
        if (!currentWorkerId.HasValue) return Enumerable.Empty<AvailableSlotDto>();
        var slots = await _availableSlotRepository.GetByWorkerId(currentWorkerId.Value);
        if (slots is null || !slots.Any()) return Enumerable.Empty<AvailableSlotDto>();
        var slotDtos = slots.Select(s => new AvailableSlotDto
        {
            Id = s.Id,
            HealthcareWorkerId = s.HealthcareWorkerId,
            Start = s.Start,
            End = s.End,
            IsBooked = s.IsBooked
        });
        return slotDtos;
    }

    public async Task<AvailableSlotDto?> GetById(int id, bool isAdmin, string? authUserId)
    {
        var existing = await _availableSlotRepository.GetById(id);
        if (existing is null) return null; // Handle by controller by using NotFound()

        if (!await IsAuthorizedForSlot(existing, isAdmin, authUserId))
            throw new UnauthorizedAccessException(); // Handle by controller by using Forbid()

        return new AvailableSlotDto
        {
            Id = existing.Id,
            HealthcareWorkerId = existing.HealthcareWorkerId,
            Start = existing.Start,
            End = existing.End,
            IsBooked = existing.IsBooked
        };
    }
    public async Task<AvailableSlotDto> Create(AvailableSlotDto dto, bool isAdmin, string? authUserId)
    {
        int workerId;
        if (isAdmin)
        {
            workerId = dto.HealthcareWorkerId;
        }
        else
        {
            var currentWorkerId = await ResolveWorkerIdAsync(authUserId);
            if (!currentWorkerId.HasValue) throw new UnauthorizedAccessException();
            workerId = currentWorkerId.Value;
        }

        var slot = new AvailableSlot
        {
            HealthcareWorkerId = workerId,
            Start = dto.Start,
            End = dto.End,
            IsBooked = false
        };
        
        var ok = await _availableSlotRepository.Create(slot);
        if (!ok)
        {
            _logger.LogError("[AvailableSlotService] create failed {@slot}", slot);
            throw new InvalidOperationException("Failed to create slot.");
        }

        var createdSlotDto = new AvailableSlotDto
        {
            Id = slot.Id,
            HealthcareWorkerId = workerId,
            Start = dto.Start,
            End = dto.End,
            IsBooked = slot.IsBooked
        };

        return createdSlotDto;
    }
    public async Task<bool> Update(int id, AvailableSlotDto dto, bool isAdmin, string? authUserId)
    {
        var existing = await _availableSlotRepository.GetById(id);
        if (existing is null) return false;

        if (!await IsAuthorizedForSlot(existing, isAdmin, authUserId)) throw new UnauthorizedAccessException();

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
        var updated = await _availableSlotRepository.Update(existing);
        if (!updated)
        {
            _logger.LogError("[AvailableSlotService] update failed {@slot}", existing);
        }
        return updated;

    }
    public async Task<bool> Delete(int id, bool isAdmin, string? authUserId)
    {
        var existing = await _availableSlotRepository.GetById(id);
        if (existing is null) return false;

        if (!await IsAuthorizedForSlot(existing, isAdmin, authUserId)) throw new UnauthorizedAccessException();
        if (existing.IsBooked) throw new ArgumentException("Cannot delete a booked slot. Please cancel the appointment first.");
        var deleted = await _availableSlotRepository.Delete(id);
        if (!deleted)
        {
            _logger.LogError("[AvailableSlotService] delete failed for {Id:0000}", id);
        }
        return deleted;

    }

    public async Task<IEnumerable<AvailableSlotDto>> GetAllUnbooked()
    {
        var allSlots = await _availableSlotRepository.GetAllUnbooked();
        if (allSlots is null || !allSlots.Any()) return Enumerable.Empty<AvailableSlotDto>();
        var allSlotsDtos = allSlots.Select(s => new AvailableSlotDto
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