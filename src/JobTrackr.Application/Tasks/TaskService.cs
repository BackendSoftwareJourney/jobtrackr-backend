using JobTrackr.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
