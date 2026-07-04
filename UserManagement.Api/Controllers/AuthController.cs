using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Common;
using UserManagement.Api.Dtos;
using UserManagement.Api.Models;
using UserManagement.Api.Services;

namespace UserManagement.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public AuthController(UserManager<User> userManager, ITokenService tokenService, IMapper mapper, IConfiguration config)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Unauthorized(ApiResponse<AuthResponseDto>.FailureResponse("Invalid email or password."));
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

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "Login successful."));
        }
    }
}
