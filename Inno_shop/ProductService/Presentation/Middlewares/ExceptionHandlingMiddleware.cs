using System.Net;
using System.Text.Json;
using ProductService.Domain.Exceptions;
namespace ProductService.Presentation.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionMessageAsync(context, ex);
        }
    }

    private static Task HandleExceptionMessageAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        int statusCode = (int)HttpStatusCode.InternalServerError;

        switch (exception)
        {
            case NotFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                break;
        }

        var result = JsonSerializer.Serialize(new
        {
            StatusCode = statusCode,
            ErrorMessage = exception.Message,
        });

        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsync(result);
    }
}
