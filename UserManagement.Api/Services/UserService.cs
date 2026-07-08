using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _repository.User.GetAllUserAsync(trackChanges: false);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _repository.User.GetUserByIdAsync(id, trackChanges: false);
            return user is null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var newUser = new User
            {
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                Email = createUserDto.Email,
                UserName = createUserDto.Email // Set UserName to Email for Identity framework
            };

            var result = await _userManager.CreateAsync(newUser, createUserDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"User creation failed: {errors}");
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

            existingUser.FirstName = updateUserDto.FirstName;
            existingUser.LastName = updateUserDto.LastName;
            existingUser.Email = updateUserDto.Email;
            existingUser.UserName = updateUserDto.Email; // Keep UserName in sync with Email

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