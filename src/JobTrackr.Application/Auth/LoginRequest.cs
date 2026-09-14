using System.ComponentModel.DataAnnotations;

namespace JobTrackr.Application.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email is not valid.")]
        [MaxLength(150, ErrorMessage = "Email cannot be longer than 150 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MaxLength(100, ErrorMessage = "Password cannot be longer than 100 characters.")]
        public string Password { get; set; } = string.Empty;
    }
}
