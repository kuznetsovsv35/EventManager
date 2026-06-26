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

        ProblemDetailsBuilder<ProblemDetails> detailsBuilder = exception switch
        {
            ValidationException ve =>ProblemDetailsFactory.VakidationProblem(ve.ValidationResult, ve.Message),
            _ => ProblemDetailsFactory.InternalServiceError(exception.Message)
        };
        
        detailsBuilder.AddInstance(request.Path);
        response.StatusCode = detailsBuilder.Problem.Status ?? MapStatusCodeToException(exception);

        response.ContentType = MediaTypeNames.Application.ProblemJson;
        await response.WriteAsJsonAsync(detailsBuilder.Problem);
    }
}