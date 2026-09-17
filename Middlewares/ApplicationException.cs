using System.Net;
using comandaAPI.Middlewares;
using comandaAPI.Domain.Exception;
using Microsoft.AspNetCore.Diagnostics;

namespace comandaAPI.Middlewares;

public static class ApplicationException
{
    public static void ConfigureExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.ContentType = "application/json";
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    var exception = contextFeature.Error;
                    var statusCode = exception switch
                    {
                        ValidationException => HttpStatusCode.BadRequest,
                        ResourceNotFoundException => HttpStatusCode.NotFound,
                        BusinessException => HttpStatusCode.Conflict,
                        _ => HttpStatusCode.InternalServerError
                    };
                    context.Response.StatusCode = (int)statusCode;
                    var errorDetails = new ErrorDetails(
                        (int)statusCode,
                        exception.Message,
                        context.TraceIdentifier,
                        null);
                    await context.Response.WriteAsync(errorDetails.ToString());
                }
            });
        });
    }
}