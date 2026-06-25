using System.Net.Mime;
using HttpContent = Microsoft.AspNetCore.Http.HttpContext;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.Infrastructure;

public class ErrorHandler(RequestDelegate next, ILogger<ErrorHandler> logger)
{
    public async Task Invoke(HttpContent content)
    {
        try
        {
            await next(content);
        }
        catch (Exception ex)
        {
            LogException(content.Request, ex);

            if (content.Response.HasStarted)
            {
                return;
            }

            content.Response.Clear();
            await CreateErrorResponse(content, ex);
        }
    }

    void LogException(HttpRequest request, Exception exception)
        => logger.LogError(exception, "Необработанное исключение: Method: {Method}, Path: {Path}", request.Method, request.Path);

    static int MapStatusCodeToException(Exception exception)
        => exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError,
        };

    static async Task CreateErrorResponse(HttpContext context, Exception exception)
    {
        var request = context.Request;
        var response = context.Response;

        ProblemDetails details = exception switch
        {
            ValidationException ve => await CreateValidationProblemDetails(ve),
            _ => await CreateProblemDetails(exception),
        };
        
        details.Instance = request.Path;
        response.StatusCode = details.Status ?? MapStatusCodeToException(exception);

        response.ContentType = MediaTypeNames.Application.Json;
        await response.WriteAsJsonAsync(details);
    }

    static async Task<ProblemDetails> CreateProblemDetails(Exception exception)
        => new()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Внутренняя ошибка сервера",
            Detail = "Произошла непредвиденная ошибка. Попробуйте повторить запрос позже.",            
            Status = StatusCodes.Status500InternalServerError,
        };

    static async Task<ValidationProblemDetails> CreateValidationProblemDetails(ValidationException exception)
    {
        var details = new ValidationProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807",
            Title = "Ошибка валидации запроса",
            Detail = "Один или несколько параметров содержат недопустимые значения.",
            Status = StatusCodes.Status400BadRequest,
        };

        foreach (var member in exception.ValidationResult.MemberNames)
        {
            details.Errors[member] = [exception.ValidationResult.ErrorMessage ?? exception.Message];
        }
        return details;
    }
}