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
                "createdAt",
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
                "createdAt",
                "asc",
                1,
                10,
                user.Id);

            Assert.Equal("Oldest task", response.Items[0].Title);
            Assert.Equal("Middle task", response.Items[1].Title);
            Assert.Equal("Newest task", response.Items[2].Title);
        }

        [Fact]
        public async Task GetAllAsync_WithDueDateAscending_ReturnsEarliestDueDateFirstAndNullLast()
        {
            await using var dbContext = CreateDbContext();
            var user = await AddUserWithDueDatesAsync(dbContext);
            var taskService = new TaskService(dbContext);

            var response = await taskService.GetAllAsync(
                null,
                null,
                "dueDate",
                "asc",
                1,
                10,
                user.Id);

            Assert.Equal("Due soon", response.Items[0].Title);
            Assert.Equal("Due later", response.Items[1].Title);
            Assert.Equal("No due date", response.Items[2].Title);
        }

        [Fact]
        public async Task GetAllAsync_WithDueDateDescending_ReturnsLatestDueDateFirstAndNullLast()
        {
            await using var dbContext = CreateDbContext();
            var user = await AddUserWithDueDatesAsync(dbContext);
            var taskService = new TaskService(dbContext);

            var response = await taskService.GetAllAsync(
                null,
                null,
                "dueDate",
                "desc",
                1,
                10,
                user.Id);

            Assert.Equal("Due later", response.Items[0].Title);
            Assert.Equal("Due soon", response.Items[1].Title);
            Assert.Equal("No due date", response.Items[2].Title);
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

        private static async Task<User> AddUserWithDueDatesAsync(AppDbContext dbContext)
        {
            var user = new User
            {
                FullName = "Due Date Sorting User",
                Email = "due.date.sorting@example.com",
                PasswordHash = "test-password-hash"
            };

            user.Tasks.Add(new JobTask
            {
                Title = "No due date",
                Priority = "Medium",
                DueDateUtc = null
            });

            user.Tasks.Add(new JobTask
            {
                Title = "Due later",
                Priority = "Medium",
                DueDateUtc = new DateTime(2026, 8, 20, 0, 0, 0, DateTimeKind.Utc)
            });

            user.Tasks.Add(new JobTask
            {
                Title = "Due soon",
                Priority = "Medium",
                DueDateUtc = new DateTime(2026, 8, 18, 0, 0, 0, DateTimeKind.Utc)
            });

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return user;
        }
    }
}
