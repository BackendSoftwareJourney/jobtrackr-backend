using JobTrackr.Application.Tasks;
using JobTrackr.Domain.Entities;
using JobTrackr.Infrastructure.Persistence;
using JobTrackr.Infrastructure.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Tests.Tasks
{
    public class TaskServiceAuthorizationTests
    {
        [Fact]
        public async Task GetByIdAsync_WithAnotherUsersTask_ReturnsNull()
        {
            await using var dbContext = CreateDbContext();
            var testData = await AddUsersAndTaskAsync(dbContext);
            var taskService = new TaskService(dbContext);

            var response = await taskService.GetByIdAsync(
                testData.Task.Id,
                testData.OtherUser.Id);

            Assert.Null(response);
        }

        [Fact]
        public async Task UpdateTaskAsync_WithAnotherUsersTask_ReturnsNullAndPreservesTask()
        {
            await using var dbContext = CreateDbContext();
            var testData = await AddUsersAndTaskAsync(dbContext);
            var taskService = new TaskService(dbContext);
            var originalTitle = testData.Task.Title;
            var request = new UpdateTaskRequest
            {
                Title = "Unauthorized update",
                Description = "This must not be saved",
                Priority = "High"
            };

            var response = await taskService.UpdateTaskAsync(
                testData.Task.Id,
                request,
                testData.OtherUser.Id);

            Assert.Null(response);
            Assert.Equal(originalTitle, testData.Task.Title);
        }

        [Fact]
        public async Task DeleteTaskAsync_WithAnotherUsersTask_ReturnsFalseAndPreservesTask()
        {
            await using var dbContext = CreateDbContext();
            var testData = await AddUsersAndTaskAsync(dbContext);
            var taskService = new TaskService(dbContext);

            var isDeleted = await taskService.DeleteTaskAsync(
                testData.Task.Id,
                testData.OtherUser.Id);

            Assert.False(isDeleted);
            Assert.True(await dbContext.Tasks.AnyAsync(
                task => task.Id == testData.Task.Id));
        }

        [Fact]
        public async Task CompleteTaskAsync_WithAnotherUsersTask_ReturnsNullAndPreservesState()
        {
            await using var dbContext = CreateDbContext();
            var testData = await AddUsersAndTaskAsync(dbContext);
            var taskService = new TaskService(dbContext);

            var response = await taskService.CompleteTaskAsync(
                testData.Task.Id,
                testData.OtherUser.Id);

            Assert.Null(response);
            Assert.False(testData.Task.IsCompleted);
        }

        [Fact]
        public async Task ReopenTaskAsync_WithAnotherUsersTask_ReturnsNullAndPreservesState()
        {
            await using var dbContext = CreateDbContext();
            var testData = await AddUsersAndTaskAsync(dbContext, isCompleted: true);
            var taskService = new TaskService(dbContext);

            var response = await taskService.ReopenTaskAsync(
                testData.Task.Id,
                testData.OtherUser.Id);

            Assert.Null(response);
            Assert.True(testData.Task.IsCompleted);
        }

        [Fact]
        public async Task CompleteAndReopenTaskAsync_WithOwnedTask_UpdatesCompletionState()
        {
            await using var dbContext = CreateDbContext();
            var testData = await AddUsersAndTaskAsync(dbContext);
            var taskService = new TaskService(dbContext);

            var completedTask = await taskService.CompleteTaskAsync(
                testData.Task.Id,
                testData.Owner.Id);

            Assert.NotNull(completedTask);
            Assert.True(completedTask.IsCompleted);
            Assert.True(testData.Task.IsCompleted);

            var reopenedTask = await taskService.ReopenTaskAsync(
                testData.Task.Id,
                testData.Owner.Id);

            Assert.NotNull(reopenedTask);
            Assert.False(reopenedTask.IsCompleted);
            Assert.False(testData.Task.IsCompleted);
        }

        private static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static async Task<AuthorizationTestData> AddUsersAndTaskAsync(
            AppDbContext dbContext,
            bool isCompleted = false)
        {
            var owner = new User
            {
                FullName = "Task Owner",
                Email = "task.owner@example.com",
                PasswordHash = "test-password-hash"
            };

            var otherUser = new User
            {
                FullName = "Other User",
                Email = "other.user@example.com",
                PasswordHash = "test-password-hash"
            };

            var task = new JobTask
            {
                Title = "Owner's private task",
                Description = "Only the owner may access this task",
                Priority = "Medium",
                IsCompleted = isCompleted,
                User = owner
            };

            dbContext.Tasks.Add(task);
            dbContext.Users.Add(otherUser);
            await dbContext.SaveChangesAsync();

            return new AuthorizationTestData(task, owner, otherUser);
        }

        private record AuthorizationTestData(
            JobTask Task,
            User Owner,
            User OtherUser);
    }
}