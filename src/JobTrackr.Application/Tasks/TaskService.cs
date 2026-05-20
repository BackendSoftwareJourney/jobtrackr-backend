using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobTrackr.Application.Tasks
{
    public class TaskService : ITaskService
    {
        private readonly List<TaskResponse> _tasks = [];
        private int _nextId = 1;

        public async Task<TaskResponse> CreateTask(CreateTaskRequest request)
        {
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
                IsCompleted = false
            };

        _nextId += 1;
        _tasks.Add(response);
        await Task.Yield();

        return response;
    }

    public async Task<List<TaskResponse>> GetAll()
        {
            await Task.Yield();

            return _tasks.ToList();
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
}
}
