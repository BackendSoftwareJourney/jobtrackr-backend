namespace JobTrackr.Application.Users
{
    public class UserService : IUserService
    {
        private readonly List<UserResponse> _users = [];
        private int _nextId = 1;

        public async Task<UserResponse> CreateUser(CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new ArgumentException("User full name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException("User email is required.");
            }

            UserResponse response = new()
            {
                Id = _nextId,
                FullName = request.FullName,
                Email = request.Email,
                CreatedAtUtc = DateTime.UtcNow
            };

            _nextId += 1;
            _users.Add(response);
            await Task.Yield();

            return response;
        }

        public async Task<List<UserResponse>> GetAll()
        {
            await Task.Yield();

            return _users.ToList();
        }
    }
}
