using AutoMapper;
using FluentValidation.Results;
using IdentityService.Entities;
using IdentityService.Models.Request;
using IdentityService.Models.Response;
using IdentityService.Repositories.IRepository;
using IdentityService.Services.IService;
using IdentityService.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPermissionRepository _permissionRepository;

        public PermissionService(ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IMapper mapper, IPermissionRepository permissionRepository)
        {
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _permissionRepository = permissionRepository;
        }

        public async Task<ResultResponse> CreatePermission(PermissionRequest request)
        {
            var res = new ResultResponse();

            var userId = _currentUserService?.User?.UserId;

            var permission = await _permissionRepository.PermissionNameExisted(request.Name.Trim().ToUpper());

            if (permission != null)
            {
                if (permission.IsDelete)
                {
                    return ResultResponse.Fail("Permission existed");
                }

                permission.IsDelete = true;
                permission.CreatedAt = DateTime.UtcNow;
                permission.CreatedBy = userId;
            }

            var newPer = new PermissionEntity
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim().ToUpper(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _permissionRepository.AddAsync(newPer);
            await _unitOfWork.SaveChangesAsync();

            res.Message = "Permission created successfully.";
            return res;
        }

        public async Task<ResultResponse> UpdatePermission(Guid permissonId, PermissionRequest request)
        {
            var res = new ResultResponse();
            var userId = _currentUserService?.User?.UserId;

            var permission = await _permissionRepository.GetByIdAsync(permissonId);

            if (permission == null)
            {
                return new ResultResponse(false, "Permisson not found!");
            }

            permission.Name = request.Name;
            permission.IsActive = request.IsActive;
            permission.UpdatedAt = DateTime.UtcNow;
            permission.UpdatedBy = userId;

            _permissionRepository.Update(permission);
            await _unitOfWork.SaveChangesAsync();

            res.Message = "Update succeed";

            return res;
        }

        public async Task<ResultResponse> GetAllPermissions()
        {
            var res = new ResultResponse<List<PermissionResponse>>();

            var permissions = await _permissionRepository.GetAllAsync();
            var perDTO = _mapper.Map<List<PermissionResponse>>(permissions);
            res.Message = "List permission";
            res.Data = perDTO;
            return res;
        }

        public async Task<ResultResponse> GetPermissionById(Guid Id)
        {
            var res = new ResultResponse<PermissionResponse>();

            var permissions = await _permissionRepository.GetByIdAsync(Id);
            var perDTO = _mapper.Map<PermissionResponse>(permissions);
            res.Message = $"Permission by id: {Id}";
            res.Data = perDTO;
            return res;
        }

        public async Task<ResultResponse> DeletePermisson(Guid permissionId)
        {
            var res = new ResultResponse();

            var permission = await _permissionRepository.GetByIdAsync(permissionId);

            _permissionRepository.Delete(permission);

            await _unitOfWork.SaveChangesAsync();

            res.Message = "Deleted Succeed!";
            return res;
        }
    }
}