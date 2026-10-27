using JobTrackr.Application.Comments;
using JobTrackr.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobTrackr.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/tasks/{taskId:int}/comments")]
    public class TaskCommentsController : ControllerBase
    {
        private readonly ITaskCommentService _taskCommentService;

        public TaskCommentsController(ITaskCommentService taskCommentService)
        {
            _taskCommentService = taskCommentService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            int taskId,
            CreateTaskCommentRequest request)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId is null)
            {
                return Unauthorized();
            }

            try
            {
                var response = await _taskCommentService.CreateAsync(
                    taskId,
                    request,
                    currentUserId.Value);

                if (response is null)
                {
                    return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Not Found",
                        detail: ErrorMessages.TaskNotFound);
                }

                var location = $"{Request.PathBase}/api/tasks/{taskId}/comments";

                return Created(location, response);
            }
            catch (ArgumentException ex)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request",
                    detail: ex.Message);
            }
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