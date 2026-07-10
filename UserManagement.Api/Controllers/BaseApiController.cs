using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
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
        /// <summary>
        /// Wraps the value in a success response envelope with a default message.
        /// </summary>
        /// <param name="value">The payload value to return.</param>
        /// <returns>An <see cref="OkObjectResult"/> containing the wrapped payload.</returns>
        [NonAction]
        public override OkObjectResult Ok(object? value)
        {
            if (value is null)
                return base.Ok(ApiResponse<object>.SuccessResponse(null!));

            var type = value.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResponse<>))
                return base.Ok(value);

            var responseType = typeof(ApiResponse<>).MakeGenericType(type);
            var successMethod = responseType.GetMethod("SuccessResponse", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var wrappedResponse = successMethod?.Invoke(null, [value, "Request successful"]);

            return base.Ok(wrappedResponse);
        }

        /// <summary>
        /// Wraps the value in a success response envelope with a custom message.
        /// </summary>
        /// <param name="value">The payload value to return.</param>
        /// <param name="message">The custom success message.</param>
        /// <returns>An <see cref="OkObjectResult"/> containing the wrapped payload.</returns>
        [NonAction]
        public OkObjectResult Ok(object? value, string message)
        {
            if (value is null)
                return base.Ok(ApiResponse<object>.SuccessResponse(null!, message));

            var type = value.GetType();
            var responseType = typeof(ApiResponse<>).MakeGenericType(type);
            var successMethod = responseType.GetMethod("SuccessResponse", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var wrappedResponse = successMethod?.Invoke(null, [value, message]);

            return base.Ok(wrappedResponse);
        }

        /// <summary>
        /// Wraps a null payload in a success response envelope with a custom message.
        /// </summary>
        /// <param name="message">The custom success message.</param>
        /// <returns>An <see cref="OkObjectResult"/> containing the message.</returns>
        [NonAction]
        public OkObjectResult Ok(string message)
        {
            return base.Ok(ApiResponse<object>.SuccessResponse(null!, message));
        }

        /// <summary>
        /// Wraps the created payload in a success response envelope with a custom message.
        /// </summary>
        /// <param name="actionName">The name of the action to use for generating the URL.</param>
        /// <param name="routeValues">The route parameters.</param>
        /// <param name="value">The payload value to return.</param>
        /// <param name="message">The custom message.</param>
        /// <returns>A <see cref="CreatedAtActionResult"/> containing the wrapped payload.</returns>
        [NonAction]
        public CreatedAtActionResult CreatedAtAction(string actionName, object routeValues, object? value, string message)
        {
            if (value is null)
                return base.CreatedAtAction(actionName, routeValues, ApiResponse<object>.SuccessResponse(null!, message));

            var type = value.GetType();
            var responseType = typeof(ApiResponse<>).MakeGenericType(type);
            var successMethod = responseType.GetMethod("SuccessResponse", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var wrappedResponse = successMethod?.Invoke(null, [value, message]);

            return base.CreatedAtAction(actionName, routeValues, wrappedResponse);
        }

        /// <summary>
        /// Wraps a NotFound message in a failure response envelope.
        /// </summary>
        /// <param name="value">The message or value describing the not found resource.</param>
        /// <returns>A <see cref="NotFoundObjectResult"/> containing the failure wrapper.</returns>
        [NonAction]
        public override NotFoundObjectResult NotFound(object? value)
        {
            if (value is string message)
                return base.NotFound(ApiResponse<object>.FailureResponse(message));

            return base.NotFound(ApiResponse<object>.FailureResponse(value?.ToString() ?? "Not Found"));
        }

        /// <summary>
        /// Wraps a BadRequest error in a failure response envelope with validation messages.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="errors">The list of validation errors.</param>
        /// <returns>A <see cref="BadRequestObjectResult"/> containing the failure wrapper.</returns>
        [NonAction]
        public BadRequestObjectResult BadRequest(string message, List<string>? errors = null)
        {
            return base.BadRequest(ApiResponse<object>.FailureResponse(message, errors));
        }

        /// <summary>
        /// Wraps model state validation errors in a failure response envelope.
        /// </summary>
        /// <param name="modelState">The invalid model state.</param>
        /// <returns>A <see cref="BadRequestObjectResult"/> containing the validation messages.</returns>
        [NonAction]
        public override BadRequestObjectResult BadRequest(ModelStateDictionary modelState)
        {
            var errors = modelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return base.BadRequest(ApiResponse<object>.FailureResponse("Validation failed", errors));
        }

        /// <summary>
        /// Wraps an Unauthorized status message in a failure response envelope.
        /// </summary>
        /// <param name="message">The unauthorized access message.</param>
        /// <returns>An <see cref="UnauthorizedObjectResult"/> containing the failure wrapper.</returns>
        [NonAction]
        public UnauthorizedObjectResult Unauthorized(string message)
        {
            return base.Unauthorized(ApiResponse<object>.FailureResponse(message));
        }

        /// <summary>
        /// Wraps an internal server error message in a failure response envelope.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>An <see cref="ObjectResult"/> containing the failure wrapper with a 500 status code.</returns>
        [NonAction]
        protected ObjectResult InternalServerError(string message)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.FailureResponse(message));
        }
    }
}
