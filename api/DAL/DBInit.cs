using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.DAL;

public static class DBInit
{

    public static async Task SeedAsync(IServiceProvider sp, SeedResult authSeed) // Seed method to initialize the database
    {
        var appDbContext = sp.GetRequiredService<AppDbContext>(); // Get the database context

        // Init of Clients with IdentityUser link and claim
        if (await appDbContext.Clients.AnyAsync()) return;


        var client = new Client { 
            Name="John Doe",  
            Address="123 Main St",  
            Phone="555-1234", 
            Email="client@homecare.local",
            AuthUserId = authSeed.UserIds["Client"] 
        };

        appDbContext.Clients.Add(client);
        await appDbContext.SaveChangesAsync();

        // Init of HealthcareWorker with IdentityUser link and claim
        var worker = new HealthcareWorker {
            Name="Alice Brown",
            Address="12 Health St",
            Phone="555-1111",
            Email="worker@homecare.local",
            AuthUserId=authSeed.UserIds["HealthcareWorker"] 
        };

        appDbContext.HealthcareWorkers.AddRange(worker);
        await appDbContext.SaveChangesAsync();


        // Init Appointments, AvailableSlots, ChangeLogs
        // Create slots
        var slots = new List<AvailableSlot>
        {
            new AvailableSlot
            {
                HealthcareWorkerId = worker.Id,
                Start = new DateTime(2025, 12, 10, 14, 0, 0), // 10.12.2025, 14:00
                End   = new DateTime(2025, 12, 10, 15, 0, 0), // 10.12.2025, 15:00
                IsBooked = false
            },
            new AvailableSlot
            {
                HealthcareWorkerId = worker.Id,
                Start = new DateTime(2025, 11, 24, 14, 0, 0), // 24.11.2025, 14:00
                End   = new DateTime(2025, 11, 24, 15, 0, 0), // 24.11.2025, 15:00
                IsBooked = false // set false for now. Flip after linking
            },
            new AvailableSlot
            {
                HealthcareWorkerId = worker.Id,
                Start = new DateTime(2025, 11, 25, 11, 0, 0), // 25.11.2025, 11:00
                End   = new DateTime(2025, 11, 25, 12, 0, 0), // 25.11.2025, 12:00
                IsBooked = false // set false for now. Flip after linking
            }
        };
        appDbContext.AvailableSlots.AddRange(slots); // Add slots to context
        await appDbContext.SaveChangesAsync(); // Save to get IDs

        // Create appointments and link both
        var appts = new List<Appointment>
        {
            new Appointment
            {
                ClientId = client.Id,
                HealthcareWorkerId = worker.Id,
                Start = slots[2].Start,
                End   = slots[2].End,
                Notes = "Assistance with mobility exercises",
                AvailableSlot = slots[2],
                AppointmentTasks = new List<AppointmentTask>
                {
                    new AppointmentTask { Description = "Help with walking exercises", IsCompleted = true },
                    new AppointmentTask { Description = "Physiotherapy session", IsCompleted = true }
                }
            },

            new Appointment
            {
                ClientId = client.Id,
                HealthcareWorkerId = worker.Id,
                Start = slots[1].Start,
                End   = slots[1].End,
                Notes = "Assistance with mobility exercises",
                AvailableSlot = slots[1],
                AppointmentTasks = new List<AppointmentTask>
                {
                    new AppointmentTask { Description = "Help with walking exercises", IsCompleted = true },
                    new AppointmentTask { Description = "Clean the room", IsCompleted = false }
                }
                
            }
        };
        appDbContext.Appointments.AddRange(appts); // Add appointments to context
        await appDbContext.SaveChangesAsync(); // Save to get IDs

        // Mark the linked slots as booked 
        slots[0].IsBooked = false;
        slots[1].IsBooked = true;
        slots[2].IsBooked = true;
        await appDbContext.SaveChangesAsync();

        
        // Change logs for appointments
        var changeLogs = new List<ChangeLog>
        {
            new ChangeLog { 
                AppointmentId = appts[0].Id, 
                AppointmentIdSnapshot= appts[0].Id,
                ChangeDate = DateTime.Now, 
                ChangedByUserId = worker.AuthUserId!, 
                ChangeDescription = "Task 'Help with walking exercises' completed." 
            },
            new ChangeLog { 
                AppointmentId = appts[0].Id, 
                AppointmentIdSnapshot= appts[0].Id,
                ChangeDate = DateTime.Now, 
                ChangedByUserId = worker.AuthUserId!, 
                ChangeDescription = "Task 'Physiotherapy session' completed." 
            },
            new ChangeLog { 
                AppointmentId = appts[1].Id, 
                AppointmentIdSnapshot= appts[1].Id,
                ChangeDate = DateTime.Now, 
                ChangedByUserId = worker.AuthUserId!, 
                ChangeDescription = "Changed time of appointment to 24.11.2025, 14:00." 
            },
               new ChangeLog { 
                AppointmentId = appts[1].Id, 
                AppointmentIdSnapshot= appts[1].Id,
                ChangeDate = DateTime.Now, 
                ChangedByUserId = worker.AuthUserId!, 
                ChangeDescription = "Task 'Help with walking exercises' completed." 
            }
        };
        
        appDbContext.ChangeLogs.AddRange(changeLogs);
        
        // Final save
        await appDbContext.SaveChangesAsync();


    }
}
