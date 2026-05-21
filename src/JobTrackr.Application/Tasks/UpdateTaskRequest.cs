namespace JobTrackr.Application.Tasks
{
    public class UpdateTaskRequest
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
