using AutoMapper;
using IdentityService.Configurations;
using IdentityService.Data;
using IdentityService.Mappers;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using IdentityService.Exceptions;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration["RedisServer:Redis"] ?? "localhost:6379, password=idenRedisPass@-@"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));
builder.Services.AddValidator();
builder.Services.AddServices();
//builder.Services.AddValidatorsFromAssemblyContaining<UserRequestValidator>();
builder.Services.AddJWT(builder.Configuration);
builder.Services.AddCustomAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Seed dữ liệu khởi tạo khi ứng dụng chạy lần đầu
await IdentityService.Data.DbSeeder.SeedAsync(app.Services);

app.Run();