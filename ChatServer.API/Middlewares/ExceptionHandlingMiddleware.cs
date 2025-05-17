using FluentValidation;

namespace ChatServer.API.Middlewares
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

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                //Log the exception type and message
                _logger.LogError($"{ex.GetType().ToString()}: {ex.Message}");

                if (ex.InnerException is not null)
                {
                    //Log the inner exception type and message
                    _logger.LogError($"{ex.InnerException.GetType().ToString()}: {ex.InnerException.Message}");
                }


                // Set appropriate status code based on exception type
                httpContext.Response.StatusCode = ex switch
                {
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    ValidationException => StatusCodes.Status400BadRequest,
                    InvalidOperationException => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status500InternalServerError // Default to 500 for other exceptions
                };
                await httpContext.Response.WriteAsJsonAsync(new { Message = ex.Message, Type = ex.GetType().ToString() });
            }

        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
