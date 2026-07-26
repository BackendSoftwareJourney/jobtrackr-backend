using JobTrackr.Application.Users;
using JobTrackr.Infrastructure.Persistence;
using JobTrackr.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Tests.Users
{
    public class UserServiceTests
    {
        [Fact]
        public async Task CreateUserAsync_WithValidRequest_CreatesUser()
        {
            await using var dbContext = CreateDbContext();
            var userService = new UserService(dbContext);
            var request = new CreateUserRequest
            {
                FullName = "Create User Test",
                Email = "create.user@example.com"
            };

            var response = await userService.CreateUserAsync(request);

            Assert.True(response.Id > 0);
            Assert.Equal(request.FullName, response.FullName);
            Assert.Equal(request.Email, response.Email);
            Assert.Equal(1, await dbContext.Users.CountAsync());

            var savedUser = await dbContext.Users.SingleAsync();

            Assert.Equal(request.FullName, savedUser.FullName);
            Assert.Equal(request.Email, savedUser.Email);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingUser_ReturnsUser()
        {
            await using var dbContext = CreateDbContext();
            var userService = new UserService(dbContext);
            var createRequest = new CreateUserRequest
            {
                FullName = "Get User Test",
                Email = "get.user@example.com"
            };

            var createdUser = await userService.CreateUserAsync(createRequest);

            var response = await userService.GetByIdAsync(createdUser.Id);

            Assert.NotNull(response);
            Assert.Equal(createdUser.Id, response.Id);
            Assert.Equal(createRequest.FullName, response.FullName);
            Assert.Equal(createRequest.Email, response.Email);
            Assert.Equal(createdUser.CreatedAtUtc, response.CreatedAtUtc);
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
