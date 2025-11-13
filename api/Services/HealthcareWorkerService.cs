using api.DAL;
using api.DTO;
using api.Models;

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
    
    public async Task<IEnumerable<HealthcareWorkerDto>> GetAll() // Get all healthcare workers
    {
        var healthcareWorkers = await _repository.GetAll(); // Get all healthcare workers from repository
        if (healthcareWorkers == null || !healthcareWorkers.Any())
        {
            _logger.LogWarning("[HealthcareWorkerService] No healthcare workers found.");
            return Enumerable.Empty<HealthcareWorkerDto>();
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
    public async Task<HealthcareWorkerDto?> GetById(int id) // Get healthcare worker by Id
    {
        var worker = await _repository.GetById(id); // Get healthcare worker from repository
        if (worker == null)
        {
            _logger.LogError("[HealthcareWorkerService] Healthcare worker not found for Id {Id:0000}", id);
            return null;
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
    public async Task<HealthcareWorkerDto?> GetByAuthUserId(string authUserId) // Get healthcare worker by AuthUserId
    {
        var worker = await _repository.GetByAuthUserId(authUserId); // Get healthcare worker from repository
        if (worker == null)
        {
            _logger.LogError("[HealthcareWorkerService] Healthcare worker not found for AuthUserId {AuthUserId}", authUserId);
            return null;
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
    public async Task<HealthcareWorkerDto> Create(HealthcareWorkerDto workerDto) // Create new healthcare worker
    {
        var worker = new HealthcareWorker // Map HealthcareWorkerDto to HealthcareWorker
        {
            Name = workerDto.Name,
            Address = workerDto.Address,
            Phone = workerDto.Phone,
            Email = workerDto.Email,
            AuthUserId = workerDto.AuthUserId
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
    public async Task<bool> Update(UpdateUserDto userDto) // Update healthcare worker
    {
        var id = userDto.Id; 
        var existingWorker = await _repository.GetById(id); // Get existing healthcare worker from repository
        if (existingWorker == null)
        {
            return false;
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
    public async Task<bool> Delete(int id) // Delete healthcare worker by Id
    {
         var worker = await _repository.GetById(id); // Get existing healthcare worker from repository
        if (worker == null)
        {
            return false;
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