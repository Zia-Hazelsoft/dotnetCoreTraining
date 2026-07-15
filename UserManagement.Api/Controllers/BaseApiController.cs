using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;
using UserManagement.Api.Common;

namespace UserManagement.Api.Controllers
{
    /// <summary>
    /// An abstract base controller that wraps HTTP helper methods (Ok, NotFound, BadRequest, etc.) 
    /// in our generic <see cref="ApiResponse{T}"/> wrapper to standardize API responses.
    /// </summary>
    public abstract class BaseApiController : ControllerBase
    {
        // ---- Strongly-typed helpers (preferred: use these from your controllers) ----

        /// <summary>
        /// Returns 200 OK with a strongly-typed ApiResponse&lt;T&gt; envelope.
        /// Preferred over the object-based Ok(...) overloads when you know T,
        /// since it preserves the generic type for Swagger/OpenAPI schemas.
        /// </summary>
        [NonAction]
        public OkObjectResult Success<T>(T data, string message = "Request successful")
        {
            return base.Ok(ApiResponse<T>.SuccessResponse(data, message));
        }

        /// <summary>
        /// Returns a given status code with a strongly-typed ApiResponse&lt;T&gt; success envelope.
        /// </summary>
        [NonAction]
        public ObjectResult Success<T>(int statusCode, T data, string message = "Request successful")
        {
            return base.StatusCode(statusCode, ApiResponse<T>.SuccessResponse(data, message));
        }

        /// <summary>
        /// Returns a failure envelope (ApiResponse&lt;T&gt;) with the given status code.
        /// Use the generic parameter to match whatever Data type the endpoint declares,
        /// or default to object when there's no natural payload type.
        /// </summary>
        [NonAction]
        public ObjectResult Failure<T>(int statusCode, string message, List<string>? errors = null)
        {
            return base.StatusCode(statusCode, ApiResponse<T>.FailureResponse(message, errors));
        }

        // ---- Overrides of ControllerBase's non-generic helpers ----
        // These can't be made generic (they override base signatures), so they
        // fall back to ApiResponse<object>. No reflection needed either way,
        // since the incoming value is already just `object?`.

        [NonAction]
        public override OkObjectResult Ok(object? value) => Ok(value, "Request successful");

        [NonAction]
        public OkObjectResult Ok(object? value, string message)
        {
            if (value is ApiResponse<object> alreadyWrapped)
                return base.Ok(alreadyWrapped);

            return base.Ok(ApiResponse<object>.SuccessResponse(value!, message));
        }

        [NonAction]
        public OkObjectResult Ok(string message) => Ok(null, message);

        [NonAction]
        public override NotFoundObjectResult NotFound(object? value)
        {
            var message = value as string ?? value?.ToString() ?? "Not Found";
            return base.NotFound(ApiResponse<object>.FailureResponse(message));
        }

        [NonAction]
        public BadRequestObjectResult BadRequest(string message, List<string>? errors = null)
        {
            return base.BadRequest(ApiResponse<object>.FailureResponse(message, errors));
        }

        [NonAction]
        public override BadRequestObjectResult BadRequest(ModelStateDictionary modelState)
        {
            var errors = modelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return base.BadRequest(ApiResponse<object>.FailureResponse("Validation failed", errors));
        }

        [NonAction]
        public UnauthorizedObjectResult Unauthorized(string message)
        {
            return base.Unauthorized(ApiResponse<object>.FailureResponse(message));
        }

        [NonAction]
        public override ObjectResult StatusCode(int statusCode, object? value)
        {
            if (value is ApiResponse<object> alreadyWrapped)
                return base.StatusCode(statusCode, alreadyWrapped);

            object wrapped = statusCode >= 400
                ? ApiResponse<object>.FailureResponse(value as string ?? value?.ToString() ?? "Error occurred")
                : ApiResponse<object>.SuccessResponse(value!, "Request successful");

            return base.StatusCode(statusCode, wrapped);
        }
    }
}