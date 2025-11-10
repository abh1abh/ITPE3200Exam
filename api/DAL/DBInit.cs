using System.Security.Claims;
using api.Models;
// using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.DAL;

public static class DBInit
{

    public static async Task SeedAsync(IServiceProvider sp, SeedResult authSeed) // Seed method to initialize the database
    {
        var appDbContext = sp.GetRequiredService<AppDbContext>(); // Get the database context

        // For dev, resets db each time
        // context.Database.EnsureDeleted(); 
        // context.Database.EnsureCreated();

        if (await appDbContext.Clients.AnyAsync()) return;


        var clients = new[]
        {
            new Client { Name="John Doe",  Address="123 Main St",  Phone="555-1234", Email="john@homecare.local", AuthUserId = authSeed.UserIds["Client"]  }
        };

        appDbContext.Clients.AddRange(clients);
        await appDbContext.SaveChangesAsync();


        // Init of HealthcareWorkers with IdentityUser link and claim
        var workers= new[]
        {
            new HealthcareWorker {
                Name="Alice Brown",
                Address="12 Health St",
                Phone="555-1111",
                Email="alice@homecare.local",
                AuthUserId=authSeed.UserIds["HealthcareWorker"] },
        };

        appDbContext.HealthcareWorkers.AddRange(workers);
        await appDbContext.SaveChangesAsync();


        // Init Appointments, AvailableSlots, ChangeLogs
        // Create slots
        var slots = new List<AvailableSlot>
        {
            new AvailableSlot
            {
                HealthcareWorkerId = workers[0].Id,
                Start = new DateTime(2025, 12, 9, 14, 0, 0), // Dec 9, 2025 at 14:00
                End   = new DateTime(2025, 12, 9, 15, 0, 0), // Dec 9, 2025 at 15:00
                IsBooked = false
            },
            new AvailableSlot
            {
                HealthcareWorkerId = workers[0].Id,
                Start = new DateTime(2025, 12, 10, 14, 0, 0), // Dec 10, 2025 at 14:00
                End   = new DateTime(2025, 12, 10, 15, 0, 0), // Dec 10, 2025 at 15:00
                IsBooked = false // set false for now; will flip after linking
            }
        };
        appDbContext.AvailableSlots.AddRange(slots); // Add slots to context
        await appDbContext.SaveChangesAsync(); // Save to get IDs

        // Create appointments and link both
        var appts = new List<Appointment>
        {
            new Appointment
            {
                ClientId = clients[0].Id,
                HealthcareWorkerId = workers[0].Id,
                Start = slots[1].Start,
                End   = slots[1].End,
                Notes = "Assistance with mobility exercises",
                AvailableSlot = slots[1],
                AppointmentTasks = new List<AppointmentTask>
                {
                    new AppointmentTask { Description = "Help with walking exercises" }
                }
            }
        };
        appDbContext.Appointments.AddRange(appts); // Add appointments to context
        await appDbContext.SaveChangesAsync(); // Save to get IDs

        // Mark the linked slots as booked (optional convenience flag)
        slots[0].IsBooked = false;
        slots[1].IsBooked = true;
        await appDbContext.SaveChangesAsync();

        // Change logs for appointments
        appDbContext.ChangeLogs.AddRange(
            new ChangeLog { AppointmentId = appts[0].Id, AppointmentIdSnapshot= appts[0].Id, ChangeDate = DateTime.Now, ChangedByUserId = workers[0].AuthUserId!, ChangeDescription = "Rescheduled due to patient request" }
        );
        await appDbContext.SaveChangesAsync();


    }
}
