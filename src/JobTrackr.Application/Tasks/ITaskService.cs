namespace JobTrackr.Application.Tasks
{
    public interface ITaskService
    {
        Task<List<TaskResponse>> GetAll(bool? isCompleted, string? search, int? userId);

        Task<TaskResponse?> GetById(int id);

        Task<TaskResponse> CreateTask(CreateTaskRequest request);

        Task<TaskResponse?> UpdateTask(int id, UpdateTaskRequest request);

        Task<bool> DeleteTask(int id);

        Task<TaskResponse?> CompleteTask(int id);

        Task<TaskResponse?> ReopenTask(int id);

        Task<List<TaskResponse>?> GetByUserId(int userId);
    }
}
