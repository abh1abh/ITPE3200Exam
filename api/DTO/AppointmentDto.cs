
using System.ComponentModel.DataAnnotations;
using HomecareAppointmentManagment.DTO;
using HomecareAppointmentManagment.Models;

namespace HomecareAppointmentManagement.DTO
{
    public class AppointmentDto 
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Client is required.")]
        public int ClientId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Healthcare worker is required.")]
        public int HealthcareWorkerId { get; set; }

        [Display(Name = "Start time")]
        [DataType(DataType.DateTime)]
        [Required]
        public DateTime Start { get; set; }

        [Display(Name = "End time")]
        [DataType(DataType.DateTime)]
        [Required]
        public DateTime End { get; set; }

        [StringLength(1000, ErrorMessage = "Notes must be at most {1} characters.")]
        public string Notes { get; set; } = string.Empty;

        public int? AvailableSlotId { get; set; } // Foreign key to AvailableSlot

        public List<AppointmentTaskDto>? AppointmentTasks { get; set; }

        public List<ChangeLogDto>? ChangeLogs { get; set; }

       
    }
}