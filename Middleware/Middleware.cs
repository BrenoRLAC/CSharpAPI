
using System.Net;
using System.Text.Json;

namespace API.Middleware
{
    public class Middleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            var result = JsonSerializer.Serialize(new { error = "Um erro ocorreu enquanto processavamos a requisição." });

            if (exception is ArgumentException) code = HttpStatusCode.BadRequest;
            if (exception is KeyNotFoundException) code = HttpStatusCode.NotFound;

            if (exception is InvalidOperationException)
            {
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new { error = "Herói inativo ou inexistente, verifique o id informado." });
            }


            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);          
        }
    }


    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Middleware>();
        }
    }
}
