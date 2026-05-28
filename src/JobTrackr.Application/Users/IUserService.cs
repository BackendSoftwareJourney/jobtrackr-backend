namespace JobTrackr.Application.Users
{
    public interface IUserService
    {
        Task<UserResponse> CreateUser(CreateUserRequest request);

        Task<List<UserResponse>> GetAll();
    }
}
