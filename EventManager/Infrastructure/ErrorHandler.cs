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
            EventNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError,
        };

    static async Task CreateErrorResponse(HttpContent context, Exception exception)
    {
        var request = context.Request;
        var response = context.Response;

        ProblemDetailsBuilder<ProblemDetails> detailsBuilder = exception switch
        {
            ValidationException ve => ProblemDetailsFactory.ValidationProblem(ve.ValidationResult, ve.Message),
            
            PaginatorParamException pe => ProblemDetailsFactory.ValidationProblem(
                new ValidationResult(
                    pe.Message, (pe.ParamName is string pn) ? new[] { pn } : null),
                pe.Message, $"параметр: {pe.ParamName}, значение: {pe.ParamValue}"),
            
            EventNotFoundException eventNotFound => ProblemDetailsFactory.NotFound($"{eventNotFound.Message}: (ID={eventNotFound.EventId})."),

            _ => ProblemDetailsFactory.InternalServiceError(exception.Message)
        };

        detailsBuilder.AddInstance(request.Path);
        response.StatusCode = detailsBuilder.Problem.Status ?? MapStatusCodeToException(exception);

        response.ContentType = MediaTypeNames.Application.ProblemJson;
        await response.WriteAsJsonAsync(detailsBuilder.Problem);
    }
}