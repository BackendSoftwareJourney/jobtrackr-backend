using JobTrackr.Application.Common;
using JobTrackr.Application.Users;
using JobTrackr.Infrastructure.Persistence;
using JobTrackr.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Tests.Users
{
    public class UserServiceTests
    {
        [Fact]
        public async Task CreateAsync_WithValidRequest_CreatesUser()
        {
            await using var dbContext = CreateDbContext();
            var userService = new UserService(dbContext);
            var request = new CreateUserRequest
            {
                FullName = "Create User Test",
                Email = "create.user@example.com"
            };

            var response = await userService.CreateAsync(request);

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

            var createdUser = await userService.CreateAsync(createRequest);

            var response = await userService.GetByIdAsync(createdUser.Id);

            Assert.NotNull(response);
            Assert.Equal(createdUser.Id, response.Id);
            Assert.Equal(createRequest.FullName, response.FullName);
            Assert.Equal(createRequest.Email, response.Email);
            Assert.Equal(createdUser.CreatedAtUtc, response.CreatedAtUtc);
        }

        [Fact]
        public async Task CreateAsync_WithInvalidEmail_ThrowsArgumentException()
        {
            await using var dbContext = CreateDbContext();
            var userService = new UserService(dbContext);
            var request = new CreateUserRequest
            {
                FullName = "Invalid Email User",
                Email = "not-an-email"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => userService.CreateAsync(request));

            Assert.Equal(ErrorMessages.EmailInvalid, exception.Message);
            Assert.Empty(dbContext.Users);
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateEmail_ThrowsArgumentException()
        {
            await using var dbContext = CreateDbContext();
            var userService = new UserService(dbContext);

            await userService.CreateAsync(new CreateUserRequest
            {
                FullName = "First User",
                Email = "duplicate@example.com"
            });

            var duplicateRequest = new CreateUserRequest
            {
                FullName = "Second User",
                Email = "DUPLICATE@example.com"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => userService.CreateAsync(duplicateRequest));

            Assert.Equal(ErrorMessages.EmailAlreadyInUse, exception.Message);
            Assert.Equal(1, await dbContext.Users.CountAsync());
        }

        [Fact]
        public async Task UpdateAsync_WithAnotherUsersEmail_ThrowsArgumentException()
        {
            await using var dbContext = CreateDbContext();
            var userService = new UserService(dbContext);
            var firstUser = await userService.CreateAsync(new CreateUserRequest
            {
                FullName = "First User",
                Email = "first@example.com"
            });
            var secondUser = await userService.CreateAsync(new CreateUserRequest
            {
                FullName = "Second User",
                Email = "second@example.com"
            });
            var request = new UpdateUserRequest
            {
                FullName = secondUser.FullName,
                Email = firstUser.Email.ToUpperInvariant()
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => userService.UpdateAsync(secondUser.Id, request));

            Assert.Equal(ErrorMessages.EmailAlreadyInUse, exception.Message);

            var savedUser = await dbContext.Users.FindAsync(secondUser.Id);

            Assert.NotNull(savedUser);
            Assert.Equal(secondUser.Email, savedUser.Email);
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
