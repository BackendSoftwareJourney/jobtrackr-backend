using JobTrackr.Application.Common;

namespace JobTrackr.Application.Tasks
{
    public interface ITaskService
    {
        Task<PagedResponse<TaskResponse>> GetAllAsync(
            bool? isCompleted,
            string? search,
            string sortBy,
            string sortDirection,
            int pageNumber,
            int pageSize,
            int userId);

        Task<TaskResponse?> GetByIdAsync(int id, int userId);

        Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, int userId);

        Task<TaskResponse?> UpdateTaskAsync(int id, UpdateTaskRequest request, int userId);

        Task<bool> DeleteTaskAsync(int id, int userId);

        Task<TaskResponse?> CompleteTaskAsync(int id, int userId);

        Task<TaskResponse?> ReopenTaskAsync(int id, int userId);

        Task<List<TaskResponse>?> GetByUserIdAsync(int userId);
    }
}
