using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Data;
using UserManagement.Api.Dtos;
using UserManagement.Api.Models;

namespace UserManagement.Api.Services
{
    /// <summary>
    /// Implements User business logic, backed by the database via AppDbContext.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user is null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var newUser = _mapper.Map<User>(createUserDto);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync(); // Id is populated here by the database (IDENTITY column)

            return _mapper.Map<UserDto>(newUser);
        }

        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var existingUser = await _context.Users.FindAsync(id);

            if (existingUser is null)
            {
                return false;
            }

            existingUser.FirstName = updateUserDto.FirstName;
            existingUser.LastName = updateUserDto.LastName;
            existingUser.Email = updateUserDto.Email;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var existingUser = await _context.Users.FindAsync(id);

            if (existingUser is null)
            {
                return false;
            }

            _context.Users.Remove(existingUser);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}