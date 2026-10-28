using JobTrackr.Application.Comments;
using JobTrackr.Application.Common;
using JobTrackr.Domain.Entities;
using JobTrackr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Infrastructure.Comments
{
    public class TaskCommentService : ITaskCommentService
    {
        private readonly AppDbContext _dbContext;

        public TaskCommentService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TaskCommentResponse?> CreateAsync(
            int taskId,
            CreateTaskCommentRequest request,
            int userId)
        {
            var ownsTask = await _dbContext.Tasks.AnyAsync(
                task => task.Id == taskId && task.UserId == userId);

            if (!ownsTask)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                throw new ArgumentException(ErrorMessages.CommentContentRequired);
            }

            if (request.Content.Length > 1000)
            {
                throw new ArgumentException(ErrorMessages.CommentContentTooLong);
            }

            var comment = new TaskComment
            {
                TaskId = taskId,
                Content = request.Content.Trim(),
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.TaskComments.Add(comment);
            await _dbContext.SaveChangesAsync();

            return new TaskCommentResponse
            {
                Id = comment.Id,
                TaskId = comment.TaskId,
                Content = comment.Content,
                CreatedAtUtc = comment.CreatedAtUtc
            };
        }

        public async Task<List<TaskCommentResponse>?> GetAllAsync(int taskId, int userId)
        {
            var ownsTask = await _dbContext.Tasks.AnyAsync(
                task => task.Id == taskId && task.UserId == userId);

            if (!ownsTask)
            {
                return null;
            }

            return await _dbContext.TaskComments
                .Where(comment => comment.TaskId == taskId &&
                    comment.Task!.UserId == userId)
                .OrderBy(comment => comment.CreatedAtUtc)
                .ThenBy(comment => comment.Id)
                .Select(comment => new TaskCommentResponse
                {
                    Id = comment.Id,
                    TaskId = comment.TaskId,
                    Content = comment.Content,
                    CreatedAtUtc = comment.CreatedAtUtc
                })
                .ToListAsync();
        }
    }
}