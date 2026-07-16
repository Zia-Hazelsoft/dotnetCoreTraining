using AutoMapper;
using Microsoft.AspNetCore.Identity;
using UserManagement.Api.Constants;
using UserManagement.Api.Dtos;
using UserManagement.Api.Models;
using UserManagement.Api.Services.TokenService;

namespace UserManagement.Api.Services.AuthService.Implementation
{
    /// <summary>
    /// Implements authentication business logic, wrapping login credential verification 
    /// and account confirmation flow utilizing ASP.NET Core Identity.
    /// </summary>
    public class AuthService(
        UserManager<User> userManager,
        ITokenService tokenService,
        IMapper mapper) : IAuthService
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Validates login credentials and ensures the user's email is confirmed before generating a JWT token.
        /// </summary>
        /// <param name="loginDto">The email and password payload.</param>
        /// <returns>An <see cref="AuthResponseDto"/> if login succeeds; null if credentials are invalid.</returns>
        /// <exception cref="ArgumentException">Thrown when the user's email has not been confirmed.</exception>
        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            User? user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                throw new ArgumentException(Messages.Error.InvalidCredentials);
            }

            if (!user.EmailConfirmed)
            {
                throw new ArgumentException(Messages.Error.EmailNotConfirmed);
            }

            string token = _tokenService.GenerateToken(user);
            UserDto userDto = _mapper.Map<UserDto>(user);

            return new AuthResponseDto
            {
                Token = token,
                User = userDto
            };
        }

        /// <summary>
        /// Confirms a user's registration by validating their email confirmation token and setting their password.
        /// </summary>
        /// <param name="confirmRegistrationDto">The email, token, and new password payload.</param>
        /// <exception cref="ArgumentException">Thrown if user is not found, email is already verified, or token validation/password hashing fails.</exception>
        public async Task ConfirmRegistrationAsync(ConfirmRegistrationDto confirmRegistrationDto)
        {
            User? user = await _userManager.FindByEmailAsync(confirmRegistrationDto.Email) 
                ?? throw new ArgumentException(Messages.Error.UserNotFound);

            if (user.EmailConfirmed)
            {
                throw new ArgumentException(Messages.Error.EmailAlreadyConfirmed);
            }

            IdentityResult confirmResult = await _userManager.ConfirmEmailAsync(user, confirmRegistrationDto.Token);
            if (!confirmResult.Succeeded)
            {
                string errors = string.Join(", ", confirmResult.Errors.Select(e => e.Description));
                throw new ArgumentException(errors);
            }

            IdentityResult passwordResult = await _userManager.AddPasswordAsync(user, confirmRegistrationDto.Password);
            if (!passwordResult.Succeeded)
            {
                // Roll back email confirmation state if password set fails so user can try again
                user.EmailConfirmed = false;
                
                string errors = string.Join(", ", passwordResult.Errors.Select(e => e.Description));
                throw new ArgumentException(errors);
            }
        }
    }
}
