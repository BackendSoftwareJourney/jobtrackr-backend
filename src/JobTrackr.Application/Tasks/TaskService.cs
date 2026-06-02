using JobTrackr.Application.Users;

namespace JobTrackr.Application.Tasks
{
    public class TaskService : ITaskService
    {
        private readonly List<TaskResponse> _tasks = [];
        private int _nextId = 1;
        private readonly IUserService _userService;

        public TaskService(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<TaskResponse> CreateTask(CreateTaskRequest request)
        {
            if (request.UserId <= 0)
            {
                throw new ArgumentException("UserId is required.");
            }

            var user = await _userService.GetById(request.UserId);

            if (user is null)
            {
                throw new ArgumentException("User not found.");
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException("Task title is required.");
            }

            TaskResponse response = new TaskResponse()
            {
                Id = _nextId,
                Description = request.Description,
                Title = request.Title,
                CreatedAtUtc = DateTime.UtcNow,
                IsCompleted = false,
                UserId = request.UserId
            };

            _nextId += 1;
            _tasks.Add(response);
            await Task.Yield();

            return response;
        }

        public async Task<List<TaskResponse>> GetAll(bool? isCompleted, string? search)
        {
            await Task.Yield();

            var query = _tasks.AsEnumerable();

            if (isCompleted is not null)
            {
                query = query.Where(x => x.IsCompleted == isCompleted);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Title.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            return query.ToList();
        }

        public async Task<TaskResponse?> GetById(int id)
        {
            await Task.Yield();

            return _tasks.Find(x => x.Id == id);
        }

        public async Task<TaskResponse?> UpdateTask(int id, UpdateTaskRequest request)
        {
            await Task.Yield();

            var task = _tasks.Find(x => x.Id == id);

            if (task is null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException("Task title is required.");
            }

            task.Title = request.Title;
            task.Description = request.Description;

            return task;
        }

        public async Task<bool> DeleteTask(int id)
        {
            await Task.Yield();

            var task = _tasks.Find(x => x.Id == id);

            if (task is null)
            {
                return false;
            }

            _tasks.Remove(task);

            return true;
        }

        public async Task<TaskResponse?> CompleteTask(int id)
        {
            await Task.Yield();

            var task = _tasks.Find(x => x.Id == id);

            if (task is null)
            {
                return null;
            }

            task.IsCompleted = true;

            return task;
        }

        public async Task<TaskResponse?> ReopenTask(int id)
        {
            await Task.Yield();

            var task = _tasks.Find(x => x.Id == id);

            if (task is null)
            {
                return null;
            }

            task.IsCompleted = false;

            return task;
        }
    }
}
