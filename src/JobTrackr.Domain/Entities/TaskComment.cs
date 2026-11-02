namespace JobTrackr.Domain.Entities
{
    public class TaskComment
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public int TaskId { get; set; }

        public JobTask? Task { get; set; }
    }
}
