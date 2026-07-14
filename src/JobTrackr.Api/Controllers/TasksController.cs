using JobTrackr.Application.Common;
using JobTrackr.Application.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobTrackr.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(bool? isCompleted, string? search, int? userId)
        {
            var tasks = await _taskService.GetAllAsync(isCompleted, search, userId);

            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _taskService.GetByIdAsync(id);

            if (response is null)
            {
                return NotFound(ErrorMessages.TaskNotFound);
            }

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTaskRequest request)
        {
            try
            {
                var response = await _taskService.CreateTaskAsync(request);

                return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
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
                var response = await _taskService.UpdateTaskAsync(id, request);

                if (response is null)
                {
                    return NotFound(ErrorMessages.TaskNotFound);
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
            var isDeleted = await _taskService.DeleteTaskAsync(id);

            if (!isDeleted)
            {
                return NotFound(ErrorMessages.TaskNotFound);
            }

            return NoContent();
        }

        [HttpPatch("{id}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            var response = await _taskService.CompleteTaskAsync(id);

            if (response is null)
            {
                return NotFound(ErrorMessages.TaskNotFound);
            }

            return Ok(response);
        }

        [HttpPatch("{id}/reopen")]
        public async Task<IActionResult> Reopen(int id)
        {
            var response = await _taskService.ReopenTaskAsync(id);

            if (response is null)
            {
                return NotFound(ErrorMessages.TaskNotFound);
            }

            return Ok(response);
        }

        private int? GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdValue, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}
