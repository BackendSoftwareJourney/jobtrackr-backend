using System.ComponentModel.DataAnnotations;

namespace JobTrackr.Application.Auth
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Current password is required.")]
        [MaxLength(
            100,
            ErrorMessage = "Current password cannot be longer than 100 characters.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters.")]
        [MaxLength(
            100,
            ErrorMessage = "New password cannot be longer than 100 characters.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password confirmation is required.")]
        [Compare(
            nameof(NewPassword),
            ErrorMessage = "New password and confirmation do not match.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}