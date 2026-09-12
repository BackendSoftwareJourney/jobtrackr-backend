namespace JobTrackr.Application.Users
{
    public interface IUserService
    {
        Task<UserResponse> CreateAsync(CreateUserRequest request);

        Task<List<UserResponse>> GetAllAsync();

        Task<UserResponse?> GetByIdAsync(int id);

        Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request);

        Task<bool> DeleteAsync(int id);
    }
}
