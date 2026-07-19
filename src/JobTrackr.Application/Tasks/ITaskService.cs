namespace JobTrackr.Application.Tasks
{
    public interface ITaskService
    {
        Task<List<TaskResponse>> GetAllAsync(bool? isCompleted, string? search, int userId);

        Task<TaskResponse?> GetByIdAsync(int id, int userId);

        Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, int userId);

        Task<TaskResponse?> UpdateTaskAsync(int id, UpdateTaskRequest request, int userId);

        Task<bool> DeleteTaskAsync(int id, int userId);

        Task<TaskResponse?> CompleteTaskAsync(int id);

        Task<TaskResponse?> ReopenTaskAsync(int id);

        Task<List<TaskResponse>?> GetByUserIdAsync(int userId);
    }
}
