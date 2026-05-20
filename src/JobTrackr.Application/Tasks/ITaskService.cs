using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobTrackr.Application.Tasks
{
    public interface ITaskService
    {
        Task<List<TaskResponse>> GetAll();

        Task<TaskResponse?> GetById(int id);

        Task<TaskResponse> CreateTask(CreateTaskRequest request);

        Task<TaskResponse?> UpdateTask(int id, UpdateTaskRequest request);

        Task<bool> DeleteTask(int id);
    }
}
