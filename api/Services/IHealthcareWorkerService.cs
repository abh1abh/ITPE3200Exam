
using api.DTO;

namespace api.Services;
public interface IHealthcareWorkerService
{
    Task<IEnumerable<HealthcareWorkerDto>> GetAll();
    Task<HealthcareWorkerDto?> GetById(int id);
    Task<HealthcareWorkerDto> Create(HealthcareWorkerDto workerDto);
    Task<bool> Update(int id, HealthcareWorkerDto workerDto);
    Task<bool> Delete(int id);

}