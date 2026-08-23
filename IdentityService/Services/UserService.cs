using AutoMapper;
using IdentityService.Models.Response;
using IdentityService.Repositories.IRepository;
using IdentityService.Services.IService;

namespace IdentityService.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ResultResponse> GetAllUsers()
        {
            var result = new ResultResponse<List<UserResponse>>();

            var users = await _userRepository.GetAllUsers();

            result.Success = true;
            result.Message = "Get all users successfully";
            result.Data = _mapper.Map<List<UserResponse>>(users);

            return result;
        }
    }
}