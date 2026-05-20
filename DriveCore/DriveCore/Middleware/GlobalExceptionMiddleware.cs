using DriveCore.Dtos.Response;
using System.Text.Json;

namespace DriveCore.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unhandled exception occurred while processing request {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

                if (context.Response.HasStarted)
                {
                    throw;
                }

                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = GetStatusCode(exception);

                var response = new ErrorResponse
                {
                    Message = GetMessage(exception),
                    Errors = GetErrors(exception)
                };

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
        }

        private int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                BadHttpRequestException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                ArgumentException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        private string GetMessage(Exception exception)
        {
            if (_environment.IsDevelopment())
            {
                return exception.Message;
            }

            return GetStatusCode(exception) == StatusCodes.Status500InternalServerError
            ? "An unexpected error occurred."
            : exception.Message;
        }

        private IEnumerable<string> GetErrors(Exception exception)
        {
            if (!_environment.IsDevelopment() || exception.InnerException is null)
            {
                return Array.Empty<string>();
            }

            return new[] { exception.InnerException.Message };
        }
    }
}