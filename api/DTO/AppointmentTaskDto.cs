

using System.ComponentModel.DataAnnotations;

namespace HomecareAppointmentManagement.DTO
{
    public record AppointmentTaskDto
    {
        public int Id { get; init; }
        
        public int AppointmentId { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Description must be at most {1} characters.")]    
        public string Description { get; set; } = string.Empty;

        public bool IsCompleted { get; init; }
    }
}