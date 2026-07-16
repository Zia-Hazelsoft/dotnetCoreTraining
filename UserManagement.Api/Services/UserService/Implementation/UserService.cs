using AutoMapper;
using Microsoft.AspNetCore.Identity;
using UserManagement.Api.Common;
using UserManagement.Api.Constants;
using UserManagement.Api.Dtos;
using UserManagement.Api.Models;
using UserManagement.Api.Repositories;
using UserManagement.Api.Services.EmailService;

namespace UserManagement.Api.Services.UserService.Implementation
{
    /// <summary>
    /// Implements User business logic, backed by the IRepositoryBase<User> and ASP.NET Core Identity.
    /// </summary>
    public class UserService(
        IRepositoryBase<User> userRepository,
        UserManager<User> userManager,
        IMapper mapper,
        ILogger<UserService> logger,
        IEmailService emailService) : IUserService
    {
        private readonly IRepositoryBase<User> _userRepository = userRepository;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<UserService> _logger = logger;
        private readonly IEmailService _emailService = emailService;

        public async Task<PaginatedResponseDto<UserDto>> GetUsersAsync(UserParameters userParameters)
        {
            PagedList<User> pagedUsers = await _userRepository.GetPagedAsync(
                userParameters.PageNumber,
                userParameters.PageSize,
                userParameters.Filters,
                userParameters.OrderBy,
                nameof(User.Id),
                trackChanges: false);

            IEnumerable<UserDto> userDtos = _mapper.Map<IEnumerable<UserDto>>(pagedUsers);

            return new PaginatedResponseDto<UserDto>
            {
                Items = userDtos,
                MetaData = pagedUsers.MetaData
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            User? user = await _userRepository.GetByIdAsync(id);
            return user is null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<CreateUserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            User newUser = _mapper.Map<User>(createUserDto);

            IdentityResult result = await _userManager.CreateAsync(newUser);
            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ArgumentException(errors);
            }

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            
            string confirmationLink = await _emailService.SendActivationEmailAsync(newUser, token);

            UserDto userDto = _mapper.Map<UserDto>(newUser);

            return new CreateUserResponseDto
            {
                User = userDto
            };
        }

        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            User? existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser is null)
            {
                return false;
            }

            _mapper.Map(updateUserDto, existingUser);

            _userRepository.Update(existingUser);
            await _userRepository.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            User? existingUser = await _userRepository.GetByIdAsync(id);
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