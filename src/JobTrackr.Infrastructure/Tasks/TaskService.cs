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

        public async Task<TaskResponse> CreateTask(CreateTaskRequest request)
        {
            if (request.UserId <= 0)
            {
                throw new ArgumentException("UserId is required.");
            }

            var userExists = await _dbContext.Users.AnyAsync(user => user.Id == request.UserId);

            if (!userExists)
            {
                throw new ArgumentException("User not found.");
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException("Task title is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Priority))
            {
                throw new ArgumentException("Task priority is required.");
            }

            var task = new JobTask()
            {
                Description = request.Description,
                Title = request.Title,
                CreatedAtUtc = DateTime.UtcNow,
                DueDateUtc = request.DueDateUtc,
                Priority = request.Priority,
                IsCompleted = false,
                UserId = request.UserId
            };

            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync();

            return MapToResponse(task);
        }

        public async Task<List<TaskResponse>> GetAll(bool? isCompleted, string? search, int? userId)
        {
            var query = _dbContext.Tasks.AsQueryable();

            if (isCompleted is not null)
            {
                query = query.Where(task => task.IsCompleted == isCompleted);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(task => task.Title.Contains(search));
            }

            if (userId is not null)
            {
                query = query.Where(task => task.UserId == userId);
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

        public async Task<TaskResponse?> GetById(int id)
        {
            var task = await _dbContext.Tasks.FindAsync(id);

            if (task is null)
            {
                return null;
            }

            return MapToResponse(task);
        }

        public async Task<TaskResponse?> UpdateTask(int id, UpdateTaskRequest request)
        {
            var task = await _dbContext.Tasks.FindAsync(id);

            if (task is null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException("Task title is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Priority))
            {
                throw new ArgumentException("Task priority is required.");
            }

            task.Title = request.Title;
            task.Description = request.Description;
            task.DueDateUtc = request.DueDateUtc;
            task.Priority = request.Priority;

            await _dbContext.SaveChangesAsync();

            return MapToResponse(task);
        }

        public async Task<bool> DeleteTask(int id)
        {
            var task = await _dbContext.Tasks.FindAsync(id);

            if (task is null)
            {
                return false;
            }

            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<TaskResponse?> CompleteTask(int id)
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

        public async Task<TaskResponse?> ReopenTask(int id)
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

        public async Task<List<TaskResponse>?> GetByUserId(int userId)
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
