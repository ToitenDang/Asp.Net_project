using AutoMapper;
using IdentityService.Entities;
using IdentityService.Models.Request;
using IdentityService.Models.Response;
using IdentityService.Repositories;
using IdentityService.Repositories.IRepository;
using IdentityService.Services.IService;
using IdentityService.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IRoleRepository _roleRepository;

        public RoleService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IRoleRepository roleRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _roleRepository = roleRepository;
        }

        public async Task<ResultResponse> CreateRole(RoleRequest request)
        {
            var res = new ResultResponse();

            var userId = _currentUserService?.User?.UserId;

            var role = await _roleRepository.RoleNameExisted(request.Name.Trim().ToUpper());

            if (role != null)
            {
                if (role.IsDelete)
                {
                    return ResultResponse.Fail("Permission existed");
                }

                role.IsDelete = true;
                role.CreatedAt = DateTime.UtcNow;
                role.CreatedBy = userId;
            }

            var newPer = new RoleEntity
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim().ToUpper(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _roleRepository.AddAsync(newPer);

            if (request.PermissionIds != null && request.PermissionIds.Any())
            {
                await _roleRepository.UpdateRolePermissionsAsync(newPer.Id, request.PermissionIds, userId);
            }

            await _unitOfWork.SaveChangesAsync();

            res.Message = "Role created successfully.";
            return res;
        }

        public async Task<ResultResponse> DeleteRole(Guid roleId)
        {
            var res = new ResultResponse();

            var role = await _roleRepository.GetByIdAsync(roleId);

            _roleRepository.Delete(role);

            await _unitOfWork.SaveChangesAsync();

            res.Message = "Deleted Succeed!";
            return res;
        }

        public async Task<ResultResponse> GetAllRoles()
        {
            var res = new ResultResponse<List<RoleResponse>>();

            var roles = await _roleRepository.GetAllAsync();

            res.Message = "List roles";
            res.Data = _mapper.Map<List<RoleResponse>>(roles);

            return res;
        }

        public async Task<ResultResponse> GetById(Guid Id)
        {
            var res = new ResultResponse<PermissionResponse>();

            var roles = await _roleRepository.GetByIdAsync(Id);
            var perDTO = _mapper.Map<PermissionResponse>(roles);
            res.Message = $"Permission by id: {Id}";
            res.Data = perDTO;
            return res;
        }

        public async Task<ResultResponse> UpdateRole(Guid roleId, RoleRequest request)
        {
            var res = new ResultResponse();
            var userId = _currentUserService?.User?.UserId;

            var role = await _roleRepository.GetByIdAsync(roleId);

            if (role == null)
            {
                return new ResultResponse(false, "Permisson not found!");
            }

            role.Name = request.Name;
            role.IsActive = request.IsActive;
            role.UpdatedAt = DateTime.UtcNow;
            role.UpdatedBy = userId;

            _roleRepository.Update(role);

            // Sync role permissions: delete old, insert new
            await _roleRepository.UpdateRolePermissionsAsync(roleId, request.PermissionIds, userId);

            await _unitOfWork.SaveChangesAsync();

            res.Message = "Update succeed";

            return res;
        }
    }
}