using api.Models;

namespace api.DAL;

public interface IAppointmentTaskRepository
{
    Task<bool> Create(AppointmentTask appointmentTask);
    Task<bool> Update(AppointmentTask appointmentTask);
}