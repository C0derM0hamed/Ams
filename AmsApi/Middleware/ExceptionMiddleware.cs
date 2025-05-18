using System.Text.Json;
using System.Net;

namespace AmsApi.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Get inner exception if available
                var innerMessage = ex.InnerException?.Message;
                var fullMessage = string.IsNullOrEmpty(innerMessage)
                    ? ex.Message
                    : $"{ex.Message} | Inner: {innerMessage}";

                var response = _env.IsDevelopment()
                    ? new ApiResponse(context.Response.StatusCode, fullMessage, ex.StackTrace?.ToString())
                    : new ApiResponse(context.Response.StatusCode, "Internal Server Error");

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}
