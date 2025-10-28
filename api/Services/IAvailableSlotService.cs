
using HomecareAppointmentManagement.DTO;

namespace HomecareAppointmentManagement.Services;
public interface IAvailableSlotService
{
    Task<IEnumerable<AvailableSlotDto>> GetAll(bool IsAdmin, string? authUserId);
    Task<AvailableSlotDto?> GetById(int id, bool isAdmin, string? authUserId);
    Task<AvailableSlotDto> Create(AvailableSlotDto dto, bool isAdmin, string? authUserId);
    Task<bool> Update(int id, AvailableSlotDto dto, bool isAdmin, string? authUserId);
    Task<bool> Delete(int id, bool isAdmin, string? authUserId);

}