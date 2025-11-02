using System.ComponentModel.DataAnnotations;

namespace api.DTO
{
    public class RegisterFromAdminDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Number { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;
    }
}
