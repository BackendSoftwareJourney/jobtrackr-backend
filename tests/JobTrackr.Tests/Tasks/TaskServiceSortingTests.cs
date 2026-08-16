using JobTrackr.Domain.Entities;
using JobTrackr.Infrastructure.Persistence;
using JobTrackr.Infrastructure.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Tests.Tasks
{
    public class TaskServiceSortingTests
    {
        [Fact]
        public async Task GetAllAsync_WithDescendingSort_ReturnsNewestTasksFirst()
        {
            await using var dbContext = CreateDbContext();
            var user = await AddUserWithTasksAsync(dbContext);
            var taskService = new TaskService(dbContext);

            var response = await taskService.GetAllAsync(
                null,
                null,
                "desc",
                1,
                10,
                user.Id);

            Assert.Equal("Newest task", response.Items[0].Title);
            Assert.Equal("Middle task", response.Items[1].Title);
            Assert.Equal("Oldest task", response.Items[2].Title);
        }

        [Fact]
        public async Task GetAllAsync_WithAscendingSort_ReturnsOldestTasksFirst()
        {
            await using var dbContext = CreateDbContext();
            var user = await AddUserWithTasksAsync(dbContext);
            var taskService = new TaskService(dbContext);

            var response = await taskService.GetAllAsync(
                null,
                null,
                "asc",
                1,
                10,
                user.Id);

            Assert.Equal("Oldest task", response.Items[0].Title);
            Assert.Equal("Middle task", response.Items[1].Title);
            Assert.Equal("Newest task", response.Items[2].Title);
        }

        private static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static async Task<User> AddUserWithTasksAsync(AppDbContext dbContext)
        {
            var user = new User
            {
                FullName = "Sorting Test User",
                Email = "sorting.test@example.com",
                PasswordHash = "test-password-hash"
            };

            user.Tasks.Add(new JobTask
            {
                Title = "Middle task",
                Priority = "Medium",
                CreatedAtUtc = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            user.Tasks.Add(new JobTask
            {
                Title = "Newest task",
                Priority = "Medium",
                CreatedAtUtc = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            user.Tasks.Add(new JobTask
            {
                Title = "Oldest task",
                Priority = "Medium",
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return user;
        }
    }
}
