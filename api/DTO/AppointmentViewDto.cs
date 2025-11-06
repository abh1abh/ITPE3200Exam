
// Dto for viewing an appointment with client and worker name 
namespace api.DTO
{
    public class AppointmentViewDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = "";               
        public int HealthcareWorkerId { get; set; }
        public string HealthcareWorkerName { get; set; } = "";     
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int? AvailableSlotId { get; set; }
        public List<AppointmentTaskDto>? AppointmentTasks { get; set; }
        public List<ChangeLogDto>? ChangeLogs { get; set; }
       
    }
}