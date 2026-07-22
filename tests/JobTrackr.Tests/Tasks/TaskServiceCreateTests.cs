using JobTrackr.Application.Common;
using JobTrackr.Application.Tasks;
using JobTrackr.Domain.Entities;
using JobTrackr.Infrastructure.Persistence;
using JobTrackr.Infrastructure.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Tests.Tasks
{
    public class TaskServiceCreateTests
    {
        [Fact]
        public async Task CreateTaskAsync_WithValidRequest_CreatesTask()
        {
            await using var dbContext = CreateDbContext();
            var user = await AddUserAsync(dbContext);
            var taskService = new TaskService(dbContext);
            var request = new CreateTaskRequest
            {
                Title = "Prepare interview notes",
                Description = "Review backend questions",
                Priority = "High"
            };

            var response = await taskService.CreateTaskAsync(request, user.Id);

            Assert.Equal(request.Title, response.Title);
            Assert.Equal(request.Description, response.Description);
            Assert.Equal(request.Priority, response.Priority);
            Assert.Equal(user.Id, response.UserId);
            Assert.False(response.IsCompleted);
            Assert.Equal(1, await dbContext.Tasks.CountAsync());
        }

        [Fact]
        public async Task CreateTaskAsync_WithEmptyTitle_ThrowsArgumentException()
        {
            await using var dbContext = CreateDbContext();
            var user = await AddUserAsync(dbContext);
            var taskService = new TaskService(dbContext);
            var request = new CreateTaskRequest
            {
                Title = "",
                Description = "Invalid task",
                Priority = "Medium"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => taskService.CreateTaskAsync(request, user.Id));

            Assert.Equal(ErrorMessages.TaskTitleRequired, exception.Message);
            Assert.Empty(dbContext.Tasks);
        }

        private static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static async Task<User> AddUserAsync(AppDbContext dbContext)
        {
            var user = new User
            {
                FullName = "Task Test User",
                Email = "task.test@example.com",
                PasswordHash = "test-password-hash"
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return user;
        }
    }
}
