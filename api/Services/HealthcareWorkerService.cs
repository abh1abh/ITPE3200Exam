using api.DAL;
using api.DTO;
using api.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace api.Services;

public class HealthcareWorkerService: IHealthcareWorkerService
{
    private readonly IHealthcareWorkerRepository _repository;
    private readonly ILogger<HealthcareWorkerService> _logger;

    public HealthcareWorkerService(IHealthcareWorkerRepository repository, ILogger<HealthcareWorkerService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    private bool IsAuthorized(HealthcareWorker worker, string? authUserId, string? role) // Check if user is authorized
    {
        if (string.IsNullOrEmpty(authUserId)) return false;

        if (role == "Admin") return true; // Admins are always authorized

        var ok = false;

        if (role == "HealthcareWorker" && worker.AuthUserId== authUserId) // HealthcareWorkers can access their own data
        {
            ok = true;
        }
        return ok;
    }
    
    public async Task<IEnumerable<HealthcareWorkerDto>> GetAll(bool isAdmin) // Get all healthcare workers
    {
        if (!isAdmin) // If not admin, return empty
        {
            _logger.LogWarning("[HealthcareWorkerService] Unauthorized access attempt to get all healthcare workers by non-admin user.");
            return Enumerable.Empty<HealthcareWorkerDto>(); // return empty enumerable

        }
        var healthcareWorkers = await _repository.GetAll(); // Get all healthcare workers from repository
        if (healthcareWorkers == null || !healthcareWorkers.Any())
        {
            _logger.LogWarning("[HealthcareWorkerService] No healthcare workers found.");
            return Enumerable.Empty<HealthcareWorkerDto>(); // return empty enumerable
        }

        var workerDtos = healthcareWorkers.Select(w => new HealthcareWorkerDto // Map HealthcareWorker to HealthcareWorkerDto
        {
            Id = w.Id,
            Name = w.Name,
            Address = w.Address,
            Phone = w.Phone,
            Email = w.Email,
            AuthUserId = w.AuthUserId
        });

        return workerDtos; // return empty enumerable if no healthcare workers found
    }
    public async Task<HealthcareWorkerDto?> GetById(int id, string authUserId, string role) // Get healthcare worker by Id
    {
        var worker = await _repository.GetById(id); // Get healthcare worker from repository
        if (worker == null)
        {
            _logger.LogError("[HealthcareWorkerService] Healthcare worker not found for Id {Id:0000}", id);
            return null;
        }
        if(!IsAuthorized(worker, authUserId, role))
        {
            _logger.LogWarning("[ClientService] Unauthorized access attempt by AuthUserId {AuthUserId} to delete HealthcareWorkerId {HealthcareWorkerId:0000}", authUserId, id);
            throw new UnauthorizedAccessException("You are not authorized to delete this client.");
        }

        var workerDto = new HealthcareWorkerDto // Map HealthcareWorker to HealthcareWorkerDto
        {
            Id = worker.Id,
            Name = worker.Name,
            Address = worker.Address,
            Phone = worker.Phone,
            Email = worker.Email,
            AuthUserId = worker.AuthUserId
        };

        return workerDto; // return null if not found
    }
    public async Task<HealthcareWorkerDto?> GetByAuthUserId(string authUserId, string authId, string role) // Get healthcare worker by AuthUserId
    {
        var worker = await _repository.GetByAuthUserId(authUserId); // Get healthcare worker from repository
        if (worker == null)
        {
            _logger.LogError("[HealthcareWorkerService] Healthcare worker not found for AuthUserId {AuthUserId}", authUserId);
            return null;
        }
        if(!IsAuthorized(worker, authId, role)) // Check if user is authorized
        {
            _logger.LogWarning("[ClientService] Unauthorized access attempt by AuthUserId {AuthUserId} to delete HealthcareWorkerId {HealthcareWorkerId:0000}", authId, worker.Id);
            throw new UnauthorizedAccessException("You are not authorized to delete this client.");
        }
        var workerDto = new HealthcareWorkerDto // Map HealthcareWorker to HealthcareWorkerDto
        {
            Id = worker.Id,
            Name = worker.Name,
            Address = worker.Address,
            Phone = worker.Phone,
            Email = worker.Email,
            AuthUserId = worker.AuthUserId
        };

        return workerDto; // return null if not found
    }
    public async Task<HealthcareWorkerDto> Create(RegisterDto dto, string authId, bool isAdmin) // Create new healthcare worker
    {
        if (!isAdmin) // Only admins can create healthcare workers
        {
            _logger.LogWarning("[HealthcareWorkerService] Unauthorized creation attempt. Only Admins can create healthcare workers. AuthUserId: {AuthUserId}", authId);
            throw new UnauthorizedAccessException("Only Admins can create healthcare workers.");
        }
        var worker = new HealthcareWorker // Map HealthcareWorkerDto to HealthcareWorker
        {
            Name = dto.Name,
            Address = dto.Address,
            Phone = dto.Phone,
            Email = dto.Email,
            AuthUserId = authId
        };

        bool created = await _repository.Create(worker); // Create healthcare worker in repository
        if (!created)
        {
            _logger.LogError("[HealthcareWorkerService] Healthcare worker creation failed {@worker}", worker);
            throw new InvalidOperationException("Create failed");

        }

        var createdDto = new HealthcareWorkerDto // Map created HealthcareWorker to HealthcareWorkerDto
        {
            Id = worker.Id,
            Name = worker.Name,
            Address = worker.Address,
            Phone = worker.Phone,
            Email = worker.Email,
            AuthUserId = worker.AuthUserId
        };

        return createdDto; // return created healthcare worker dto
    }
    public async Task<bool> Update(UpdateUserDto userDto, string authUserId, string role) // Update healthcare worker
    {
        var id = userDto.Id; 
        var existingWorker = await _repository.GetById(id); // Get existing healthcare worker from repository
        if (existingWorker == null)
        {
            return false;
        }
        if(!IsAuthorized(existingWorker, authUserId, role)) // Check if user is authorized
        {
            _logger.LogWarning("[HealthcareWorkerService] Unauthorized access attempt by AuthUserId {AuthUserId} to update HealthcareWorkerId {HealthcareWorkerId:0000}", authUserId, id);
            throw new UnauthorizedAccessException("You are not authorized to update this Healthcare Worker.");
        }

        existingWorker.Name = userDto.Name; // Update healthcare worker properties
        existingWorker.Address = userDto.Address;
        existingWorker.Phone = userDto.Phone;
        existingWorker.Email = userDto.Email;

        bool updated = await _repository.Update(existingWorker); // Update healthcare worker in repository
        if (!updated)
        {
            _logger.LogError("[HealthcareWorkerService] Update failed for Id {Id:0000}, {@worker}", id, existingWorker);
            throw new InvalidOperationException($"Update operation failed for Id {id}");
        }
        return updated;
    }
    public async Task<bool> Delete(int id, string authUserId, string role) // Delete healthcare worker by Id
    {
         var worker = await _repository.GetById(id); // Get existing healthcare worker from repository
        if (worker is null)
        {
            _logger.LogError("[HealthcareWorkerService] Healthcare worker not found for Id {Id:0000}", id);
            return false;
        }
        if(!IsAuthorized(worker, authUserId, role)) // Check if user is authorized
        {
            _logger.LogWarning("[ClientService] Unauthorized access attempt by AuthUserId {AuthUserId} to delete HealthcareWorkerId {HealthcareWorkerId:0000}", authUserId, id);
            throw new UnauthorizedAccessException("You are not authorized to delete this Healthcare Worker.");
        }
        bool deleted = await _repository.Delete(id); // Delete healthcare worker from repository
        if (!deleted)
        {
            _logger.LogError("[HealthcareWorkerService] Deletion failed for Id {Id:0000}", id);
            throw new InvalidOperationException($"Deletion operation failed for Id {id}");
        }
        return deleted; // return true if deleted
    }
}