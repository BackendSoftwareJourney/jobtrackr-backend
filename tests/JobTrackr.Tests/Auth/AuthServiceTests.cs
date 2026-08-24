using JobTrackr.Application.Auth;
using JobTrackr.Application.Common;
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

        [Fact]
        public async Task ChangePasswordAsync_WithValidRequest_ChangesPassword()
        {
            await using var dbContext = CreateDbContext();
            var passwordHasher = new PasswordHasherService();
            var authService = new AuthService(
                dbContext,
                passwordHasher,
                new FakeJwtTokenService());
            var registeredUser = await RegisterChangePasswordUserAsync(authService);
            var savedUser = await dbContext.Users.SingleAsync();
            var originalPasswordHash = savedUser.PasswordHash;

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "Password123",
                NewPassword = "NewPassword456",
                ConfirmNewPassword = "NewPassword456"
            };

            var passwordChanged = await authService.ChangePasswordAsync(
                registeredUser.UserId,
                request);

            Assert.True(passwordChanged);
            Assert.NotEqual(originalPasswordHash, savedUser.PasswordHash);
            Assert.False(passwordHasher.VerifyPassword(
                request.CurrentPassword,
                savedUser.PasswordHash));
            Assert.True(passwordHasher.VerifyPassword(
                request.NewPassword,
                savedUser.PasswordHash));

            var newPasswordLogin = await authService.LoginAsync(new LoginRequest
            {
                Email = registeredUser.Email,
                Password = request.NewPassword
            });

            Assert.Equal(registeredUser.UserId, newPasswordLogin.UserId);
            Assert.Equal(
                $"test-token-{registeredUser.UserId}",
                newPasswordLogin.Token);

            var oldPasswordException = await Assert.ThrowsAsync<ArgumentException>(
                () => authService.LoginAsync(new LoginRequest
                {
                    Email = registeredUser.Email,
                    Password = request.CurrentPassword
                }));

            Assert.Equal("Invalid email or password.", oldPasswordException.Message);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithIncorrectCurrentPassword_ThrowsArgumentException()
        {
            await using var dbContext = CreateDbContext();
            var authService = CreateAuthService(dbContext);
            var registeredUser = await RegisterChangePasswordUserAsync(authService);
            var savedUser = await dbContext.Users.SingleAsync();
            var originalPasswordHash = savedUser.PasswordHash;

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "WrongPassword",
                NewPassword = "NewPassword456",
                ConfirmNewPassword = "NewPassword456"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => authService.ChangePasswordAsync(
                    registeredUser.UserId,
                    request));

            Assert.Equal(ErrorMessages.CurrentPasswordIncorrect, exception.Message);
            Assert.Equal(originalPasswordHash, savedUser.PasswordHash);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithMismatchedConfirmation_ThrowsArgumentException()
        {
            await using var dbContext = CreateDbContext();
            var authService = CreateAuthService(dbContext);
            var registeredUser = await RegisterChangePasswordUserAsync(authService);
            var savedUser = await dbContext.Users.SingleAsync();
            var originalPasswordHash = savedUser.PasswordHash;

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "Password123",
                NewPassword = "NewPassword456",
                ConfirmNewPassword = "DifferentPassword789"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => authService.ChangePasswordAsync(
                    registeredUser.UserId,
                    request));

            Assert.Equal(ErrorMessages.NewPasswordMismatch, exception.Message);
            Assert.Equal(originalPasswordHash, savedUser.PasswordHash);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithSamePassword_ThrowsArgumentException()
        {
            await using var dbContext = CreateDbContext();
            var authService = CreateAuthService(dbContext);
            var registeredUser = await RegisterChangePasswordUserAsync(authService);
            var savedUser = await dbContext.Users.SingleAsync();
            var originalPasswordHash = savedUser.PasswordHash;

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "Password123",
                NewPassword = "Password123",
                ConfirmNewPassword = "Password123"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => authService.ChangePasswordAsync(
                    registeredUser.UserId,
                    request));

            Assert.Equal(ErrorMessages.NewPasswordMustBeDifferent, exception.Message);
            Assert.Equal(originalPasswordHash, savedUser.PasswordHash);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithMissingUser_ReturnsFalse()
        {
            await using var dbContext = CreateDbContext();
            var authService = CreateAuthService(dbContext);

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "Password123",
                NewPassword = "NewPassword456",
                ConfirmNewPassword = "NewPassword456"
            };

            var passwordChanged = await authService.ChangePasswordAsync(999, request);

            Assert.False(passwordChanged);
            Assert.Empty(dbContext.Users);
        }

        private static Task<AuthResponse> RegisterChangePasswordUserAsync(
            AuthService authService)
        {
            return authService.RegisterAsync(new RegisterRequest
            {
                FullName = "Change Password Test User",
                Email = "change.password@example.com",
                Password = "Password123"
            });
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
