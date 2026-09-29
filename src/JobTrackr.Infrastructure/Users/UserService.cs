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

        public async Task<UserResponse> CreateAsync(CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new ArgumentException(ErrorMessages.UserFullNameRequired);
            }

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

            var user = new User()
            {
                FullName = request.FullName,
                Email = normalizedEmail,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return MapToResponse(user);
        }

        public async Task<List<UserResponse>> GetAllAsync()
        {
            return await ProjectToResponses(_dbContext.Users)
                .ToListAsync();
        }

        public async Task<UserResponse?> GetByIdAsync(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);

            if (user is null)
            {
                return null;
            }

            return MapToResponse(user);
        }

        public async Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new ArgumentException(ErrorMessages.UserFullNameRequired);
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException(ErrorMessages.UserEmailRequired);
            }

            if (!EmailValidation.IsValid(request.Email))
            {
                throw new ArgumentException(ErrorMessages.EmailInvalid);
            }

            var normalizedEmail = EmailValidation.Normalize(request.Email);

            var user = await _dbContext.Users.FindAsync(id);

            if (user is null)
            {
                return null;
            }

            var emailExists = await _dbContext.Users.AnyAsync(
                existingUser => existingUser.Id != id &&
                    existingUser.Email.ToLower() == normalizedEmail);

            if (emailExists)
            {
                throw new ArgumentException(ErrorMessages.EmailAlreadyInUse);
            }

            user.FullName = request.FullName;
            user.Email = normalizedEmail;

            await _dbContext.SaveChangesAsync();

            return MapToResponse(user);
        }

        public async Task<bool> DeleteAsync(int id)
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

        private static IQueryable<UserResponse> ProjectToResponses(IQueryable<User> query)
        {
            return query.Select(user => new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                CreatedAtUtc = user.CreatedAtUtc
            });
        }

        private static UserResponse MapToResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                CreatedAtUtc = user.CreatedAtUtc
            };
        }
    }
}
