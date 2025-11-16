
using api.DTO;

namespace api.Services;
public interface IHealthcareWorkerService
{
    Task<IEnumerable<HealthcareWorkerDto>> GetAll(bool isAdmin);
    Task<HealthcareWorkerDto?> GetById(int id, string authUserId, string role);
    Task<HealthcareWorkerDto> Create(RegisterDto dto, string authId, bool isAdmin);
    Task<bool> Update(UpdateUserDto userDto, string authId, string role);
    Task<bool> Delete(int id, string authId, string role);
    Task<HealthcareWorkerDto?> GetByAuthUserId(string authUserId, string authId, string role);
}