using JobTrackr.Application.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace JobTrackr.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _taskService.GetAll();

            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _taskService.GetById(id);

            if (response is null)
            {
                return NotFound("Task is not found");
            }

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTaskRequest request)
        {
            try
            {
                var response = await _taskService.CreateTask(request);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTaskRequest request)
        {
            try
            {
                var response = await _taskService.UpdateTask(id, request);

                if (response is null)
                {
                    return NotFound("Task is not found");
                }

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var isDeleted = await _taskService.DeleteTask(id);

        if (!isDeleted)
        {
            return NotFound("Task is not found");
        }

        return NoContent();
    }
}
}
