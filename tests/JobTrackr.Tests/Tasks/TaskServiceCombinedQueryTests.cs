using JobTrackr.Domain.Entities;
using JobTrackr.Infrastructure.Persistence;
using JobTrackr.Infrastructure.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Tests.Tasks
{
    public class TaskServiceCombinedQueryTests
    {
        [Fact]
        public async Task GetAllAsync_WithCombinedQuery_ReturnsFilteredSortedFirstPage()
        {
            await using var dbContext = CreateDbContext();
            var user = await AddQueryTestDataAsync(dbContext);
            var taskService = new TaskService(dbContext);

            var response = await taskService.GetAllAsync(
                false,
                "API",
                "dueDate",
                "asc",
                1,
                2,
                user.Id);

            Assert.Equal(2, response.Items.Count);
            Assert.Equal("API due soon", response.Items[0].Title);
            Assert.Equal("API due middle", response.Items[1].Title);
            Assert.All(response.Items, task => Assert.Equal(user.Id, task.UserId));
            Assert.Equal(1, response.PageNumber);
            Assert.Equal(2, response.PageSize);
            Assert.Equal(4, response.TotalCount);
            Assert.Equal(2, response.TotalPages);
        }

        [Fact]
        public async Task GetAllAsync_WithCombinedQuery_ReturnsCorrectSecondPageAndMetadata()
        {
            await using var dbContext = CreateDbContext();
            var user = await AddQueryTestDataAsync(dbContext);
            var taskService = new TaskService(dbContext);

            var response = await taskService.GetAllAsync(
                false,
                "API",
                "dueDate",
                "asc",
                2,
                2,
                user.Id);

            Assert.Equal(2, response.Items.Count);
            Assert.Equal("API due later", response.Items[0].Title);
            Assert.Equal("API no due date", response.Items[1].Title);
            Assert.All(response.Items, task => Assert.Equal(user.Id, task.UserId));
            Assert.Equal(2, response.PageNumber);
            Assert.Equal(2, response.PageSize);
            Assert.Equal(4, response.TotalCount);
            Assert.Equal(2, response.TotalPages);
        }

        private static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static async Task<User> AddQueryTestDataAsync(AppDbContext dbContext)
        {
            var user = new User
            {
                FullName = "Combined Query User",
                Email = "combined.query@example.com",
                PasswordHash = "test-password-hash"
            };

            user.Tasks.Add(new JobTask
            {
                Title = "API due later",
                Priority = "Medium",
                IsCompleted = false,
                DueDateUtc = new DateTime(2026, 8, 22, 0, 0, 0, DateTimeKind.Utc)
            });

            user.Tasks.Add(new JobTask
            {
                Title = "API due soon",
                Priority = "Medium",
                IsCompleted = false,
                DueDateUtc = new DateTime(2026, 8, 18, 0, 0, 0, DateTimeKind.Utc)
            });

            user.Tasks.Add(new JobTask
            {
                Title = "API no due date",
                Priority = "Medium",
                IsCompleted = false,
                DueDateUtc = null
            });

            user.Tasks.Add(new JobTask
            {
                Title = "API due middle",
                Priority = "Medium",
                IsCompleted = false,
                DueDateUtc = new DateTime(2026, 8, 20, 0, 0, 0, DateTimeKind.Utc)
            });

            user.Tasks.Add(new JobTask
            {
                Title = "API completed",
                Priority = "Medium",
                IsCompleted = true,
                DueDateUtc = new DateTime(2026, 8, 17, 0, 0, 0, DateTimeKind.Utc)
            });

            user.Tasks.Add(new JobTask
            {
                Title = "Database task",
                Priority = "Medium",
                IsCompleted = false,
                DueDateUtc = new DateTime(2026, 8, 16, 0, 0, 0, DateTimeKind.Utc)
            });

            var otherUser = new User
            {
                FullName = "Other Query User",
                Email = "other.query@example.com",
                PasswordHash = "test-password-hash"
            };

            otherUser.Tasks.Add(new JobTask
            {
                Title = "API other user task",
                Priority = "Medium",
                IsCompleted = false,
                DueDateUtc = new DateTime(2026, 8, 15, 0, 0, 0, DateTimeKind.Utc)
            });

            dbContext.Users.AddRange(user, otherUser);
            await dbContext.SaveChangesAsync();

            return user;
        }
    }
}