using JobTrackr.Application.Common;
using JobTrackr.Application.Users;
using JobTrackr.Domain.Entities;
using JobTrackr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobTrackr.Infrastructure.Users
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new ArgumentException(ErrorMessages.UserFullNameRequired);
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException(ErrorMessages.UserEmailRequired);
            }

            var user = new User()
            {
                FullName = request.FullName,
                Email = request.Email,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                CreatedAtUtc = user.CreatedAtUtc
            };
        }

        public async Task<List<UserResponse>> GetAllAsync()
        {
            return await _dbContext.Users
                .Select(user => new UserResponse
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    CreatedAtUtc = user.CreatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<UserResponse?> GetByIdAsync(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);

            if (user is null)
            {
                return null;
            }

            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                CreatedAtUtc = user.CreatedAtUtc
            };
        }

        public async Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new ArgumentException(ErrorMessages.UserFullNameRequired);
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException(ErrorMessages.UserEmailRequired);
            }

            var user = await _dbContext.Users.FindAsync(id);

            if (user is null)
            {
                return null;
            }

            user.FullName = request.FullName;
            user.Email = request.Email;

            await _dbContext.SaveChangesAsync();

            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                CreatedAtUtc = user.CreatedAtUtc
            };
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);

            if (user is null)
            {
                return false;
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
