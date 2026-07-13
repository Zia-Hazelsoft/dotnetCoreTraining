using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using UserManagement.Api.Common;
using UserManagement.Api.Constants;
using UserManagement.Api.Dtos;
using UserManagement.Api.Services.Interfaces;

namespace UserManagement.Api.Controllers
{
    /// <summary>
    /// Handles user authentication, token generation, and account activation/password setup flows.
    /// </summary>
    [ApiController]
    [Route(ApiRoutes.Auth)]
    public class AuthController(IAuthService authService, ILogger<AuthController> logger) : BaseApiController
    {
        private readonly IAuthService _authService = authService;
        private readonly ILogger<AuthController> _logger = logger;

        /// <summary>
        /// Authenticates a user using their email and password, returning a JWT token if successful.
        /// </summary>
        /// <param name="loginDto">The login credentials.</param>
        /// <returns>An <see cref="AuthResponseDto"/> containing the JWT token, or Unauthorized if authentication fails.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                AuthResponseDto? authResult = await _authService.LoginAsync(loginDto);
                if (authResult == null)
                {
                    return Unauthorized(Messages.Error.InvalidCredentials);
                }

                return Ok(authResult, Messages.Success.LoginSuccessful);
            }
            catch (ApplicationValidationException ex)
            {
                return BadRequest(ex.Message, ex.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for user: {Email}", loginDto.Email);
                return InternalServerError(Messages.Error.Unexpected);
            }
        }

        /// <summary>
        /// Confirms email registration and sets the user's password using the generated token.
        /// </summary>
        /// <param name="confirmDto">The email, token, and chosen password.</param>
        /// <returns>Ok if confirmation and password set succeeded; BadRequest otherwise.</returns>
        [HttpPost("confirm-registration")]
        public async Task<IActionResult> ConfirmRegistration([FromBody] ConfirmRegistrationDto confirmDto)
        {
            try
            {
                await _authService.ConfirmRegistrationAsync(confirmDto);
                return Ok(Messages.Success.RegistrationConfirmed);
            }
            catch (ApplicationValidationException ex)
            {
                return BadRequest(ex.Message, ex.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during registration confirmation for user: {Email}", confirmDto.Email);
                return InternalServerError(Messages.Error.Unexpected);
            }
        }

        /// <summary>
        /// Serves the account activation HTML page to the user in their browser when they click the email link.
        /// </summary>
        /// <param name="email">The user email address.</param>
        /// <param name="token">The email confirmation token.</param>
        /// <returns>The rendered HTML password setup page.</returns>
        [HttpGet("confirm-registration")]
        public async Task<IActionResult> GetConfirmRegistrationPage([FromQuery] string email, [FromQuery] string token)
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "ActivationTemplate.html");
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Activation template not found.");
                }

                string html = await System.IO.File.ReadAllTextAsync(filePath);
                
                // Replace placeholders safely
                html = html.Replace("{email}", email)
                           .Replace("{token}", Uri.EscapeDataString(token));
                
                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading activation page for {Email}", email);
                return InternalServerError(Messages.Error.Unexpected);
            }
        }
    }
}
