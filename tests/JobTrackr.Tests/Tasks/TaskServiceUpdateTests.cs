using JobTrackr.Application.Common;
using JobTrackr.Application.Tasks;
using JobTrackr.Domain.Entities;
using JobTrackr.Infrastructure.Persistence;
using JobTrackr.Infrastructure.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Tests.Tasks
{
    public class TaskServiceUpdateTests
    {
        [Fact]
        public async Task UpdateTaskAsync_WithValidRequest_UpdatesTask()
        {
            await using var dbContext = CreateDbContext();
            var task = await AddTaskAsync(dbContext);
            var taskService = new TaskService(dbContext);
            var request = new UpdateTaskRequest
            {
                Title = "Updated task title",
                Description = "Updated task description",
                DueDateUtc = DateTime.UtcNow.AddDays(7),
                Priority = "High"
            };

            var response = await taskService.UpdateTaskAsync(
                task.Id,
                request,
                task.UserId);

            Assert.NotNull(response);
            Assert.Equal(request.Title, response.Title);
            Assert.Equal(request.Description, response.Description);
            Assert.Equal(request.DueDateUtc, response.DueDateUtc);
            Assert.Equal(request.Priority, response.Priority);
            Assert.Equal(task.UserId, response.UserId);

            var savedTask = await dbContext.Tasks.SingleAsync();

            Assert.Equal(request.Title, savedTask.Title);
            Assert.Equal(request.Description, savedTask.Description);
            Assert.Equal(request.DueDateUtc, savedTask.DueDateUtc);
            Assert.Equal(request.Priority, savedTask.Priority);
        }

        [Fact]
        public async Task UpdateTaskAsync_WithMissingTask_ReturnsNull()
        {
            await using var dbContext = CreateDbContext();
            var taskService = new TaskService(dbContext);
            var request = new UpdateTaskRequest
            {
                Title = "Updated title",
                Description = "Updated description",
                Priority = "Medium"
            };

            var response = await taskService.UpdateTaskAsync(999, request, 1);

            Assert.Null(response);
        }

        [Fact]
        public async Task UpdateTaskAsync_WithEmptyTitle_ThrowsArgumentException()
        {
            await using var dbContext = CreateDbContext();
            var task = await AddTaskAsync(dbContext);
            var originalTitle = task.Title;
            var taskService = new TaskService(dbContext);
            var request = new UpdateTaskRequest
            {
                Title = "",
                Description = "This update should fail",
                Priority = "Medium"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => taskService.UpdateTaskAsync(task.Id, request, task.UserId));

            Assert.Equal(ErrorMessages.TaskTitleRequired, exception.Message);

            var savedTask = await dbContext.Tasks.SingleAsync();

            Assert.Equal(originalTitle, savedTask.Title);
        }

        private static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static async Task<JobTask> AddTaskAsync(AppDbContext dbContext)
        {
            var user = new User
            {
                FullName = "Update Task Test User",
                Email = "update.task@example.com",
                PasswordHash = "test-password-hash"
            };
            var task = new JobTask
            {
                Title = "Original task title",
                Description = "Original task description",
                Priority = "Low",
                User = user
            };

            dbContext.Tasks.Add(task);
            await dbContext.SaveChangesAsync();

            return task;
        }
    }
}