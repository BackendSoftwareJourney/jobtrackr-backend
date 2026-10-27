namespace JobTrackr.Application.Comments
{
    public class TaskCommentResponse
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }
    }
}