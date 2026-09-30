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
            var normalizedEmail = EmailValidation.Normalize(request.Email);

            var user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Email.ToLower() == normalizedEmail);

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

            return MapToResponse(user, token);
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            ValidatePassword(request.Password);

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException(ErrorMessages.UserEmailRequired);
            }

            if (!EmailValidation.IsValid(request.Email))
            {
                throw new ArgumentException(ErrorMessages.EmailInvalid);
            }

            var normalizedEmail = EmailValidation.Normalize(request.Email);
            var emailExists = await _dbContext.Users.AnyAsync(
                user => user.Email.ToLower() == normalizedEmail);

            if (emailExists)
            {
                throw new ArgumentException(ErrorMessages.EmailAlreadyInUse);
            }

            var user = new User
            {
                Email = normalizedEmail,
                FullName = request.FullName,
                PasswordHash = _passwordHasherService.HashPassword(request.Password),
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return MapToResponse(user, string.Empty);
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

            ValidatePassword(request.NewPassword);

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

        private static void ValidatePassword(string password)
        {
            if (!PasswordValidation.HasMinimumLength(password))
            {
                throw new ArgumentException(ErrorMessages.PasswordMinimumLength);
            }

            if (!PasswordValidation.HasBasicComplexity(password))
            {
                throw new ArgumentException(ErrorMessages.PasswordComplexity);
            }
        }
        private static AuthResponse MapToResponse(User user, string token)
        {
            return new AuthResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Token = token
            };
        }
    }
}
