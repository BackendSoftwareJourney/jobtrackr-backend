namespace JobTrackr.Application.Comments
{
    public interface ITaskCommentService
    {
        Task<TaskCommentResponse?> CreateAsync(
            int taskId,
            CreateTaskCommentRequest request,
            int userId);

        Task<List<TaskCommentResponse>?> GetAllAsync(int taskId, int userId);

        Task<bool> DeleteAsync(int taskId, int commentId, int userId);
    }
}