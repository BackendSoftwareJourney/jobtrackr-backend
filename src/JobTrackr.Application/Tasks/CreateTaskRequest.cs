namespace JobTrackr.Application.Tasks
{
    public class CreateTaskRequest
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int UserId { get; set; }
    }
}
