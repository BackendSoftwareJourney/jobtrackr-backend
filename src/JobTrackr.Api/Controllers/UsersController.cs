using JobTrackr.Application.Common;
using JobTrackr.Application.Tasks;
using JobTrackr.Application.Users;
using Microsoft.AspNetCore.Mvc;

namespace JobTrackr.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITaskService _taskService;

        public UsersController(IUserService userService, ITaskService taskService)
        {
            _userService = userService;
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAll();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetById(id);

            if (user is null)
            {
                return NotFound(ErrorMessages.UserNotFound);
            }

            return Ok(user);
        }

        [HttpGet("{userId}/tasks")]
        public async Task<IActionResult> GetTasksByUserId(int userId)
        {
            var tasks = await _taskService.GetByUserId(userId);

            if (tasks is null)
            {
                return NotFound(ErrorMessages.UserNotFound);
            }

            return Ok(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserRequest request)
        {
            try
            {
                var response = await _userService.CreateUser(request);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateUserRequest request)
        {
            try
            {
                var response = await _userService.UpdateUser(id, request);

                if (response is null)
                {
                    return NotFound(ErrorMessages.UserNotFound);
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
            var isDeleted = await _userService.DeleteUser(id);

            if (!isDeleted)
            {
                return NotFound(ErrorMessages.UserNotFound);
            }

            return NoContent();
        }
    }
}
