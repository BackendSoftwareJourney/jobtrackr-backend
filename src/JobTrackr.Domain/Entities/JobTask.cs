namespace JobTrackr.Domain.Entities
{
    public class JobTask
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsCompleted { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? DueDateUtc { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }
    }
}
