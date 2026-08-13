using System.ComponentModel.DataAnnotations;

namespace JobTrackr.Application.Tasks
{
    public class TaskQueryRequest
    {
        public bool? IsCompleted { get; set; }

        public string? Search { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1.")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
        public int PageSize { get; set; } = 10;
    }
}
