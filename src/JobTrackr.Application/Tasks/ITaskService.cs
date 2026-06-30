namespace JobTrackr.Application.Tasks
{
    public interface ITaskService
    {
        Task<List<TaskResponse>> GetAllAsync(bool? isCompleted, string? search, int? userId);

        Task<TaskResponse?> GetByIdAsync(int id);

        Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request);

        Task<TaskResponse?> UpdateTaskAsync(int id, UpdateTaskRequest request);

        Task<bool> DeleteTaskAsync(int id);

        Task<TaskResponse?> CompleteTaskAsync(int id);

        Task<TaskResponse?> ReopenTaskAsync(int id);

        Task<List<TaskResponse>?> GetByUserIdAsync(int userId);
    }
}
