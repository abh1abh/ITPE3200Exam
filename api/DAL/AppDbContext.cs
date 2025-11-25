
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; } // Clients table
        public DbSet<HealthcareWorker> HealthcareWorkers { get; set; } // Healthcare Workers table
        public DbSet<Appointment> Appointments { get; set; } // Appointments table
        public DbSet<AppointmentTask> AppointmentTasks { get; set; } // Appointment Tasks table
        public DbSet<AvailableSlot> AvailableSlots { get; set; } // Available Slots table

        public DbSet<ChangeLog> ChangeLogs { get; set; } // Change Logs table

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
        {
            optionsBuilder.UseLazyLoadingProxies(); // Enable lazy loading
        }

         protected override void OnModelCreating(ModelBuilder modelBuilder) // Customize model creation 
        {
            base.OnModelCreating(modelBuilder); // Keep Identity mappings

            // Keep ChangeLogs even when the Appointment row is deleted (suggestion from ChatGPT)
            modelBuilder.Entity<Appointment>()
                .HasMany(a => a.ChangeLogs)
                .WithOne(cl => cl.Appointment)
                .HasForeignKey(cl => cl.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);
        
        }
    }
}