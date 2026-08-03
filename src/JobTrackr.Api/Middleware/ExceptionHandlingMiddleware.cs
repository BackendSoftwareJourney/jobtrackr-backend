using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace JobTrackr.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while processing the request.");

                var problemDetails = new ProblemDetails()
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred."
                };
                context.Response.StatusCode = problemDetails.Status.Value;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}
