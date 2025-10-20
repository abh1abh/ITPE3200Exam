using HomecareAppointmentManagement.Models;
using HomecareAppointmentManagement.ViewModels;


namespace HomecareAppointentManagement.ViewModels;


public class AppointmentDetailsViewModel
{
    public AppointmentViewModel ViewMode { get; set; }
    public Appointment Appointment { get; set; } = new Appointment();

}
