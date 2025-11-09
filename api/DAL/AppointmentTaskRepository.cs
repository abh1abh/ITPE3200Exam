using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.DAL;

public class AppointmentTaskRepository : IAppointmentTaskRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<AppointmentTaskRepository> _logger; 

    public AppointmentTaskRepository(AppDbContext db, ILogger<AppointmentTaskRepository> logger) 
    {
        _db = db;
        _logger = logger; 
    }

    public async Task<bool> Create(AppointmentTask appointmentTask) 
    {
        try
        {
            await _db.AppointmentTasks.AddAsync(appointmentTask); // Add the appointment task
            await _db.SaveChangesAsync(); // Save changes to the database
            return true; // Return true on success
        }
        catch (Exception e)
        {
            _logger.LogError("[AppointmentTaskRepository] appointment task AddAsync() failed when Create(), error messager: {e}", e.Message);
            return false; // Return false on failure
        }
    }

    public async Task<bool> Update(AppointmentTask appointmentTask)
    {
        try
        {
            _db.AppointmentTasks.Update(appointmentTask); // Update the appointment task
            await _db.SaveChangesAsync(); // Save changes to the database
            return true; // Return true on success
        }
        catch (Exception e)
        {
            _logger.LogError("[AppointmentTaskRepository] appointment task Update() failed when Update() for AppointmentTaskId {AppointmentTaskId:0000}, error messager: {e}", appointmentTask.Id, e.Message);
            return false; // Return false on failure
        }
    }
    
    public async Task<bool> Delete(int id)
    {
        try
        {
            var appointmentTask = await _db.AppointmentTasks.FindAsync(id);
            if (appointmentTask == null) return false; 

            _db.AppointmentTasks.Remove(appointmentTask); // Remove the client
            await _db.SaveChangesAsync(); // Save changes to the database
            return true; // Return true on success
        }
        catch (Exception e)
        {
            _logger.LogError("[AppointmentTaskRepository] appointment task Delete() failed when Remove() for AppointmentTaskId {AppointmentTaskId:0000}, error messager: {e}", id, e.Message);
            return false; // Return false on failure
        }
    }
    
}