
using HomecareAppointmentManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HomecareAppointmentManagement.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            // Database.EnsureCreated();
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

            // Keep ChangeLogs even when the Appointment row is deleted (suggestion from ChatGPT, might move to soft deleting later)
            modelBuilder.Entity<ChangeLog>()
                .HasOne(c => c.Appointment)
                .WithMany()
                .HasForeignKey(c => c.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull); // Set FK to null on Appointment deletion

            modelBuilder.Entity<Appointment>()
               .HasMany(a => a.ChangeLogs)
               .WithOne(cl => cl.Appointment)
               .HasForeignKey(cl => cl.AppointmentId)
               .OnDelete(DeleteBehavior.Cascade); // or Restrict
        
            // Male sure AuthUserId is unique  
            modelBuilder.Entity<HealthcareWorker>()
                .HasIndex(x => x.AuthUserId)
                .IsUnique();
        }
    }
}