
using api.DTO;

namespace api.Services;
public interface IHealthcareWorkerService
{
    Task<IEnumerable<HealthcareWorkerDto>> GetAll();
    Task<HealthcareWorkerDto?> GetById(int id);
    Task<HealthcareWorkerDto> Create(HealthcareWorkerDto workerDto);
    Task<bool> Update(UpdateUserDto userDto);
    Task<bool> Delete(int id);
    Task<HealthcareWorkerDto?> GetByAuthUserId(string authUserId);
}