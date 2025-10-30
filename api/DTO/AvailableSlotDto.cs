using System;
using System.ComponentModel.DataAnnotations;

namespace api.DTO
{
    public class AvailableSlotDto
    {
        public int Id { get; set; }

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
