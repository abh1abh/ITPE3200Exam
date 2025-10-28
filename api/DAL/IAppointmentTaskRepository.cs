using HomecareAppointmentManagement.Models;

namespace HomecareAppointmentManagement.DAL;

public interface IAppointmentTaskRepository
{
    Task<bool> Create(AppointmentTask appointmentTask);
    Task<bool> Update(AppointmentTask appointmentTask);
}