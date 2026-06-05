using JobTrackr.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<JobTask> Tasks { get; set; }
}
