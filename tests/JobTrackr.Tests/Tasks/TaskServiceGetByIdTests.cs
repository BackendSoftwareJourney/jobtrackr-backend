using JobTrackr.Domain.Entities;
using JobTrackr.Infrastructure.Persistence;
using JobTrackr.Infrastructure.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Tests.Tasks
{
    public class TaskServiceGetByIdTests
    {
        [Fact]
        public async Task GetByIdAsync_WithExistingOwnedTask_ReturnsTask()
        {
            await using var dbContext = CreateDbContext();
            var user = new User
            {
                FullName = "Get Task Test User",
                Email = "get.task@example.com",
                PasswordHash = "test-password-hash"
            };
            var task = new JobTask
            {
                Title = "Review Get By Id",
                Description = "Test existing task behavior",
                Priority = "High",
                User = user
            };

            dbContext.Tasks.Add(task);
            await dbContext.SaveChangesAsync();

            var taskService = new TaskService(dbContext);

            var response = await taskService.GetByIdAsync(task.Id, user.Id);

            Assert.NotNull(response);
            Assert.Equal(task.Id, response.Id);
            Assert.Equal(task.Title, response.Title);
            Assert.Equal(task.Description, response.Description);
            Assert.Equal(task.Priority, response.Priority);
            Assert.Equal(user.Id, response.UserId);
        }

        [Fact]
        public async Task GetByIdAsync_WithMissingTask_ReturnsNull()
        {
            await using var dbContext = CreateDbContext();
            var taskService = new TaskService(dbContext);

            var response = await taskService.GetByIdAsync(999, 1);

            Assert.Null(response);
        }

        private static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }
    }
}