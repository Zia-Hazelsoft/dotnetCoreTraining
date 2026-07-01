using AutoMapper;
using UserManagement.Api.Dtos;
using UserManagement.Api.Models;

namespace UserManagement.Api.Services
{
    /// <summary>
    /// Implements User business logic. Currently backed by an in-memory list;
    /// will be replaced with EF Core persistence in Week 3.
    /// </summary>
    public class UserService : IUserService
    {
        // In-memory "database" for now.
        private static readonly List<User> _users = new()
        {
            new User { Id = 1, FirstName = "Ali", LastName = "Khan", Email = "ali.khan@example.com" },
            new User { Id = 2, FirstName = "Sara", LastName = "Ahmed", Email = "sara.ahmed@example.com" },
            new User { Id = 3, FirstName = "Bilal", LastName = "Hassan", Email = "bilal.hassan@example.com" }
        };

        private static int _nextId = 4;

        private readonly IMapper _mapper;

        public UserService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public IEnumerable<UserDto> GetAllUsers()
        {
            return _mapper.Map<IEnumerable<UserDto>>(_users);
        }

        public UserDto? GetUserById(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            return user is null ? null : _mapper.Map<UserDto>(user);
        }

        public UserDto CreateUser(CreateUserDto createUserDto)
        {
            var newUser = _mapper.Map<User>(createUserDto);
            newUser.Id = _nextId++; // server-generated id

            _users.Add(newUser);

            return _mapper.Map<UserDto>(newUser);
        }

        public bool UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            var existingUser = _users.FirstOrDefault(u => u.Id == id);

            if (existingUser is null)
            {
                return false;
            }

            existingUser.FirstName = updateUserDto.FirstName;
            existingUser.LastName = updateUserDto.LastName;
            existingUser.Email = updateUserDto.Email;

            return true;
        }

        public bool DeleteUser(int id)
        {
            var existingUser = _users.FirstOrDefault(u => u.Id == id);

            if (existingUser is null)
            {
                return false;
            }

            _users.Remove(existingUser);
            return true;
        }
    }
}