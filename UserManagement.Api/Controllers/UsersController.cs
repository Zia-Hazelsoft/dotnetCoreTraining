using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Common;
using UserManagement.Api.Dtos;
using UserManagement.Api.Services;

namespace UserManagement.Api.Controllers
{
    /// <summary>
    /// Exposes CRUD endpoints for User entities.
    /// All business logic lives in IUserService — this controller only
    /// handles HTTP concerns (routing, status codes, response shaping).
    /// </summary>
    [ApiController]
    [Route("api/v1/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        // The concrete UserService is injected automatically by the
        // DI container, based on the registration in Program.cs.
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Gets all users.
        /// </summary>
        [HttpGet]
        public ActionResult<ApiResponse<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = _userService.GetAllUsers();
            return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResponse(users));
        }

        /// <summary>
        /// Gets a single user by id.
        /// </summary>
        [HttpGet("{id:int}")]
        public ActionResult<ApiResponse<UserDto>> GetUserById(int id)
        {
            var user = _userService.GetUserById(id);

            if (user is null)
            {
                return NotFound(ApiResponse<UserDto>.FailureResponse($"User with id {id} was not found."));
            }

            return Ok(ApiResponse<UserDto>.SuccessResponse(user));
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        [HttpPost]
        public ActionResult<ApiResponse<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            var createdUser = _userService.CreateUser(createUserDto);

            return CreatedAtAction(
                nameof(GetUserById),
                new { id = createdUser.Id },
                ApiResponse<UserDto>.SuccessResponse(createdUser, "User created successfully."));
        }

        /// <summary>
        /// Updates an existing user by id.
        /// </summary>
        [HttpPut("{id:int}")]
        public ActionResult<ApiResponse<object>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            var wasUpdated = _userService.UpdateUser(id, updateUserDto);

            if (!wasUpdated)
            {
                return NotFound(ApiResponse<object>.FailureResponse($"User with id {id} was not found."));
            }

            return Ok(ApiResponse<object>.SuccessResponse(new { }, "User updated successfully."));
        }

        /// <summary>
        /// Deletes a user by id.
        /// </summary>
        [HttpDelete("{id:int}")]
        public ActionResult<ApiResponse<object>> DeleteUser(int id)
        {
            var wasDeleted = _userService.DeleteUser(id);

            if (!wasDeleted)
            {
                return NotFound(ApiResponse<object>.FailureResponse($"User with id {id} was not found."));
            }

            return Ok(ApiResponse<object>.SuccessResponse(new { }, "User deleted successfully."));
        }
    }
}