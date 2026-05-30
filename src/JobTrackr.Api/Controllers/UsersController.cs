using JobTrackr.Application.Users;
using Microsoft.AspNetCore.Mvc;

namespace JobTrackr.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
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
                return NotFound("User not found.");
            }

            return Ok(user);
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
    }
}
