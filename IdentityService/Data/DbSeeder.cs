using IdentityService.Entities;
using IdentityService.Models.OptionsPatternModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IdentityService.Data
{
    public static class DbSeeder
    {
        // Cố định ID để seed luôn ổn định, không tạo mới mỗi lần chạy
        private static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        private static readonly Guid UserRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid AdminUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var services = scope.ServiceProvider;

            var settings = services.GetRequiredService<IOptions<DataSeedingSettings>>().Value;
            var accounts = settings.AdminAccount;
            var roleSettings = settings.RoleSettings;

            await SeedRolesAsync(context, roleSettings);
            await SeedAdminUserAsync(context, accounts);
        }

        // ─────────────────────────────────────────────────────────
        // SEED ROLES
        // ─────────────────────────────────────────────────────────
        private static async Task SeedRolesAsync(AppDbContext context, RoleSettings roleSettings)
        {
            var rolesToSeed = new List<RoleEntity>
            {
                new RoleEntity
                {
                    Id         = AdminRoleId,
                    Name       = roleSettings.RoleAdminName,
                    RoleCode   = roleSettings.RoleAdminCode,
                    IsActive   = true,
                    CreatedAt  = DateTime.UtcNow
                },
                new RoleEntity
                {
                    Id         = UserRoleId,
                    Name       = roleSettings.RoleUserName,
                    RoleCode   = roleSettings.RoleUserCode,
                    IsActive   = true,
                    CreatedAt  = DateTime.UtcNow
                }
            };

            foreach (var role in rolesToSeed)
            {
                // Chỉ thêm nếu chưa tồn tại (idempotent)
                if (!await context.Roles.AnyAsync(r => r.Id == role.Id))
                {
                    await context.Roles.AddAsync(role);
                }
            }

            await context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────
        // SEED ADMIN USER
        // ─────────────────────────────────────────────────────────
        private static async Task SeedAdminUserAsync(AppDbContext context, AdminAccount account)
        {
            // Nếu đã tồn tại, bỏ qua
            if (await context.Users.AnyAsync(u => u.Id == AdminUserId))
                return;

            var adminUser = new UserEntity
            {
                Id = AdminUserId,
                Name = account.Name,
                UserName = account.UserName,
                Email = account.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Hash password
            var hasher = new PasswordHasher<UserEntity>();
            adminUser.Password = hasher.HashPassword(adminUser, account.Password);

            await context.Users.AddAsync(adminUser);

            // Gán Role Admin cho tài khoản này
            var userRole = new UserRoleEntity
            {
                Id = Guid.NewGuid(),
                UserId = AdminUserId,
                RoleId = AdminRoleId,
                CreatedAt = DateTime.UtcNow
            };

            await context.UserRoles.AddAsync(userRole);

            await context.SaveChangesAsync();
        }
    }
}