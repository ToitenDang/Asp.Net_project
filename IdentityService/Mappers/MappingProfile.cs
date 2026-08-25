using AutoMapper;
using IdentityService.Entities;
using IdentityService.Models.Request;
using IdentityService.Models.Response;

namespace IdentityService.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserEntity, UserResponse>().ReverseMap();
            CreateMap<PermissionEntity, PermissionResponse>().ReverseMap();
            CreateMap<RoleEntity, RoleResponse>().ReverseMap();

            CreateMap<UserRequest, UserEntity>().ReverseMap();
            //CreateMap<CreateUserDto, User>();
            //CreateMap<UpdateUserDto, User>();
        }
    }
}