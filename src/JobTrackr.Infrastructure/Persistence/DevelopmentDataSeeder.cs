using JobTrackr.Application.Auth;
using JobTrackr.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Infrastructure.Persistence;

public static class DevelopmentDataSeeder
{
    private const string SeedEmail = "developer@jobtrackr.local";
    private const string SeedPassword = "Development123!";

    public static async Task SeedAsync(
        AppDbContext dbContext,
        IPasswordHasherService passwordHasherService)
    {
        var seedUserExists = await dbContext.Users
            .AnyAsync(user => user.Email == SeedEmail);

        if (seedUserExists)
        {
            return;
        }

        var now = DateTime.UtcNow;

        var user = new User
        {
            FullName = "Development User",
            Email = SeedEmail,
            PasswordHash = passwordHasherService.HashPassword(SeedPassword),
            CreatedAtUtc = now,
            Tasks =
            [
                new JobTask
                {
                    Title = "Review JobTrackr API",
                    Description = "Test filtering, sorting, and pagination.",
                    Priority = "High",
                    DueDateUtc = now.AddDays(3),
                    CreatedAtUtc = now.AddMinutes(-2)
                },
                new JobTask
                {
                    Title = "Update backend notes",
                    Description = "Record the latest development progress.",
                    Priority = "Medium",
                    DueDateUtc = now.AddDays(7),
                    CreatedAtUtc = now.AddMinutes(-1)
                },
                new JobTask
                {
                    Title = "Completed sample task",
                    Description = "Use this task to test completion filtering.",
                    IsCompleted = true,
                    Priority = "Low",
                    CreatedAtUtc = now
                }
            ]
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
    }
}