using AutoMapper;
using UserManagement.Api.Dtos;
using UserManagement.Api.Models;

namespace UserManagement.Api.Mappings
{
    /// <summary>
    /// Defines how AutoMapper converts between domain models and DTOs.
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();
        }
    }
}