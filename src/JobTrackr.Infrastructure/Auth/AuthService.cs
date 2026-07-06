using JobTrackr.Application.Auth;
using JobTrackr.Domain.Entities;
using JobTrackr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Infrastructure.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly IPasswordHasherService _passwordHasherService;

        public AuthService(AppDbContext dbContext, IPasswordHasherService passwordHasherService)
        {
            _dbContext = dbContext;
            _passwordHasherService = passwordHasherService;
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
    }
}
