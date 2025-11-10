

using api.DAL;
using api.DTO;
using api.Models;

namespace api.Services;

public class HealthcareWorkerService: IHealthcareWorkerService
{
    private readonly IHealthcareWorkerRepository _repository;
    private readonly ILogger<HealthcareWorkerService> _logger;
    private readonly IAuthService _authService;

    public HealthcareWorkerService(IHealthcareWorkerRepository repository, ILogger<HealthcareWorkerService> logger)
    {
        _repository = repository;
        _logger = logger;
        _authService = _authService;
    }
    
    public async Task<IEnumerable<HealthcareWorkerDto>> GetAll()
    {
        var healthcareWorkers = await _repository.GetAll();
        if (healthcareWorkers == null || !healthcareWorkers.Any())
        {
            _logger.LogWarning("[HealthcareWorkerService] No healthcare workers found.");
            return Enumerable.Empty<HealthcareWorkerDto>();
        }

        var workerDtos = healthcareWorkers.Select(w => new HealthcareWorkerDto
        {
            HealthcareWorkerId = w.HealthcareWorkerId,
            Name = w.Name,
            Address = w.Address,
            Phone = w.Phone,
            Email = w.Email,
            AuthUserId = w.AuthUserId
        });

        return workerDtos;
    }
    public async Task<HealthcareWorkerDto?> GetById(int id)
    {
        var worker = await _repository.GetById(id);
        if (worker == null)
        {
            _logger.LogError("[HealthcareWorkerService] Healthcare worker not found for HealthcareWorkerId {HealthcareWorkerId:0000}", id);
            return null;
        }

        var workerDto = new HealthcareWorkerDto
        {
            HealthcareWorkerId = worker.HealthcareWorkerId,
            Name = worker.Name,
            Address = worker.Address,
            Phone = worker.Phone,
            Email = worker.Email,
            AuthUserId = worker.AuthUserId
        };

        return workerDto;
    }
    public async Task<HealthcareWorkerDto?> GetByAuthUserId(string authUserId)
    {
        var worker = await _repository.GetByAuthUserId(authUserId);
        if (worker == null)
        {
            _logger.LogError("[HealthcareWorkerService] Healthcare worker not found for AuthUserId {AuthUserId}", authUserId);
            return null;
        }

        var workerDto = new HealthcareWorkerDto
        {
            HealthcareWorkerId = worker.HealthcareWorkerId,
            Name = worker.Name,
            Address = worker.Address,
            Phone = worker.Phone,
            Email = worker.Email,
            AuthUserId = worker.AuthUserId
        };

        return workerDto;
    }
    public async Task<HealthcareWorkerDto> Create(HealthcareWorkerDto workerDto)
    {
        var worker = new HealthcareWorker
        {
            Name = workerDto.Name,
            Address = workerDto.Address,
            Phone = workerDto.Phone,
            Email = workerDto.Email,
            AuthUserId = workerDto.AuthUserId
        };

        bool created = await _repository.Create(worker);
        if (!created)
        {
            _logger.LogError("[HealthcareWorkerService] Healthcare worker creation failed {@worker}", worker);
            throw new InvalidOperationException("Create failed");

        }

        var createdDto = new HealthcareWorkerDto
        {
            HealthcareWorkerId = worker.HealthcareWorkerId,
            Name = worker.Name,
            Address = worker.Address,
            Phone = worker.Phone,
            Email = worker.Email,
            AuthUserId = worker.AuthUserId
        };

        return createdDto;
    }
    public async Task<bool> Update(int id, UpdateWorkerDto workerDto)
    {
        var existingWorker = await _repository.GetById(id);
        if (existingWorker == null)
        {
            return false;
        }

        existingWorker.Name = workerDto.Name;
        existingWorker.Address = workerDto.Address;
        existingWorker.Phone = workerDto.Phone;
        existingWorker.Email = workerDto.Email;

        bool updated = await _repository.Update(existingWorker);
        if (!updated)
        {
            _logger.LogError("[HealthcareWorkerService] Update failed for HealthcareWorkerId {HealthcareWorkerId:0000}, {@worker}", id, existingWorker);
            throw new InvalidOperationException($"Update operation failed for HealthcareWorkerId {id}");
        }
        return updated;
    }
    public async Task<bool> Delete(int id)
    {
         var worker = await _repository.GetById(id);
        if (worker == null)
        {
            return false;
        }

        bool deleted = await _repository.Delete(id);
        if (!deleted)
        {
            _logger.LogError("[HealthcareWorkerService] Deletion failed for HealthcareWorkerId {HealthcareWorkerId:0000}", id);
            throw new InvalidOperationException($"Deletion operation failed for HealthcareWorkerId {id}");
        }
        return deleted;
    }
}