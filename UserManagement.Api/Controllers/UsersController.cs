using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Common;
using UserManagement.Api.Constants;
using UserManagement.Api.Dtos;
using UserManagement.Api.Services;

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
    public class UsersController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponseDto<UserDto>>>> GetAllUsers([FromQuery] UserParameters userParameters)
        {
            var usersResult = await _userService.GetUsersAsync(userParameters);
            return Ok(ApiResponse<PaginatedResponseDto<UserDto>>.SuccessResponse(usersResult, Messages.Success.RequestSuccessful));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user is null)
            {
                return NotFound(ApiResponse<UserDto>.FailureResponse(string.Format(Messages.Error.UserNotFound, id)));
            }

            return Ok(ApiResponse<UserDto>.SuccessResponse(user, Messages.Success.RequestSuccessful));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            var createdUser = await _userService.CreateUserAsync(createUserDto);

            return CreatedAtAction(
                nameof(GetUserById),
                new { id = createdUser.Id },
                ApiResponse<UserDto>.SuccessResponse(createdUser, Messages.Success.UserCreated));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            var wasUpdated = await _userService.UpdateUserAsync(id, updateUserDto);

            if (!wasUpdated)
            {
                return NotFound(ApiResponse<object>.FailureResponse(string.Format(Messages.Error.UserNotFound, id)));
            }

            return Ok(ApiResponse<object>.SuccessResponse(new { }, Messages.Success.UserUpdated));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
        {
            var wasDeleted = await _userService.DeleteUserAsync(id);

            if (!wasDeleted)
            {
                return NotFound(ApiResponse<object>.FailureResponse(string.Format(Messages.Error.UserNotFound, id)));
            }

            return Ok(ApiResponse<object>.SuccessResponse(new { }, Messages.Success.UserDeleted));
        }
    }
}