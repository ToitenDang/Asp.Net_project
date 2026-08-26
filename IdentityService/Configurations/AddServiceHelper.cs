using FluentValidation;
using IdentityService.Configurations.OptionsPatternModels;
using IdentityService.Repositories;
using IdentityService.Repositories.IRepository;
using IdentityService.Services.IService;
using IdentityService.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace IdentityService.Configurations
{
    public static class AddServiceHelper
    {
        public static void AddServices(this IServiceCollection services)
        {
            // Add your services here, for example:
            //services.AddScoped<IAuthRepository, AuthRepository>();
            // Đăng ký tự động tầng Repositories
            services.Scan(scan => scan
                            .FromAssemblyOf<ITokenRepository>()
                            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
                            .AsImplementedInterfaces()
                            .WithScopedLifetime());
            // Đăng ký tự động tầng Services
            services.Scan(scan => scan
                            .FromAssemblyOf<IAuthService>()
                            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
                            .AsImplementedInterfaces()
                            .WithScopedLifetime());

            // Đăng ký UnitOfWork
            services.Scan(scan => scan
                            .FromAssemblyOf<IUnitOfWork>()
                            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("UnitOfWork")))
                            .AsImplementedInterfaces()
                            .WithScopedLifetime());
        }

        public static void AddValidator(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<UserRequestValidator>();
        }

        public static void AddOptionsPattern(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DataSeedingSettings>(configuration.GetSection("DataSeeding"));
        }

        public static void AddJWT(this IServiceCollection services, IConfiguration configuration)
        {
            var jwt = configuration.GetSection("Jwt");

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwt["Issuer"],
                        ValidAudience = jwt["Audience"],

                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwt["Key"]!))
                    };
                });
        }

        public static void AddCustomAuthorization(this IServiceCollection services)
        {
            services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, IdentityService.Authorization.PermissionPolicyProvider>();
            services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, IdentityService.Authorization.PermissionHandler>();
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomResponseAuthorizationMiddleware>();
        }
    }
}