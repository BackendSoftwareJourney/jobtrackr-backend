namespace JobTrackr.Application.Users
{
    public interface IUserService
    {
        Task<UserResponse> CreateUserAsync(CreateUserRequest request);

        Task<List<UserResponse>> GetAllAsync();

        Task<UserResponse?> GetByIdAsync(int id);

        Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request);

        Task<bool> DeleteUserAsync(int id);
    }
}
