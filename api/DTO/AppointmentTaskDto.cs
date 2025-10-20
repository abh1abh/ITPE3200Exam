

namespace HomecareAppointmentManagement.DTO
{
    public record AppointmentTaskDto
    {
        public int Id { get; init; }
        public int AppointmentId { get; init; }
        public string Description { get; init; } = "";
        public bool IsCompleted { get; init; }
    }
}