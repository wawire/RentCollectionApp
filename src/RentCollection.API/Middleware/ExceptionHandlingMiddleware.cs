using System.Net;
using System.Text.Json;
using RentCollection.Application.Common.Exceptions;

namespace RentCollection.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            Message = "An error occurred processing your request.",
            Errors = new List<string>()
        };

        switch (exception)
        {
            case NotFoundException notFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new
                {
                    Message = notFoundException.Message,
                    Errors = new List<string>()
                };
                break;

            case ValidationException validationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    Message = "Validation errors occurred.",
                    Errors = validationException.Errors.SelectMany(e => e.Value).ToList()
                };
                break;

            case BadRequestException badRequestException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    Message = badRequestException.Message,
                    Errors = new List<string>()
                };
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                _logger.LogError(exception, "An unhandled exception occurred");
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }
}
