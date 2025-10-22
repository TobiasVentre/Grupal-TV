using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class LoginRequest
    {
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }
    }
}
