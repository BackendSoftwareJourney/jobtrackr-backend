using JobTrackr.Domain.Entities;
using JobTrackr.Infrastructure.Persistence;
using JobTrackr.Infrastructure.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Tests.Tasks
{
    public class TaskServiceDeleteTests
    {
        [Fact]
        public async Task DeleteTaskAsync_WithExistingOwnedTask_DeletesTask()
        {
            await using var dbContext = CreateDbContext();
            var task = await AddTaskAsync(dbContext);
            var taskService = new TaskService(dbContext);

            var isDeleted = await taskService.DeleteTaskAsync(
                task.Id,
                task.UserId);

            Assert.True(isDeleted);
            Assert.Equal(0, await dbContext.Tasks.CountAsync());
            Assert.Equal(1, await dbContext.Users.CountAsync());
        }

        [Fact]
        public async Task DeleteTaskAsync_WithMissingTask_ReturnsFalse()
        {
            await using var dbContext = CreateDbContext();
            var taskService = new TaskService(dbContext);

            var isDeleted = await taskService.DeleteTaskAsync(999, 1);

            Assert.False(isDeleted);
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
                FullName = "Delete Task Test User",
                Email = "delete.task@example.com",
                PasswordHash = "test-password-hash"
            };
            var task = new JobTask
            {
                Title = "Task to delete",
                Description = "This task should be removed",
                Priority = "Medium",
                User = user
            };

            dbContext.Tasks.Add(task);
            await dbContext.SaveChangesAsync();

            return task;
        }
    }
}
