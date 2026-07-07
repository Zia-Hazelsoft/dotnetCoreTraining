using System.Net;
using System.Text.Json;

namespace UserManagement.Api.Common
{
    public class ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            ApiResponse<object> response;

            switch (exception)
            {
                case ApplicationValidationException validationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    response = ApiResponse<object>.FailureResponse(
                        validationException.Message,
                        validationException.Errors);

                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    response = ApiResponse<object>.FailureResponse(
                    "An unexpected error occurred on the server.");

                    break;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(response, options);

            return context.Response.WriteAsync(json);
        }
    }
}