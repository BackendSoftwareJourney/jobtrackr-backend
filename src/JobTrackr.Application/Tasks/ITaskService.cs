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

        Task<TaskResponse> CreateAsync(CreateTaskRequest request, int userId);

        Task<TaskResponse?> UpdateAsync(int id, UpdateTaskRequest request, int userId);

        Task<bool> DeleteAsync(int id, int userId);

        Task<TaskResponse?> CompleteAsync(int id, int userId);

        Task<TaskResponse?> ReopenAsync(int id, int userId);

        Task<List<TaskResponse>?> GetByUserIdAsync(int userId);
    }
}
