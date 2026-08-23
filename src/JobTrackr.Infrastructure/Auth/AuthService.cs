using JobTrackr.Application.Auth;
using JobTrackr.Application.Common;
using JobTrackr.Domain.Entities;
using JobTrackr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Infrastructure.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly IPasswordHasherService _passwordHasherService;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            AppDbContext dbContext,
            IPasswordHasherService passwordHasherService,
            IJwtTokenService jwtTokenService)
        {
            _dbContext = dbContext;
            _passwordHasherService = passwordHasherService;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == request.Email);

            if (user is null)
            {
                throw new ArgumentException("Invalid email or password.");
            }

            var passwordIsValid = _passwordHasherService.VerifyPassword(request.Password, user.PasswordHash);

            if (!passwordIsValid)
            {
                throw new ArgumentException("Invalid email or password.");
            }

            var token = _jwtTokenService.GenerateToken(user);

            return new AuthResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Token = token
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var emailExists = await _dbContext.Users.AnyAsync(user => user.Email == request.Email);

            if (emailExists)
            {
                throw new ArgumentException("Email is already registered.");
            }

            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = _passwordHasherService.HashPassword(request.Password),
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return new AuthResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Token = string.Empty
            };
        }

        public async Task<bool> ChangePasswordAsync(
            int userId,
            ChangePasswordRequest request)
        {
            var user = await _dbContext.Users.FindAsync(userId);

            if (user is null)
            {
                return false;
            }

            var currentPasswordIsValid = _passwordHasherService.VerifyPassword(
                request.CurrentPassword,
                user.PasswordHash);

            if (!currentPasswordIsValid)
            {
                throw new ArgumentException(ErrorMessages.CurrentPasswordIncorrect);
            }

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                throw new ArgumentException(ErrorMessages.NewPasswordMismatch);
            }

            var newPasswordMatchesCurrent = _passwordHasherService.VerifyPassword(
                request.NewPassword,
                user.PasswordHash);

            if (newPasswordMatchesCurrent)
            {
                throw new ArgumentException(ErrorMessages.NewPasswordMustBeDifferent);
            }

            user.PasswordHash = _passwordHasherService.HashPassword(request.NewPassword);

            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
