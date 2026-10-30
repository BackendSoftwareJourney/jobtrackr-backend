using JobTrackr.Application.Comments;
using JobTrackr.Application.Common;
using JobTrackr.Domain.Entities;
using JobTrackr.Infrastructure.Comments;
using JobTrackr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Tests.Comments;

public class TaskCommentServiceTests
{
    [Fact]
    public async Task CreateAsync_WithOwnedTask_SavesTrimmedComment()
    {
        await using var db = CreateDbContext();
        var task = await AddTaskAsync(db);
        var service = new TaskCommentService(db);
        var before = DateTime.UtcNow;

        var response = await service.CreateAsync(task.Id,
            new CreateTaskCommentRequest { Content = "  Progress update  " },
            task.UserId);

        Assert.NotNull(response);
        Assert.True(response.Id > 0);
        Assert.Equal(task.Id, response.TaskId);
        Assert.Equal("Progress update", response.Content);
        Assert.InRange(response.CreatedAtUtc, before, DateTime.UtcNow);

        db.ChangeTracker.Clear();
        var saved = await db.TaskComments.SingleAsync();
        Assert.Equal(response.Id, saved.Id);
        Assert.Equal(task.Id, saved.TaskId);
        Assert.Equal(response.Content, saved.Content);
        Assert.Equal(response.CreatedAtUtc, saved.CreatedAtUtc);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("oversized", true)]
    public async Task CreateAsync_WithInvalidContent_DoesNotSave(
        string content, bool oversized)
    {
        await using var db = CreateDbContext();
        var task = await AddTaskAsync(db);
        var service = new TaskCommentService(db);
        var request = new CreateTaskCommentRequest
        {
            Content = oversized ? new string('a', 1001) : content
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(task.Id, request, task.UserId));

        Assert.Equal(oversized
            ? ErrorMessages.CommentContentTooLong
            : ErrorMessages.CommentContentRequired, exception.Message);
        Assert.Empty(await db.TaskComments.ToListAsync());
    }

    [Fact]
    public async Task CreateAsync_WithMissingOrUnownedTask_ReturnsNull()
    {
        await using var db = CreateDbContext();
        var task = await AddTaskAsync(db);
        var otherTask = await AddTaskAsync(db);
        var service = new TaskCommentService(db);
        var request = new CreateTaskCommentRequest { Content = "Not allowed" };

        Assert.Null(await service.CreateAsync(int.MaxValue, request, task.UserId));
        Assert.Null(await service.CreateAsync(task.Id, request, otherTask.UserId));
        Assert.Empty(await db.TaskComments.ToListAsync());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyOwnedTaskCommentsInStableOrder()
    {
        await using var db = CreateDbContext();
        var task = await AddTaskAsync(db);
        var otherTask = await AddTaskAsync(db);
        var service = new TaskCommentService(db);
        var time = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var later = new TaskComment
        {
            TaskId = task.Id,
            Content = "Later",
            CreatedAtUtc = time.AddHours(1)
        };
        var first = new TaskComment
        {
            TaskId = task.Id,
            Content = "First",
            CreatedAtUtc = time
        };
        var second = new TaskComment
        {
            TaskId = task.Id,
            Content = "Second",
            CreatedAtUtc = time
        };
        db.TaskComments.AddRange(later, first, second,
            new TaskComment { TaskId = otherTask.Id, Content = "Other task" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var response = await service.GetAllAsync(task.Id, task.UserId);

        Assert.NotNull(response);
        Assert.Equal(new[] { first.Id, second.Id, later.Id },
            response.Select(comment => comment.Id));
        Assert.All(response, comment => Assert.Equal(task.Id, comment.TaskId));
        Assert.Equal("First", response[0].Content);
        Assert.Equal(time, response[0].CreatedAtUtc);
        Assert.Null(await service.GetAllAsync(task.Id, otherTask.UserId));
        Assert.Null(await service.GetAllAsync(int.MaxValue, task.UserId));
    }

    [Fact]
    public async Task GetAllAsync_WithOwnedTaskWithoutComments_ReturnsEmptyList()
    {
        await using var db = CreateDbContext();
        var task = await AddTaskAsync(db);
        var service = new TaskCommentService(db);

        var response = await service.GetAllAsync(task.Id, task.UserId);

        Assert.NotNull(response);
        Assert.Empty(response);
    }

    [Fact]
    public async Task DeleteAsync_WithOwnedComment_RemovesOnlyThatComment()
    {
        await using var db = CreateDbContext();
        var task = await AddTaskAsync(db);
        var service = new TaskCommentService(db);
        var removed = await service.CreateAsync(task.Id,
            new CreateTaskCommentRequest { Content = "Remove" }, task.UserId);
        var retained = await service.CreateAsync(task.Id,
            new CreateTaskCommentRequest { Content = "Keep" }, task.UserId);
        Assert.NotNull(removed);
        Assert.NotNull(retained);

        Assert.True(await service.DeleteAsync(task.Id, removed.Id, task.UserId));
        db.ChangeTracker.Clear();
        var saved = await db.TaskComments.SingleAsync();
        Assert.Equal(retained.Id, saved.Id);
        Assert.NotNull(await db.Tasks.FindAsync(task.Id));
        Assert.NotNull(await db.Users.FindAsync(task.UserId));
        Assert.False(await service.DeleteAsync(task.Id, removed.Id, task.UserId));
    }

    [Fact]
    public async Task DeleteAsync_WithUnownedMismatchedOrMissingTarget_PreservesComment()
    {
        await using var db = CreateDbContext();
        var task = await AddTaskAsync(db);
        var otherTask = await AddTaskAsync(db);
        var service = new TaskCommentService(db);
        var comment = await service.CreateAsync(task.Id,
            new CreateTaskCommentRequest { Content = "Keep" }, task.UserId);
        Assert.NotNull(comment);

        Assert.False(await service.DeleteAsync(task.Id, comment.Id, otherTask.UserId));
        Assert.False(await service.DeleteAsync(otherTask.Id, comment.Id, otherTask.UserId));
        Assert.False(await service.DeleteAsync(task.Id, int.MaxValue, task.UserId));
        Assert.Equal(comment.Id, (await db.TaskComments.SingleAsync()).Id);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<JobTask> AddTaskAsync(AppDbContext db)
    {
        var task = new JobTask
        {
            Title = "Comment test task",
            User = new User
            {
                FullName = "Comment Test User",
                Email = $"comment-{Guid.NewGuid():N}@example.com",
                PasswordHash = "test-only-hash"
            }
        };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();
        return task;
    }
}