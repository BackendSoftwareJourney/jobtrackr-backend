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

        public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, int userId)
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

        public async Task<List<TaskResponse>> GetAllAsync(bool? isCompleted, string? search, int userId)
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

            return await query
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

        public async Task<TaskResponse?> UpdateTaskAsync(int id, UpdateTaskRequest request, int userId)
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

        public async Task<bool> DeleteTaskAsync(int id, int userId)
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

        public async Task<TaskResponse?> CompleteTaskAsync(int id)
        {
            var task = await _dbContext.Tasks.FindAsync(id);

            if (task is null)
            {
                return null;
            }

            task.IsCompleted = true;

            await _dbContext.SaveChangesAsync();

            return MapToResponse(task);
        }

        public async Task<TaskResponse?> ReopenTaskAsync(int id)
        {
            var task = await _dbContext.Tasks.FindAsync(id);

            if (task is null)
            {
                return null;
            }

            task.IsCompleted = false;

            await _dbContext.SaveChangesAsync();

            return MapToResponse(task);
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
