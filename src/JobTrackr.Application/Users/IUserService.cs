namespace JobTrackr.Application.Users
{
    public interface IUserService
    {
        Task<UserResponse> CreateUser(CreateUserRequest request);

        Task<List<UserResponse>> GetAll();

        Task<UserResponse?> GetById(int id);

        Task<UserResponse?> UpdateUser(int id, UpdateUserRequest request);

        Task<bool> DeleteUser(int id);
    }
}
