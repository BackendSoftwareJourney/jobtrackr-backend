using JobTrackr.Application.Auth;
using JobTrackr.Domain.Entities;
using JobTrackr.Infrastructure.Auth;
using JobTrackr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Tests.Auth
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task RegisterAsync_WithValidRequest_RegistersUser()
        {
            await using var dbContext = CreateDbContext();
            var passwordHasher = new PasswordHasherService();
            var authService = new AuthService(
                dbContext,
                passwordHasher,
                new FakeJwtTokenService());

            var request = new RegisterRequest
            {
                FullName = "Register Test User",
                Email = "register.test@example.com",
                Password = "Password123"
            };

            var response = await authService.RegisterAsync(request);

            Assert.True(response.UserId > 0);
            Assert.Equal(request.FullName, response.FullName);
            Assert.Equal(request.Email, response.Email);
            Assert.Empty(response.Token);

            var savedUser = await dbContext.Users.SingleAsync();

            Assert.NotEqual(request.Password, savedUser.PasswordHash);
            Assert.True(passwordHasher.VerifyPassword(
                request.Password,
                savedUser.PasswordHash));
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
        {
            await using var dbContext = CreateDbContext();
            var authService = CreateAuthService(dbContext);
            var registerRequest = new RegisterRequest
            {
                FullName = "Login Test User",
                Email = "login.test@example.com",
                Password = "Password123"
            };

            var registeredUser = await authService.RegisterAsync(registerRequest);

            var loginRequest = new LoginRequest
            {
                Email = registerRequest.Email,
                Password = registerRequest.Password
            };

            var response = await authService.LoginAsync(loginRequest);

            Assert.Equal(registeredUser.UserId, response.UserId);
            Assert.Equal(registerRequest.FullName, response.FullName);
            Assert.Equal(registerRequest.Email, response.Email);
            Assert.Equal($"test-token-{registeredUser.UserId}", response.Token);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ThrowsArgumentException()
        {
            await using var dbContext = CreateDbContext();
            var authService = CreateAuthService(dbContext);
            var registerRequest = new RegisterRequest
            {
                FullName = "Invalid Login Test User",
                Email = "invalid.login@example.com",
                Password = "Password123"
            };

            await authService.RegisterAsync(registerRequest);

            var loginRequest = new LoginRequest
            {
                Email = registerRequest.Email,
                Password = "WrongPassword"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => authService.LoginAsync(loginRequest));

            Assert.Equal("Invalid email or password.", exception.Message);
        }

        private static AuthService CreateAuthService(AppDbContext dbContext)
        {
            return new AuthService(
                dbContext,
                new PasswordHasherService(),
                new FakeJwtTokenService());
        }

        private static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private class FakeJwtTokenService : IJwtTokenService
        {
            public string GenerateToken(User user)
            {
                return $"test-token-{user.Id}";
            }
        }
    }
}
