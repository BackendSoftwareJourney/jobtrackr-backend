using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobTrackr.Application.Tasks
{
    public  interface ITaskService
    {
        Task<List<TaskResponse>> GetAll();

        Task<TaskResponse?> GetById(int id);

        Task<TaskResponse> CreateTask(CreateTaskRequest  request);
    }
}
