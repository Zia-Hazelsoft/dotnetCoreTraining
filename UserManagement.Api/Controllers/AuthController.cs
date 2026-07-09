using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Common;
using UserManagement.Api.Constants;
using UserManagement.Api.Dtos;
using UserManagement.Api.Models;
using UserManagement.Api.Services;

namespace UserManagement.Api.Controllers
{
    [ApiController]
    [Route(ApiRoutes.Auth)]
    public class AuthController(UserManager<User> userManager, ITokenService tokenService, IMapper mapper, IConfiguration config) : ControllerBase
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IMapper _mapper = mapper;
        private readonly IConfiguration _config = config;

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Unauthorized(ApiResponse<AuthResponseDto>.FailureResponse(Messages.Error.InvalidCredentials));
            }

            var token = _tokenService.GenerateToken(user);
            
            var expiryMinutes = double.Parse(_config["Jwt:ExpiryInMinutes"] ?? "60");
            var expiration = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var userDto = _mapper.Map<UserDto>(user);

            var authResponse = new AuthResponseDto
            {
                Token = token,
                Expiration = expiration,
                User = userDto
            };

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, Messages.Success.LoginSuccessful));
        }
    }
}
