using System.ComponentModel.DataAnnotations;

namespace JobTrackr.Application.Tasks
{
    public class CreateTaskRequest
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public DateTime? DueDateUtc { get; set; }

        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = "Medium";

        [Range(1, int.MaxValue)]
        public int UserId { get; set; }
    }
}
