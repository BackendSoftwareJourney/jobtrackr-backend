using JobTrackr.Application.Common;
using JobTrackr.Application.Tasks;
using JobTrackr.Domain.Entities;
using JobTrackr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Infrastructure.Tasks
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _dbContext;

        public TaskService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException(ErrorMessages.UserIdRequired);
            }

            var userExists = await _dbContext.Users.AnyAsync(user => user.Id == userId);

            if (!userExists)
            {
                throw new ArgumentException(ErrorMessages.UserNotFound);
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException(ErrorMessages.TaskTitleRequired);
            }

            if (string.IsNullOrWhiteSpace(request.Priority))
            {
                throw new ArgumentException(ErrorMessages.TaskPriorityRequired);
            }

            var task = new JobTask()
            {
                Description = request.Description,
                Title = request.Title,
                CreatedAtUtc = DateTime.UtcNow,
                DueDateUtc = request.DueDateUtc,
                Priority = request.Priority,
                IsCompleted = false,
                UserId = userId
            };

            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync();

            return MapToResponse(task);
        }

        public async Task<PagedResponse<TaskResponse>> GetAllAsync(
            bool? isCompleted,
            string? search,
            string sortBy,
            string sortDirection,
            int pageNumber,
            int pageSize,
            int userId)
        {
            var query = _dbContext.Tasks.Where(task => task.UserId == userId);

            if (isCompleted is not null)
            {
                query = query.Where(task => task.IsCompleted == isCompleted);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(task => task.Title.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var tasksToSkip = (pageNumber - 1) * pageSize;
            var orderedQuery = ApplySorting(query, sortBy, sortDirection);

            var taskResponses = await orderedQuery
                .Skip(tasksToSkip)
                .Take(pageSize)
                .Select(task => new TaskResponse
                {
                    Id = task.Id,
                    Description = task.Description,
                    Title = task.Title,
                    CreatedAtUtc = task.CreatedAtUtc,
                    DueDateUtc = task.DueDateUtc,
                    Priority = task.Priority,
                    IsCompleted = task.IsCompleted,
                    UserId = task.UserId
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResponse<TaskResponse>
            {
                Items = taskResponses,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<TaskResponse?> GetByIdAsync(int id, int userId)
        {
            var task = await _dbContext.Tasks
                .FirstOrDefaultAsync(task => task.Id == id && task.UserId == userId);

            if (task is null)
            {
                return null;
            }

            return MapToResponse(task);
        }

        public async Task<TaskResponse?> UpdateAsync(int id, UpdateTaskRequest request, int userId)
        {
            var task = await _dbContext.Tasks
                .FirstOrDefaultAsync(task => task.Id == id && task.UserId == userId);

            if (task is null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException(ErrorMessages.TaskTitleRequired);
            }

            if (string.IsNullOrWhiteSpace(request.Priority))
            {
                throw new ArgumentException(ErrorMessages.TaskPriorityRequired);
            }

            task.Title = request.Title;
            task.Description = request.Description;
            task.DueDateUtc = request.DueDateUtc;
            task.Priority = request.Priority;

            await _dbContext.SaveChangesAsync();

            return MapToResponse(task);
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var task = await _dbContext.Tasks
                .FirstOrDefaultAsync(task => task.Id == id && task.UserId == userId);

            if (task is null)
            {
                return false;
            }

            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<TaskResponse?> CompleteAsync(int id, int userId)
        {
            var task = await _dbContext.Tasks
                .FirstOrDefaultAsync(task => task.Id == id && task.UserId == userId);

            if (task is null)
            {
                return null;
            }

            task.IsCompleted = true;

            await _dbContext.SaveChangesAsync();

            return MapToResponse(task);
        }

        public async Task<TaskResponse?> ReopenAsync(int id, int userId)
        {
            var task = await _dbContext.Tasks
                .FirstOrDefaultAsync(task => task.Id == id && task.UserId == userId);

            if (task is null)
            {
                return null;
            }

            task.IsCompleted = false;

            await _dbContext.SaveChangesAsync();

            return MapToResponse(task);
        }

        private static IOrderedQueryable<JobTask> ApplySorting(
            IQueryable<JobTask> query,
            string sortBy,
            string sortDirection)
        {
            var isAscending = string.Equals(
                sortDirection,
                "asc",
                StringComparison.OrdinalIgnoreCase);

            if (string.Equals(sortBy, "dueDate", StringComparison.OrdinalIgnoreCase))
            {
                return isAscending
                    ? query
                        .OrderBy(task => task.DueDateUtc == null)
                        .ThenBy(task => task.DueDateUtc)
                        .ThenBy(task => task.Id)
                    : query
                        .OrderBy(task => task.DueDateUtc == null)
                        .ThenByDescending(task => task.DueDateUtc)
                        .ThenByDescending(task => task.Id);
            }

            return isAscending
                ? query
                    .OrderBy(task => task.CreatedAtUtc)
                    .ThenBy(task => task.Id)
                : query
                    .OrderByDescending(task => task.CreatedAtUtc)
                    .ThenByDescending(task => task.Id);
        }

        private static TaskResponse MapToResponse(JobTask task)
        {
            return new TaskResponse
            {
                Id = task.Id,
                Description = task.Description,
                Title = task.Title,
                CreatedAtUtc = task.CreatedAtUtc,
                DueDateUtc = task.DueDateUtc,
                Priority = task.Priority,
                IsCompleted = task.IsCompleted,
                UserId = task.UserId
            };
        }

        public async Task<List<TaskResponse>?> GetByUserIdAsync(int userId)
        {
            var userExists = await _dbContext.Users.AnyAsync(user => user.Id == userId);

            if (!userExists)
            {
                return null;
            }

            return await _dbContext.Tasks
                .Where(task => task.UserId == userId)
                .Select(task => MapToResponse(task))
                .ToListAsync();
        }
    }
}
