using JobTrackr.Domain.Entities;
using JobTrackr.Infrastructure.Persistence;
using JobTrackr.Infrastructure.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Tests.Tasks
{
    public class TaskServicePaginationTests
    {
        [Fact]
        public async Task GetAllAsync_WithSmallPageSize_ReturnsRequestedPage()
        {
            await using var dbContext = CreateDbContext();
            var user = await AddUserWithTasksAsync(dbContext, 5);
            var taskService = new TaskService(dbContext);

            var response = await taskService.GetAllAsync(
                null,
                null,
                2,
                2,
                user.Id);

            Assert.Equal(2, response.Count);
            Assert.Equal("Task 3", response[0].Title);
            Assert.Equal("Task 4", response[1].Title);
        }

        [Fact]
        public async Task GetAllAsync_WithLargePageSize_ReturnsAllMatchingTasks()
        {
            await using var dbContext = CreateDbContext();
            var user = await AddUserWithTasksAsync(dbContext, 5);
            var taskService = new TaskService(dbContext);

            var response = await taskService.GetAllAsync(
                null,
                null,
                1,
                100,
                user.Id);

            Assert.Equal(5, response.Count);
        }

        private static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static async Task<User> AddUserWithTasksAsync(
            AppDbContext dbContext,
            int taskCount)
        {
            var user = new User
            {
                FullName = "Pagination Test User",
                Email = "pagination.test@example.com",
                PasswordHash = "test-password-hash"
            };

            for (var number = 1; number <= taskCount; number++)
            {
                user.Tasks.Add(new JobTask
                {
                    Title = $"Task {number}",
                    Description = $"Pagination task {number}",
                    Priority = "Medium"
                });
            }

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return user;
        }
    }
}