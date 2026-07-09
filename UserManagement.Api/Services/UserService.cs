using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Common;
using UserManagement.Api.Constants;
using UserManagement.Api.Dtos;
using UserManagement.Api.Models;
using UserManagement.Api.Repositories;

namespace UserManagement.Api.Services
{
    /// <summary>
    /// Implements User business logic, backed by the IRepositoryWrapper and ASP.NET Core Identity.
    /// </summary>
    public class UserService(IRepositoryWrapper repository, UserManager<User> userManager, IMapper mapper) : IUserService
    {
        private readonly IRepositoryWrapper _repository = repository;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IMapper _mapper = mapper;

        public async Task<PaginatedResponseDto<UserDto>> GetUsersAsync(UserParameters userParameters)
        {
            var pagedUsers = await _repository.User.GetUsersAsync(userParameters, trackChanges: false);
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(pagedUsers);

            return new PaginatedResponseDto<UserDto>
            {
                Items = userDtos,
                MetaData = pagedUsers.MetaData
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _repository.User.GetUserByIdAsync(id, trackChanges: false);
            return user is null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var newUser = _mapper.Map<User>(createUserDto);

            var result = await _userManager.CreateAsync(newUser, createUserDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                throw new ApplicationValidationException(Messages.Error.UserCreationFailed, errors);
            }

            return _mapper.Map<UserDto>(newUser);
        }

        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var existingUser = await _repository.User.GetUserByIdAsync(id, trackChanges: true);
            if (existingUser is null)
            {
                return false;
            }

            _mapper.Map(updateUserDto, existingUser);

            _repository.User.Update(existingUser);
            await _repository.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var existingUser = await _repository.User.GetUserByIdAsync(id, trackChanges: true);
            if (existingUser is null)
            {
                return false;
            }

            _repository.User.Delete(existingUser);
            await _repository.SaveAsync();
            return true;
        }
    }
}