using System.ComponentModel.DataAnnotations;

namespace JobTrackr.Application.Users
{
    public class UpdateUserRequest
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;
    }
}
