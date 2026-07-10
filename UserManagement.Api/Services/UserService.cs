using AutoMapper;
using Microsoft.AspNetCore.Identity;
using UserManagement.Api.Common;
using UserManagement.Api.Constants;
using UserManagement.Api.Dtos;
using UserManagement.Api.Models;
using UserManagement.Api.Repositories;
using UserManagement.Api.Services.Interfaces;

namespace UserManagement.Api.Services
{
    /// <summary>
    /// Implements User business logic, backed by the IRepositoryBase<User> and ASP.NET Core Identity.
    /// </summary>
    public class UserService(
        IRepositoryBase<User> userRepository,
        UserManager<User> userManager,
        IMapper mapper,
        ILogger<UserService> logger,
        IEmailSender emailSender,
        IHttpContextAccessor httpContextAccessor) : IUserService
    {
        private readonly IRepositoryBase<User> _userRepository = userRepository;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<UserService> _logger = logger;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<PaginatedResponseDto<UserDto>> GetUsersAsync(UserParameters userParameters)
        {
            var pagedUsers = await _userRepository.GetPagedAsync(
                userParameters.PageNumber,
                userParameters.PageSize,
                userParameters.SearchTerm,
                [nameof(User.FirstName), nameof(User.LastName), nameof(User.Email)],
                userParameters.OrderBy,
                nameof(User.Id),
                trackChanges: false);

            var userDtos = _mapper.Map<IEnumerable<UserDto>>(pagedUsers);

            return new PaginatedResponseDto<UserDto>
            {
                Items = userDtos,
                MetaData = pagedUsers.MetaData
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user is null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<CreateUserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var newUser = _mapper.Map<User>(createUserDto);

            var result = await _userManager.CreateAsync(newUser);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                throw new ApplicationValidationException(Messages.Error.UserCreationFailed, errors);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            
            // Generate link dynamically using the current request host and schema
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = request != null ? $"{request.Scheme}://{request.Host}" : "http://localhost:5067";
            var confirmationLink = $"{baseUrl}/api/v1/auth/confirm-registration?email={newUser.Email}&token={Uri.EscapeDataString(token)}";

            _logger.LogInformation("New user registration. Email: {Email}, Confirmation Link: {Link}", newUser.Email, confirmationLink);

            // Compose HTML Email Content
            var subject = "Welcome! Confirm your registration and set your password";
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                    <h2 style='color: #333;'>Welcome to the System, {newUser.FirstName}!</h2>
                    <p>An account has been created for you. To complete your registration and set your password, please click the button below:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{confirmationLink}' style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; font-weight: bold; border-radius: 5px; display: inline-block;'>Confirm Registration & Set Password</a>
                    </div>
                    <p style='color: #666; font-size: 12px;'>If the button above does not work, copy and paste this URL into your browser:</p>
                    <p style='color: #007bff; font-size: 12px; word-break: break-all;'>{confirmationLink}</p>
                </div>";

            // Send real email asynchronously in background task so API response doesn't hang
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailSender.SendEmailAsync(newUser.Email ?? string.Empty, subject, htmlBody);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send registration email to {Email}", newUser.Email);
                }
            });

            var userDto = _mapper.Map<UserDto>(newUser);

            return new CreateUserResponseDto
            {
                User = userDto,
                ConfirmationLink = confirmationLink
            };
        }

        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser is null)
            {
                return false;
            }

            // Check if email is changing and if the new email is already in use by another user
            if (!string.Equals(existingUser.Email, updateUserDto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var userWithEmail = await _userManager.FindByEmailAsync(updateUserDto.Email);
                if (userWithEmail != null && userWithEmail.Id != existingUser.Id)
                {
                    throw new ApplicationValidationException(Messages.Error.EmailAlreadyExists);
                }
            }

            _mapper.Map(updateUserDto, existingUser);

            // Update Identity normalized fields since email has changed
            if (existingUser.Email != null)
            {
                existingUser.NormalizedEmail = existingUser.Email.ToUpperInvariant();
                existingUser.UserName = existingUser.Email;
                existingUser.NormalizedUserName = existingUser.Email.ToUpperInvariant();
            }

            _userRepository.Update(existingUser);
            await _userRepository.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser is null)
            {
                return false;
            }

            _userRepository.Delete(existingUser);
            await _userRepository.SaveAsync();
            return true;
        }
    }
}