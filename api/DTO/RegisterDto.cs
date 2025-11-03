using System.ComponentModel.DataAnnotations;

namespace api.DTO
{
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between {2} and {1} characters.")]
        public string Password { get; set; } = string.Empty;
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; } = string.Empty;
        [Required]
        [StringLength(200, ErrorMessage = "Name must be at most {1} characters.")]
        public string Name { get; set; } = string.Empty;
        [Required]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string Number { get; set; } = string.Empty;
        [Required]
        [StringLength(200, ErrorMessage = "Address must be at most {1} characters.")]
        public string Address { get; set; } = string.Empty;

    }
}