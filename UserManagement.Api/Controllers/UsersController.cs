using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UserManagement.Api.Common;
using UserManagement.Api.Constants;
using UserManagement.Api.Dtos;
using UserManagement.Api.Services.UserService;

namespace UserManagement.Api.Controllers
{
    /// <summary>
    /// Exposes CRUD endpoints for User entities.
    /// All business logic lives in IUserService — this controller only
    /// handles HTTP concerns (routing, status codes, response shaping).
    /// </summary>
    [Authorize]
    [ApiController]
    [Route(ApiRoutes.Users)]
    public class UsersController(IUserService userService, ILogger<UsersController> logger) : BaseApiController
    {
        private readonly IUserService _userService = userService;
        private readonly ILogger<UsersController> _logger = logger;

        /// <summary>
        /// Retrieves a paginated list of users filtered by search term and sorted.
        /// </summary>
        /// <param name="userParameters">The search, pagination, and sorting parameters.</param>
        /// <returns>A paginated response containing matching users.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserParameters userParameters)
        {
            try
            {
                PaginatedResponseDto<UserDto> usersResult = await _userService.GetUsersAsync(userParameters);
                return Ok(usersResult, Messages.Success.RequestSuccessful);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all users with parameters: {@Params}", userParameters);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>The user details if found; otherwise, NotFound.</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                UserDto? user = await _userService.GetUserByIdAsync(id);

                if (user is null)
                {
                    return NotFound(string.Format(Messages.Error.UserNotFound, id));
                }

                return Ok(user, Messages.Success.RequestSuccessful);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting user by id: {Id}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new user without a password and sends a confirmation email.
        /// </summary>
        /// <param name="createUserDto">The details of the user to create.</param>
        /// <returns>The created user.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                CreateUserResponseDto response = await _userService.CreateUserAsync(createUserDto);

                return Ok(response, Messages.Success.UserCreatedWithEmailLink);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating user with email: {Email}", createUserDto.Email);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates the profile details (name, email) of an existing user.
        /// </summary>
        /// <param name="id">The unique identifier of the user to update.</param>
        /// <param name="updateUserDto">The updated details.</param>
        /// <returns>Ok if update succeeded; BadRequest or NotFound otherwise.</returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                bool wasUpdated = await _userService.UpdateUserAsync(id, updateUserDto);

                if (!wasUpdated)
                {
                    return NotFound(string.Format(Messages.Error.UserNotFound, id));
                }

                return Ok(Messages.Success.UserUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user: {Id}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete.</param>
        /// <returns>Ok if deleted; NotFound otherwise.</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                bool wasDeleted = await _userService.DeleteUserAsync(id);

                if (!wasDeleted)
                {
                    return NotFound(string.Format(Messages.Error.UserNotFound, id));
                }

                return Ok(Messages.Success.UserDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting user: {Id}", id);
                return BadRequest(ex.Message);
            }
        }
    }
}