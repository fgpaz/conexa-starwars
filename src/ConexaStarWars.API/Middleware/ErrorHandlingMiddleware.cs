using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace ConexaStarWars.API.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no manejado en la aplicación");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Error de validación";
                response.Details = validationEx.Errors;
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = "No autorizado";
                break;

            case ForbiddenException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Message = "Acceso prohibido";
                break;

            case NotFoundException notFoundEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = notFoundEx.Message;
                break;

            case InvalidOperationException invalidOpEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = invalidOpEx.Message;
                break;

            case ArgumentException argEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = argEx.Message;
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "Error interno del servidor";
                break;
        }

        context.Response.StatusCode = response.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }
    public string TraceId { get; set; } = Activity.Current?.Id ?? string.Empty;
}

// Excepciones personalizadas
public class ValidationException(object errors) : Exception("Error de validación")
{
    public object Errors { get; } = errors;
}

public class NotFoundException(string message) : Exception(message);

public class ForbiddenException(string message = "Acceso prohibido") : Exception(message);