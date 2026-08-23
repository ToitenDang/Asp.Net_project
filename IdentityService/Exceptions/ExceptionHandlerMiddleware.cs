using FluentValidation;
using IdentityService.Models.Response;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IdentityService.Exceptions
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                var errors = ex.Errors
                    .Select(x => new
                    {
                        Field = x.PropertyName,
                        Message = x.ErrorMessage
                    })
                    .ToList();

                await context.Response.WriteAsJsonAsync(
                    ResultResponse.Fail(
                        "Data invalid",
                        errors
                    )
                );
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status200OK;

                var errors = ex.Message;

                await context.Response.WriteAsJsonAsync(
                    ResultResponse.Fail(
                        "Data invalid",
                        errors
                    )
                );
            }
        }
    }
}