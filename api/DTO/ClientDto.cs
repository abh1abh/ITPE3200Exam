namespace HomecareAppointmentManagement.DTO
{
    public class ClientDto
    {
        public int ClientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? AuthUserId { get; set; }
    }
}
