namespace JobTrackr.Application.Comments
{
    public interface ITaskCommentService
    {
        Task<TaskCommentResponse?> CreateAsync(
            int taskId,
            CreateTaskCommentRequest request,
            int userId);
    }
}