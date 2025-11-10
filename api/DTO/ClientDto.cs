using System.ComponentModel.DataAnnotations;

namespace api.DTO
{
    public class ClientDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Name must be at most {1} characters.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string? Address { get; set; }

        [Required]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string? Phone { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string? Email { get; set; }
        public string? AuthUserId { get; set; }
    }
}
