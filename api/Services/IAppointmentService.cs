
using api.DTO;

namespace api.Services;
public interface IAppointmentService
{
    Task<IEnumerable<AppointmentViewDto>> GetAll();
    Task<IEnumerable<AppointmentViewDto>> GetAppointmentsByClientId(string? authUserId);
    Task<IEnumerable<AppointmentViewDto>> GetAppointmentsByHealthcareWorkerId(string? authUserId);
    Task<AppointmentViewDto?> GetById(int id, string? role, string? authUserId);
    Task<AppointmentDto> Create(AppointmentDto dto, string? role, string? authUserId);
    Task<bool> Update(int id, AppointmentDto dto, string? role, string? authUserId);
    Task<bool> Delete(int id, string? role, string? authUserId);
    Task<IEnumerable<ChangeLogDto>> GetChangeLog(int id, string? role, string? authUserId);

}