using System;
using System.ComponentModel.DataAnnotations;

namespace HomecareAppointmentManagement.DTO
{
    public class AvailableSlotDto
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Healthcare worker is required.")]
        public int HealthcareWorkerId { get; set; }
        
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Start { get; set; }
        
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime End { get; set; }
        public bool IsBooked { get; set; }
    }
}
