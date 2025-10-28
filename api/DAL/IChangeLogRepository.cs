using HomecareAppointmentManagement.Models;

namespace HomecareAppointmentManagement.DAL;

public interface IChangeLogRepository
{
    Task<bool> Create(ChangeLog changeLog);
    Task<IEnumerable<ChangeLog>?> GetByAppointmentId(int appointmentId);
}