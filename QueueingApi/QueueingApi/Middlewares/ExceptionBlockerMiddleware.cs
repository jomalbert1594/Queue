using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace QueueingApi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ExceptionBlockerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionBlockerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception e)
            {
                // Catch the concurrency exception and return it to the user
                if (e is DbUpdateConcurrencyException exception)
                    e = exception;

                if (httpContext.Response.HasStarted)
                {
                    throw;
                }

                var allExceptions = "";

                do
                {
                    allExceptions = allExceptions + e.Message + "\n";
                    e = e.InnerException;
                } while (e != null);

                httpContext.Response.Clear();
                httpContext.Response.StatusCode = 400; // Bad Request

                await httpContext.Response.WriteAsync(allExceptions);
            }
            
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ExceptionBlockerMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionBlockerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionBlockerMiddleware>();
        }
    }
}
